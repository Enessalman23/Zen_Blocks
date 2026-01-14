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
    public float spacing = 1.1f; 

    [Header("Önizleme Renkleri")]
    public Color ghostColor = new Color(1f, 1f, 1f, 0.5f); 
    public Color highlightColor = new Color(0f, 1f, 0f, 0.8f); 

    private bool[,] isOccupied;
    private GameObject[,] blockMatrix; 
    private SpriteRenderer[,] cellRenderers;
    private Color[,] originalCellColors;
    public GameObject explosionPrefab; // Hazırladığın Particle System Prefab'ı
    [Header("Efektler")]
    public GameObject blastEffectPrefab;

    void Awake()
    {
        if (instance == null) instance = this;
        DOTween.Init();
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
                cell.transform.localScale = Vector3.one * 0.7f;
                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                cellRenderers[x, y] = sr;
                originalCellColors[x, y] = sr.color; 
            }
        }
    }
    void PlayExplosionEffect(Vector3 position, bool isHorizontal)
    {
        GameObject effect = Instantiate(explosionPrefab, position, Quaternion.identity);
    
        // Eğer dikey patlamaysa efekti 90 derece döndür
        if (!isHorizontal)
        {
            effect.transform.rotation = Quaternion.Euler(0, 0, 90);
        }

        Destroy(effect, 1f); // 1 saniye sonra objeyi temizle
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

        for (int y = 0; y < height; y++)
        {
            bool rowFull = true;
            for (int x = 0; x < width; x++) if (!tempOccupied[x, y]) { rowFull = false; break; }
            if (rowFull) for (int x = 0; x < width; x++) cellRenderers[x, y].color = highlightColor;
        }

        for (int x = 0; x < width; x++)
        {
            bool colFull = true;
            for (int y = 0; y < height; y++) if (!tempOccupied[x, y]) { colFull = false; break; }
            if (colFull) for (int y = 0; y < height; y++) cellRenderers[x, y].color = highlightColor;
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
            foreach (int y in rowsToClear)
                for (int x = 0; x < width; x++) ClearSpecificBlock(x, y);

            foreach (int x in columnsToClear)
                for (int y = 0; y < height; y++) ClearSpecificBlock(x, y);

            if (ScreenShake.instance) ScreenShake.instance.Shake(0.2f, 0.15f);
        }

        GameManager.instance.ProcessMatch(totalLines);
        
        CancelInvoke("TriggerGameOverCheck");
        Invoke("TriggerGameOverCheck", 0.6f); // Animasyonlar için ideal bekleme
    }

    void TriggerGameOverCheck()
    {
        if (GameOverManager.instance != null) GameOverManager.instance.CheckGameOver();
    }

    void ClearSpecificBlock(int x, int y)
    {
        if (blockMatrix[x, y] != null)
        {
            GameObject block = blockMatrix[x, y];
        
            // --- RENK HEDEFİ TETİKLEME ---
            if (LevelManager.instance != null)
            {
                SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                    LevelManager.instance.OnBlockDestroyed(sr.sprite.name);
            }

            blockMatrix[x, y] = null;
            isOccupied[x, y] = false;
            SpawnBlastEffect(block);
            block.transform.DOKill();
            block.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => Destroy(block));
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
    
    public void PlaceInitialBlock(int x, int y, GameObject blockPrefab)
    {
        // Hücrenin dünya üzerindeki pozisyonunu hesapla
        float offsetX = (width - 1) * spacing / 2f;
        float offsetY = (height - 1) * spacing / 2f;
        Vector3 pos = new Vector3(x * spacing - offsetX, y * spacing - offsetY, 0);

        // Bloğu oluştur
        GameObject newBlock = Instantiate(blockPrefab, pos, Quaternion.identity);
        newBlock.transform.SetParent(this.transform); // GridManager'ın altına koy
    
        // Bloğun boyutunu hücreye uygun ayarla (Gerekirse)
        newBlock.transform.localScale = Vector3.one * 0.8f; 

        // Grid verisini güncelle (Burası kritik, yoksa üstüne blok koyulabilir)
        blockMatrix[x, y] = newBlock;
        isOccupied[x, y] = true;
    }

    public Vector2Int WorldToGridPos(Vector3 worldPos)
    {
        float offsetX = (width - 1) * spacing / 2f;
        float offsetY = (height - 1) * spacing / 2f;
        int x = Mathf.RoundToInt((worldPos.x + offsetX) / spacing);
        int y = Mathf.RoundToInt((worldPos.y + offsetY) / spacing);
        return new Vector2Int(x, y);
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
        
        // --- RENK ÇÖZÜMÜ ---
        SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
        Color neonColor = Color.white;
        if (sr != null && sr.sprite != null)
        {
            string sName = sr.sprite.name;
            if (sName.Contains("14_49_34_0")) neonColor = new Color(0f, 0.7f, 1f, 1f); 
            else if (sName.Contains("14_59_14_0")) neonColor = new Color(1f, 0.9f, 0f, 1f); 
            else if (sName.Contains("14_57_09_0")) neonColor = new Color(1f, 0.2f, 0.6f, 1f); 
            else if (sName.Contains("15_01_10_0")) neonColor = new Color(0.2f, 1f, 0.3f, 1f); 
            else if (sName.Contains("14_55_20_0")) neonColor = new Color(0.7f, 0.1f, 1f, 1f);
        }

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = neonColor;
        }
        
        effect.transform.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.OutExpo);
        Destroy(effect, 0.8f);
    }
}