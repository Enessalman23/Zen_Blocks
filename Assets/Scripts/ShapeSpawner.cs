using UnityEngine;
using System.Collections.Generic;

public class ShapeSpawner : MonoBehaviour
{
    public static ShapeSpawner instance;

    [Header("Prefab Ayarları")]
    public List<GameObject> shapePrefabs; // Tüm blok çeşitlerini buraya ekle
    public Transform[] spawnPoints;      // Alttaki 3 spawn noktası

    [Header("Spawn Ağırlıkları")]
    [Tooltip("Hedef renklerin spawn şansını ne kadar artıracağı (1.0 = normal, 2.0 = 2x daha fazla)")]
    public float targetColorWeightMultiplier = 2f; // Hedef renkler 2x daha fazla çıkar

    [Header("Aktif Blok Takibi")]
    private List<GameObject> currentActiveShapes = new List<GameObject>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SpawnNewShapes();
    }

    public void SpawnNewShapes()
    {
        // Seviye tamamlandıysa yeni blok spawn etme
        if (GameData.currentMode == GameData.Mode.Level && LevelManager.instance != null && LevelManager.instance.IsLevelComplete)
        {
            return;
        }

        if (shapePrefabs == null || shapePrefabs.Count == 0 || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("ShapeSpawner: Prefab listesi veya spawn noktaları eksik!");
            return;
        }

        currentActiveShapes.Clear();

        // 3 farklı blok seçimi (Daha önce yaptığımız farklı blok mantığı)
        List<int> selectedIndices = GetThreeDifferentIndices();

        for (int i = 0; i < spawnPoints.Length && i < selectedIndices.Count; i++)
        {
            if (spawnPoints[i] == null) continue;
            
            int randomIndex = selectedIndices[i];
            if (randomIndex < 0 || randomIndex >= shapePrefabs.Count || shapePrefabs[randomIndex] == null)
            {
                Debug.LogWarning($"ShapeSpawner: Geçersiz prefab index: {randomIndex}");
                continue;
            }
            
            GameObject shape = Instantiate(shapePrefabs[randomIndex], spawnPoints[i].position, Quaternion.identity);
            
            // Bloğu takip listesine ekle
            if (shape != null)
                currentActiveShapes.Add(shape);
        }

        // Bloklar spawn animasyonu bitene kadar bekle (DraggableShape.Start'ta 0.4s animasyon var)
        // Sonra game over kontrolü yap
        Invoke("CheckGameOverDelayed", 0.5f);
    }

    private List<int> GetThreeDifferentIndices()
    {
        // Level modunda hedef renklere göre ağırlıklı seçim yap
        if (GameData.currentMode == GameData.Mode.Level && LevelManager.instance != null)
        {
            return GetWeightedIndices();
        }
        
        // Endless modunda normal rastgele seçim
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

    // Ağırlıklı seçim sistemi - Hedef renklere göre spawn şansını artırır
    private List<int> GetWeightedIndices()
    {
        // Aktif hedefleri al
        List<string> targetColors = new List<string>();
        if (LevelManager.instance != null && LevelManager.instance.activeGoals != null)
        {
            foreach (var goal in LevelManager.instance.activeGoals)
            {
                if (!targetColors.Contains(goal.spriteName))
                    targetColors.Add(goal.spriteName);
            }
        }

        // Eğer hedef yoksa normal seçim yap
        if (targetColors.Count == 0)
        {
            return GetNormalRandomIndices();
        }

        // Her prefab için ağırlık hesapla
        List<float> weights = new List<float>();
        for (int i = 0; i < shapePrefabs.Count; i++)
        {
            float weight = 1.0f; // Normal ağırlık
            
            // Prefab içindeki blokların renklerini kontrol et
            GameObject prefab = shapePrefabs[i];
            if (prefab != null)
            {
                int targetColorCount = 0;
                int totalBlockCount = 0;
                
                foreach (Transform child in prefab.transform)
                {
                    if (child != null && child.gameObject.activeSelf)
                    {
                        totalBlockCount++;
                        SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            string spriteName = sr.sprite.name;
                            // Hedef renklerden birini içeriyor mu kontrol et
                            foreach (string targetColor in targetColors)
                            {
                                if (spriteName.Contains(targetColor))
                                {
                                    targetColorCount++;
                                    break;
                                }
                            }
                        }
                    }
                }
                
                // Eğer prefab hedef renkleri içeriyorsa ağırlığı artır
                if (totalBlockCount > 0 && targetColorCount > 0)
                {
                    // Hedef renk oranına göre ağırlık hesapla
                    float targetRatio = (float)targetColorCount / totalBlockCount;
                    weight = 1.0f + (targetRatio * (targetColorWeightMultiplier - 1.0f));
                }
            }
            
            weights.Add(weight);
        }

        // Ağırlıklı rastgele seçim yap
        List<int> selectedIndices = new List<int>();
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < shapePrefabs.Count; i++) availableIndices.Add(i);

        // İlk 3 bloğu seç (farklı olmaları için)
        for (int i = 0; i < 3 && availableIndices.Count > 0; i++)
        {
            int selectedIndex = GetWeightedRandomIndex(availableIndices, weights);
            selectedIndices.Add(selectedIndex);
            availableIndices.Remove(selectedIndex);
        }

        // Eğer 3'ten az seçildiyse, kalanları normal ağırlıkla doldur
        while (selectedIndices.Count < 3 && availableIndices.Count > 0)
        {
            int selectedIndex = GetWeightedRandomIndex(availableIndices, weights);
            selectedIndices.Add(selectedIndex);
            availableIndices.Remove(selectedIndex);
        }

        return selectedIndices;
    }

    // Ağırlıklı rastgele index seç
    private int GetWeightedRandomIndex(List<int> availableIndices, List<float> weights)
    {
        if (availableIndices.Count == 0) return 0;
        if (availableIndices.Count == 1) return availableIndices[0];

        // Toplam ağırlığı hesapla
        float totalWeight = 0f;
        foreach (int index in availableIndices)
        {
            if (index >= 0 && index < weights.Count)
                totalWeight += weights[index];
        }

        if (totalWeight <= 0) return availableIndices[Random.Range(0, availableIndices.Count)];

        // Rastgele değer seç
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (int index in availableIndices)
        {
            if (index >= 0 && index < weights.Count)
            {
                currentWeight += weights[index];
                if (randomValue <= currentWeight)
                    return index;
            }
        }

        // Fallback
        return availableIndices[availableIndices.Count - 1];
    }

    // Normal rastgele seçim (fallback)
    private List<int> GetNormalRandomIndices()
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
            Invoke("CheckGameOverDelayed", 0.3f);
        }
    }
    
    void CheckGameOverAfterSpawn()
    {
        // Yeni bloklar spawn edildikten sonra game over kontrolü yap
        if (GameOverManager.instance != null)
        {
            GameOverManager.instance.CheckGameOver();
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