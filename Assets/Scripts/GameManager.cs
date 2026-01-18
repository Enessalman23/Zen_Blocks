using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Oyun Durumu")]
    public bool isGameStarted = false;
    public int streakCount = 0;
    
    [Header("Combo Sistemi")]
    [Tooltip("Kaç hamle boyunca blok patlamazsa combo sıfırlanır")]
    public int comboTolerance = 2;
    private int missedMoves = 0; // Kaç hamledir blok patlamadı

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Mobil optimizasyon ayarları
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        isGameStarted = false;
        // 0.5 saniyelik açılış süresi
        yield return new WaitForSecondsRealtime(0.5f);
        isGameStarted = true;
        
        // GridManager'a başlangıç animasyonu ver
        if(GridManager.instance != null)
            GridManager.instance.transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0), 0.5f, 1);
    }

    // Patlama ve seri mantığını burada işliyoruz
    public void ProcessMatch(int totalLines)
    {
        if (totalLines > 0)
        {
            streakCount++; // Seri arttı
            missedMoves = 0; // Başarılı hamle, sayacı sıfırla
            
            // Puan hesaplama (totalLines * streakCount)
            if (ScoreManager.instance != null)
                ScoreManager.instance.AddScoreWithMultiplier(totalLines * streakCount);

            // Combo/Streak görseli
            if (ComboManager.instance != null && (totalLines >= 2 || streakCount >= 2))
                ComboManager.instance.ShowCombo(totalLines, streakCount);
        }
        else
        {
            // Blok patlamadı, kaçırılan hamle sayısını artır
            missedMoves++;
            
            // Tolerans aşıldıysa combo sıfırlanır
            if (missedMoves >= comboTolerance)
            {
                streakCount = 0;
                missedMoves = 0;
            }
        }
    }
}