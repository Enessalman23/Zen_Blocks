using UnityEngine;

public class BottomPanelGrid : MonoBehaviour
{
    [Header("Hücre Ayarları")]
    public GameObject cellPrefab;   
    public Vector3 cellSize = new Vector3(0.7f, 0.7f, 1f); 

    [Header("Otomatik Oluşturma")]
    public bool regenerateOnStart = true; 

    private void Start()
    {
        if (regenerateOnStart)
        {
            GenerateSingleCell();
        }
    }

    /// <summary>
    /// Sadece tek bir hücre oluşturur.
    /// </summary>
    public void GenerateSingleCell()
    {
        if (cellPrefab == null) return;

        // Eski objeleri temizle
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Objeyi bu scriptin olduğu tam noktada oluştur
        GameObject cell = Instantiate(cellPrefab, transform);
        
        // Pozisyonu tam merkeze (0,0,0) sıfırla, böylece ana objeyi nereye çekersen o da oraya gider
        cell.transform.localPosition = Vector3.zero;
        cell.transform.localScale = cellSize; 
        
        cell.name = "Single_Background_Cell";
    }
}