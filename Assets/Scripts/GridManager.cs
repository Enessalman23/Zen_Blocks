using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [Header("Izgara Ayarları")]
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;
    public float spacing = 0.75f; 

    [Header("Önizleme Renkleri")]
    public Color ghostColor = new Color(1f, 1f, 1f, 0.5f); 
    public Color highlightColor = new Color(0f, 1f, 0f, 0.8f); // Sabit Yeşil

    private bool[,] isOccupied;
    private GameObject[,] blockMatrix; 
    private SpriteRenderer[,] cellRenderers;
    private Color[,] originalCellColors;
    
    [Header("Efektler")]
    public GameObject blastEffectPrefab;

    void Awake()
    {
        if (instance == null) instance = this;
        isOccupied = new bool[width, height];
        blockMatrix = new GameObject[width, height];
        cellRenderers = new SpriteRenderer[width, height];
        originalCellColors = new Color[width, height];
    }

    void Start() => CreateGrid();

    void CreateGrid()
    {
        float offsetX = (width - 1) * spacing / 2f;
        float offsetY = (height - 1) * spacing / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * spacing - offsetX, y * spacing - offsetY, 0);
                GameObject cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                cell.transform.localScale = Vector3.one * 0.2453707f; 
                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                cellRenderers[x, y] = sr;
                originalCellColors[x, y] = sr.color; 
                cell.name = $"Cell_{x}_{y}";
            }
        }
    }

    public void CheckForMatches()
    {
        if (GameManager.instance == null || !GameManager.instance.isGameStarted) return;

        List<int> rowsToClear = new List<int>();
        List<int> columnsToClear = new List<int>();

        for (int y = 0; y < height; y++)
        {
            bool isFull = true;
            for (int x = 0; x < width; x++) if (!isOccupied[x, y]) isFull = false;
            if (isFull) rowsToClear.Add(y);
        }

        for (int x = 0; x < width; x++)
        {
            bool isFull = true;
            for (int y = 0; y < height; y++) if (!isOccupied[x, y]) isFull = false;
            if (isFull) columnsToClear.Add(x);
        }

        int totalLines = rowsToClear.Count + columnsToClear.Count;

        if (totalLines > 0)
        {
            // --- SES TETİKLEME: BLAST ---
            if(AudioManager.instance != null) 
                AudioManager.instance.PlaySound(AudioManager.instance.blastSound);

            // --- SES TETİKLEME: COMBO (Birden fazla satır patlarsa) ---
            if(totalLines > 1 && AudioManager.instance != null)
                AudioManager.instance.PlaySound(AudioManager.instance.comboSound);

            foreach (int y in rowsToClear)
                for (int x = 0; x < width; x++) ClearSpecificBlock(x, y);

            foreach (int x in columnsToClear)
                for (int y = 0; y < height; y++) ClearSpecificBlock(x, y);

            if (ScreenShake.instance) ScreenShake.instance.Shake(0.2f, 0.15f);
        }

        GameManager.instance.ProcessMatch(totalLines);
        
        // 1 saniyelik garantici bekleme süresi (Güvenli Alan Açılması İçin)
        CancelInvoke("TriggerGameOverCheck");
        Invoke("TriggerGameOverCheck", 0.6f); 
    }

    public Vector2Int WorldToGridPos(Vector3 worldPos)
    {
        float offsetX = (width - 1) * spacing / 2f;
        float offsetY = (height - 1) * spacing / 2f;
        
        // Pivot sorunlarını çözen hassas yuvarlama
        float rawX = (worldPos.x + offsetX + 0.001f) / spacing;
        float rawY = (worldPos.y + offsetY + 0.001f) / spacing;
        
        return new Vector2Int(Mathf.RoundToInt(rawX), Mathf.RoundToInt(rawY));
    }

    void ClearSpecificBlock(int x, int y)
    {
        if (blockMatrix[x, y] != null)
        {
            GameObject block = blockMatrix[x, y];
            if (LevelManager.instance != null)
                LevelManager.instance.OnBlockDestroyed(block.GetComponent<SpriteRenderer>().sprite.name);

            blockMatrix[x, y] = null;
            isOccupied[x, y] = false; // Veriyi anında temizle
            
            SpawnBlastEffect(block);
            block.transform.DOKill();
            block.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() => Destroy(block));
        }
    }

    public void SetCellStatus(int x, int y, bool status, GameObject block = null)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            isOccupied[x, y] = status;
            blockMatrix[x, y] = block;
        }
    }

    public bool IsCellOccupied(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return true;
        return isOccupied[x, y];
    }

    public void ShowPreview(GameObject shapePrefab, Vector3 currentPos)
    {
        ClearPreviews(); 
        List<Vector2Int> targetCells = GetTargetCells(shapePrefab, currentPos);
        if (targetCells == null || !CanPlaceAt(targetCells)) return;

        foreach (Vector2Int pos in targetCells)
            cellRenderers[pos.x, pos.y].color = ghostColor;

        HighlightPotentialMatches(targetCells);
    }

    public void ClearPreviews()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cellRenderers[x, y].color = originalCellColors[x, y];
    }

    private List<Vector2Int> GetTargetCells(GameObject shapePrefab, Vector3 currentPos)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        foreach (Transform child in shapePrefab.transform)
        {
            if (child.gameObject.activeSelf)
            {
                Vector2Int p = WorldToGridPos(child.position);
                if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height) return null;
                cells.Add(p);
            }
        }
        return cells;
    }

    private bool CanPlaceAt(List<Vector2Int> targetCells)
    {
        foreach (Vector2Int pos in targetCells)
            if (isOccupied[pos.x, pos.y]) return false;
        return true;
    }

    private void HighlightPotentialMatches(List<Vector2Int> hypotheticalCells)
{
    bool[,] tempOccupied = (bool[,])isOccupied.Clone();
    foreach (var p in hypotheticalCells) tempOccupied[p.x, p.y] = true;

    // --- SATIR KONTROLÜ ---
    for (int y = 0; y < height; y++)
    {
        bool rowFull = true;
        for (int x = 0; x < width; x++) if (!tempOccupied[x, y]) { rowFull = false; break; }
        if (rowFull) for (int x = 0; x < width; x++) cellRenderers[x, y].color = highlightColor;
    }

    // --- SÜTUN KONTROLÜ (Düzeltildi) ---
    for (int x = 0; x < width; x++)
    {
        bool colFull = true;
        for (int y = 0; y < height; y++) if (!tempOccupied[x, y]) { colFull = false; break; }
        
        // KRİTİK DÜZELTME: cellRenderers[y, x] yerine cellRenderers[x, y] kullanıyoruz
        if (colFull) for (int y = 0; y < height; y++) cellRenderers[x, y].color = highlightColor;
    }
}

    public bool CanAnywherePlace(GameObject shapePrefab)
    {
        List<Vector2Int> relativePositions = new List<Vector2Int>();
        foreach (Transform child in shapePrefab.transform)
        {
            if (child.gameObject.activeSelf)
            {
                int rX = Mathf.RoundToInt(child.localPosition.x / spacing);
                int rY = Mathf.RoundToInt(child.localPosition.y / spacing);
                relativePositions.Add(new Vector2Int(rX, rY));
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool canPlaceHere = true;
                foreach (Vector2Int rel in relativePositions)
                {
                    int tx = x + rel.x;
                    int ty = y + rel.y;
                    if (tx < 0 || tx >= width || ty < 0 || ty >= height || isOccupied[tx, ty])
                    {
                        canPlaceHere = false;
                        break;
                    }
                }
                if (canPlaceHere) return true; 
            }
        }
        return false; 
    }

    void SpawnBlastEffect(GameObject block)
    {
        if (blastEffectPrefab == null || block == null) return;
        GameObject effect = Instantiate(blastEffectPrefab, block.transform.position + Vector3.back, Quaternion.identity);
        SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
        Color neonColor = Color.white;
        if (sr != null && sr.sprite != null)
        {
            string sName = sr.sprite.name;
            if (sName.Contains("Mavi")) neonColor = new Color(0f, 0.7f, 1f, 1f); 
            else if (sName.Contains("Sarı")) neonColor = new Color(1f, 0.9f, 0f, 1f); 
            else if (sName.Contains("Pembe")) neonColor = new Color(1f, 0.2f, 0.6f, 1f); 
            else if (sName.Contains("Yeşil")) neonColor = new Color(0.2f, 1f, 0.3f, 1f); 
            else if (sName.Contains("Mor")) neonColor = new Color(0.7f, 0.1f, 1f, 1f);
        }
        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null) { var main = ps.main; main.startColor = neonColor; }
        effect.transform.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.OutExpo);
        Destroy(effect, 0.8f);
    }

    void TriggerGameOverCheck() { if (GameOverManager.instance != null) GameOverManager.instance.CheckGameOver(); }
}