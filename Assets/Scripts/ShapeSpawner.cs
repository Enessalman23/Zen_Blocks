using UnityEngine;
using System.Collections.Generic;

public class ShapeSpawner : MonoBehaviour
{
    public static ShapeSpawner instance;

    [Header("Prefab Ayarları")]
    public List<GameObject> shapePrefabs; // Tüm blok çeşitlerini buraya ekle
    public Transform[] spawnPoints;      // Alttaki 3 spawn noktası

    [Header("Aktif Blok Takibi")]
    private List<GameObject> currentActiveShapes = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SpawnNewShapes();
    }

    public void SpawnNewShapes()
    {
        currentActiveShapes.Clear();

        // 3 farklı blok seçimi (Daha önce yaptığımız farklı blok mantığı)
        List<int> selectedIndices = GetThreeDifferentIndices();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int randomIndex = selectedIndices[i];
            GameObject shape = Instantiate(shapePrefabs[randomIndex], spawnPoints[i].position, Quaternion.identity);
            
            // Bloğu takip listesine ekle
            currentActiveShapes.Add(shape);
        }

        // Yeni bloklar geldiğinde hamle olup olmadığını 0.1 saniye sonra kontrol et
        Invoke("CheckGameOverDelayed", 0.1f);
    }

    private List<int> GetThreeDifferentIndices()
    {
        List<int> indices = new List<int>();
        int limit = Mathf.Min(3, shapePrefabs.Count);

        while (indices.Count < limit)
        {
            int r = Random.Range(0, shapePrefabs.Count);
            if (!indices.Contains(r)) indices.Add(r);
        }

        while (indices.Count < 3)
        {
            indices.Add(Random.Range(0, shapePrefabs.Count));
        }

        return indices;
    }

    // Blok yerleşince DraggableShape tarafından çağrılır
    public void ShapePlaced(GameObject shape)
    {
        if (currentActiveShapes.Contains(shape))
        {
            currentActiveShapes.Remove(shape);
        }

        // Eğer alttaki 3 blok da bittiyse yenilerini getir
        if (currentActiveShapes.Count == 0)
        {
            Invoke("SpawnNewShapes", 0.2f);
        }
        else
        {
            // Hala altta blok varsa, kalan bloklar sığıyor mu diye kontrol et
            CheckGameOverDelayed();
        }
    }

    void CheckGameOverDelayed()
    {
        if (GameOverManager.instance != null)
        {
            GameOverManager.instance.CheckGameOver();
        }
    }
}