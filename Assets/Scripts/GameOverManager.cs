using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager instance;
    public GameObject gameOverPanel;
    private bool isGameOver = false;

    void Awake() { if (instance == null) instance = this; }

    public void CheckGameOver()
    {
        if (isGameOver || GameManager.instance == null || !GameManager.instance.isGameStarted) return;

        // Kritik: Sahnede sürüklenmeyi bekleyen (yerleşmemiş) şekilleri bul
        DraggableShape[] shapes = Object.FindObjectsByType<DraggableShape>(FindObjectsSortMode.None);
        
        bool anyMovePossible = false;
        int activeShapesCount = 0;

        foreach (var s in shapes)
        {
            // Eğer şekil bir Spawner içindeyse (henüz yerleşmemişse)
            if (s.gameObject.activeInHierarchy && s.enabled)
            {
                activeShapesCount++;
                if (GridManager.instance.CanAnywherePlace(s.gameObject))
                {
                    anyMovePossible = true;
                    break;
                }
            }
        }

        // Sahnede şekil var ama hiçbirine yer yoksa oyun biter
        if (activeShapesCount > 0 && !anyMovePossible)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        DOVirtual.DelayedCall(0.5f, () => {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.localScale = Vector3.zero;
            gameOverPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true)
                .OnComplete(() => Time.timeScale = 0);
        }).SetUpdate(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}