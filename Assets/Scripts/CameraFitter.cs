using UnityEngine;

public class CameraFitter : MonoBehaviour
{
    private GridManager gridManager;

    void Start()
    {
        gridManager = Object.FindFirstObjectByType<GridManager>();
        if (gridManager != null)
        {
            FitCamera();
        }
    }

    void FitCamera()
    {
        // Izgaranın toplam genişliğini hesapla (Hücre sayısı * Hücreler arası boşluk)
        float gridTotalWidth = gridManager.width * gridManager.spacing;
        
        // Ekranın en/boy oranını al (Örn: 9/16 veya 9/19)
        float screenAspect = (float)Screen.width / (float)Screen.height;

        // Orthographic Size hesaplama: (Genişlik / Ekran Oranı) / 2
        // Sonuna eklediğimiz +2f pay bırakır (UI elemanları için)
        float targetSize = (gridTotalWidth / screenAspect) / 2f + 2.5f;

        Camera.main.orthographicSize = targetSize;
        
        // Kamerayı ızgaranın tam ortasına odakla (Eğer 0,0,0 değilse)
        transform.position = new Vector3(0, 0, -10);
    }
}