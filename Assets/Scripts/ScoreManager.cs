using UnityEngine;
using TMPro;
using DG.Tweening; // DOTween kütüphanesini eklemeyi unutma

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI Elementleri")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    private int currentScore = 0;
    private int highScore = 0;
    private int displayedScore = 0; // Animasyonlu gösterim için

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Kayıtlı yüksek skoru yükle
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // Başlangıç değerlerini yazdır
        scoreText.text = "Score: 0";
        if (highScoreText != null)
            highScoreText.text = "Best: " + highScore.ToString();
    }

    /// <summary>
    /// Çarpanlı puan ekleme sistemi. 
    /// linesCount: Aynı anda silinen satır/sütun toplamı.
    /// </summary>
    public void AddScoreWithMultiplier(int linesCount)
    {
        if (linesCount <= 0) return;

        // PUAN HESABI: (Satır Sayısı x 10) * Satır Sayısı
        // Örn: 1 satır = 10, 2 satır = 40, 3 satır = 90
        int pointsToAdd = (linesCount * 50) * linesCount;
        
        currentScore += pointsToAdd;

        // Yüksek skor kontrolü ve kaydı
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save(); // Veriyi fiziksel olarak diske yaz (Mobil için önemli)
            
            if (highScoreText != null)
                highScoreText.text = "Best: " + highScore.ToString();
        }

        AnimateScoreUpdate();
    }

    // Klasik tekli puan ekleme (isteğe bağlı kullanım için)
    public void AddScore(int points)
    {
        currentScore += points;
        AnimateScoreUpdate();
    }

    void AnimateScoreUpdate()
    {
        // Sayıların tırmanma efekti (0.5 saniye sürer)
        DOTween.To(() => displayedScore, x => displayedScore = x, currentScore, 0.5f)
            .OnUpdate(() => {
                scoreText.text = "Score: " + displayedScore.ToString();
            })
            .SetEase(Ease.OutQuad);

        // Score yazısına hafif bir "Zıplama" efekti ver
        scoreText.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0f), 0.2f);
    }
}