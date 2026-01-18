using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager instance;
    
    [Header("UI Panelleri")]
    public GameObject gameOverPanel;
    
    private bool isGameOver = false;

    void Awake() 
    { 
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CheckGameOver()
    {
        if (isGameOver) return;
        if (LevelManager.instance != null && LevelManager.instance.IsLevelComplete) return;

        DraggableShape[] shapes = Object.FindObjectsByType<DraggableShape>(FindObjectsSortMode.None);
        int activeShapesCount = 0;
        bool anyMovePossible = false;

        foreach (var s in shapes)
        {
            if (s != null && s.gameObject.activeInHierarchy && s.enabled)
            {
                activeShapesCount++;
                if (GridManager.instance != null && GridManager.instance.CanAnywherePlace(s.gameObject))
                {
                    anyMovePossible = true;
                    break;
                }
            }
        }

        if (activeShapesCount > 0 && !anyMovePossible)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        // Game over sesini çal
        if (AudioManager.instance != null && AudioManager.instance.gameOverSound != null)
        {
            AudioManager.instance.PlaySound(AudioManager.instance.gameOverSound);
        }
        
        StartCoroutine(OpenPanelWithDelay(1f));
    }

    IEnumerator OpenPanelWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (GameData.currentMode == GameData.Mode.Level && LevelManager.instance != null)
        {
            LevelManager.instance.TriggerGameOver();
        }
        else
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                gameOverPanel.transform.SetAsLastSibling();
                gameOverPanel.transform.localScale = Vector3.zero;
                
                gameOverPanel.transform.DOScale(1f, 1.0f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true) 
                    .OnComplete(() => Time.timeScale = 0);
            }
            else
            {
                Debug.LogError("HATA: gameOverPanel NULL! Inspector'da panel atanmamış!");
            }
        }
    }

    public void StopChecks() => isGameOver = true;
}