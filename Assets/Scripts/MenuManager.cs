using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TextMeshProUGUI levelDisplaySubtitle; // "Level: 5" yazısı için (İsteğe bağlı)

    void Start()
    {
        // Ana menü her açıldığında zamanı normale döndür (Panelden çıkışlarda sorun olmasın)
        Time.timeScale = 1;

        // Kayıtlı seviyeyi ana menüde gösterelim
        UpdateMenuUI();
    }

    // Level Modu Butonuna Bağlanacak Fonksiyon
    public void StartLevelMode()
    {
        GameData.currentMode = GameData.Mode.Level;
        
        // PlayerPrefs'ten en son kalınan seviyeyi oku (Kayıt yoksa 1 döner)
        int savedLevel = PlayerPrefs.GetInt("SavedLevel", 1);
        GameData.currentLevel = savedLevel;

        Debug.Log("Level Modu Başlatılıyor: Seviye " + GameData.currentLevel);
        SceneManager.LoadScene("EndlessGameScene");
    }

    // Endless Modu Butonuna Bağlanacak Fonksiyon
    public void StartEndlessMode()
    {
        GameData.currentMode = GameData.Mode.Endless;
        Debug.Log("Endless Modu Başlatılıyor");
        SceneManager.LoadScene("EndlessGameScene");
    }

    // İlerlemeyi Sıfırla Butonuna Bağlanabilir (Test amaçlı)
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("SavedLevel");
        UpdateMenuUI();
        Debug.Log("Tüm ilerleme sıfırlandı!");
    }

    void UpdateMenuUI()
    {
        if (levelDisplaySubtitle != null)
        {
            int current = PlayerPrefs.GetInt("SavedLevel", 1);
            levelDisplaySubtitle.text = "LEVEL " + current;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}