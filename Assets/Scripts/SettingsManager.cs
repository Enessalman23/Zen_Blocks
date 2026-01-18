using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;
    
    [Header("UI Panelleri")]
    public GameObject settingsPanel;
    public Button settingsButton; 
    
    [Header("Gizlenecek UI Elemanları")]
    public GameObject levelGoalsUI; // Üstteki hedef kutucukları
    public GameObject scoreUI;      // Skor yazısı

    private bool isPaused = false;
    private Vector3 originalScale;

    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) { Destroy(gameObject); return; }

        if (settingsPanel != null)
        {
            originalScale = settingsPanel.transform.localScale;
            settingsPanel.SetActive(false);
        }
    }

    public void ToggleSettings()
    {
        // Eğer bölüm bittiyse (Next Level aktifse) ayarları açma
        GameObject nextLevelBtn = GameObject.Find("NextLevelButton"); 
        if (nextLevelBtn != null && nextLevelBtn.activeInHierarchy) return;

        isPaused = !isPaused;

        if (isPaused)
            OpenSettings();
        else
            ResumeGame();
    }

    private void OpenSettings()
    {
        settingsPanel.SetActive(true);
        settingsPanel.transform.localScale = Vector3.zero;

        // --- YENİ: Arkadaki UI elemanlarını gizle ---
        if (levelGoalsUI != null) levelGoalsUI.SetActive(false);
        if (scoreUI != null) scoreUI.SetActive(false);

        if (settingsButton != null) settingsButton.interactable = false;

        settingsPanel.transform.DOScale(originalScale, 0.4f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true) 
            .OnComplete(() => {
                Time.timeScale = 0; 
                if (settingsButton != null) settingsButton.interactable = true;
            });
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;

        // --- YENİ: Arkadaki UI elemanlarını geri getir ---
        if (levelGoalsUI != null) levelGoalsUI.SetActive(true);
        if (scoreUI != null) scoreUI.SetActive(true);

        settingsPanel.transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => settingsPanel.SetActive(false));
    }

    public void GoToHome()
    {
        Time.timeScale = 1;
        DOTween.KillAll();
        SceneManager.LoadScene("MainMenu"); 
    }

    public void RestartLevel()
    {
        Time.timeScale = 1;
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}