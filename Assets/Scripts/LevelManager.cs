using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class ColorGoal
{
    public string spriteName; 
    public int targetAmount;  
    public int currentAmount; 
    public Color goalColor;   
    [HideInInspector] public TextMeshProUGUI countText; 
    [HideInInspector] public Image iconImage;   
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Üst Hedef UI (Sabit)")]
    public Transform goalContainer;      
    public GameObject goalPrefab;        

    [Header("Seviye Başlangıç UI (Panel)")]
    public GameObject levelStartPanel;    
    public Transform startGoalContainer; 
    public TextMeshProUGUI levelTitleText; // Paneldeki "LEVEL 1" yazısı

    [Header("Diğer Paneller")]
    public GameObject levelCompletePanel; 

    [Header("Seviye Tasarımı")]
    public List<ColorGoal> activeGoals = new List<ColorGoal>();

    private bool isLevelComplete = false;
    private Vector3 panelOriginalScale;

    void Awake()
    {
        if (instance == null) instance = this;
        if (levelStartPanel != null) panelOriginalScale = levelStartPanel.transform.localScale;
    }

    void Start()
    {
        if (GameData.currentMode == GameData.Mode.Level)
        {
            // Score ve HighScore yazılarını gizle
            GameObject scoreTxt = GameObject.Find("ScoreText");
            GameObject highTxt = GameObject.Find("HighScoreText");
            if (scoreTxt != null) scoreTxt.SetActive(false);
            if (highTxt != null) highTxt.SetActive(false);

            if (goalContainer != null) goalContainer.gameObject.SetActive(true);
            SetupLevel(GameData.currentLevel);
        }
        else
        {
            if (goalContainer != null) goalContainer.gameObject.SetActive(false);
        }
    }

    void SetupLevel(int level)
    {
        activeGoals.Clear();
        isLevelComplete = false;

        // 20 SEVİYE TASARIMI (Renk kodlarınla güncellendi)
        switch (level)
        {
            case 1: AddGoal("14_49_34_0", 10, new Color(0f, 0.7f, 1f)); break;
            case 2: AddGoal("14_55_20_0", 12, new Color(0.7f, 0.1f, 1f)); break;
            case 3: 
                AddGoal("14_49_34_0", 8, new Color(0f, 0.7f, 1f));
                AddGoal("14_55_20_0", 8, new Color(0.7f, 0.1f, 1f));
                break;
            case 4: AddGoal("14_59_14_0", 15, new Color(1f, 0.9f, 0f)); break;
            case 5:
                AddGoal("14_55_20_0", 10, new Color(0.7f, 0.1f, 1f));
                AddGoal("14_59_14_0", 10, new Color(1f, 0.9f, 0f));
                break;
            case 6:
                AddGoal("14_49_34_0", 10, new Color(0f, 0.7f, 1f));
                AddGoal("14_55_20_0", 10, new Color(0.7f, 0.1f, 1f));
                AddGoal("14_59_14_0", 10, new Color(1f, 0.9f, 0f));
                break;
            case 7: AddGoal("15_01_10_0", 18, new Color(0.2f, 1f, 0.3f)); break;
            case 8:
                AddGoal("15_01_10_0", 12, new Color(0.2f, 1f, 0.3f));
                AddGoal("14_49_34_0", 12, new Color(0f, 0.7f, 1f));
                break;
            case 9: AddGoal("14_57_09_0", 20, new Color(1f, 0.2f, 0.6f)); break;
            case 10:
                AddGoal("14_49_34_0", 15, new Color(0f, 0.7f, 1f));
                AddGoal("14_55_20_0", 15, new Color(0.7f, 0.1f, 1f));
                AddGoal("14_59_14_0", 15, new Color(1f, 0.9f, 0f));
                AddGoal("15_01_10_0", 15, new Color(0.2f, 1f, 0.3f));
                break;
            case 11: AddGoal("14_55_20_0", 25, new Color(0.7f, 0.1f, 1f)); break;
            case 12:
                AddGoal("14_57_09_0", 15, new Color(1f, 0.2f, 0.6f));
                AddGoal("14_59_14_0", 15, new Color(1f, 0.9f, 0f));
                break;
            case 13:
                AddGoal("15_01_10_0", 20, new Color(0.2f, 1f, 0.3f));
                AddGoal("14_49_34_0", 20, new Color(0f, 0.7f, 1f));
                break;
            case 14:
                AddGoal("14_55_20_0", 15, new Color(0.7f, 0.1f, 1f));
                AddGoal("14_57_09_0", 15, new Color(1f, 0.2f, 0.6f));
                AddGoal("15_01_10_0", 15, new Color(0.2f, 1f, 0.3f));
                break;
            case 15: AddGoal("14_49_34_0", 30, new Color(0f, 0.7f, 1f)); break;
            case 16:
                AddGoal("14_59_14_0", 20, new Color(1f, 0.9f, 0f));
                AddGoal("15_01_10_0", 20, new Color(0.2f, 1f, 0.3f));
                AddGoal("14_57_09_0", 20, new Color(1f, 0.2f, 0.6f));
                break;
            case 17:
                AddGoal("14_49_34_0", 25, new Color(0f, 0.7f, 1f));
                AddGoal("14_55_20_0", 25, new Color(0.7f, 0.1f, 1f));
                break;
            case 18: AddGoal("14_59_14_0", 40, new Color(1f, 0.9f, 0f)); break;
            case 19:
                AddGoal("14_49_34_0", 20, new Color(0f, 0.7f, 1f));
                AddGoal("15_01_10_0", 20, new Color(0.2f, 1f, 0.3f));
                AddGoal("14_57_09_0", 20, new Color(1f, 0.2f, 0.6f));
                AddGoal("14_55_20_0", 20, new Color(0.7f, 0.1f, 1f));
                break;
            case 20:
                AddGoal("14_49_34_0", 30, new Color(0f, 0.7f, 1f));
                AddGoal("14_55_20_0", 30, new Color(0.7f, 0.1f, 1f));
                AddGoal("14_59_14_0", 30, new Color(1f, 0.9f, 0f));
                AddGoal("15_01_10_0", 30, new Color(0.2f, 1f, 0.3f));
                AddGoal("14_57_09_0", 30, new Color(1f, 0.2f, 0.6f));
                break;
            default: AddGoal("14_49_34_0", 50, new Color(0f, 0.7f, 1f)); break;
        }

        CreateGoalUI();
        StartCoroutine(ShowLevelStartUI());
    }

    void AddGoal(string sName, int amount, Color col)
    {
        activeGoals.Add(new ColorGoal { spriteName = sName, targetAmount = amount, currentAmount = 0, goalColor = col });
    }

    void CreateGoalUI()
    {
        foreach (Transform child in goalContainer) Destroy(child.gameObject);
        foreach (var goal in activeGoals)
        {
            GameObject gUI = Instantiate(goalPrefab, goalContainer);
            goal.countText = gUI.GetComponentInChildren<TextMeshProUGUI>();
            Transform iconObj = gUI.transform.Find("Icon");
            if (iconObj != null) {
                goal.iconImage = iconObj.GetComponent<Image>();
                goal.iconImage.color = goal.goalColor;
            }
            UpdateGoalText(goal);
        }
    }

    IEnumerator ShowLevelStartUI()
    {
        if(levelTitleText != null) levelTitleText.text = "LEVEL " + GameData.currentLevel;

        foreach (Transform child in startGoalContainer) Destroy(child.gameObject);
        List<GameObject> tempIcons = new List<GameObject>();

        foreach (var goal in activeGoals)
        {
            GameObject gUI = Instantiate(goalPrefab, startGoalContainer);
            gUI.transform.localScale = Vector3.zero;
            tempIcons.Add(gUI);
            gUI.GetComponentInChildren<TextMeshProUGUI>().text = goal.targetAmount.ToString();
            Transform iconObj = gUI.transform.Find("Icon");
            if (iconObj != null) iconObj.GetComponent<Image>().color = goal.goalColor;
        }

        levelStartPanel.SetActive(true);
        levelStartPanel.transform.localScale = Vector3.zero;

        // Açılış Animasyonu (Zaman durduğunda çalışması için SetUpdate(true))
        Sequence openSeq = DOTween.Sequence().SetUpdate(true);
        openSeq.Append(levelStartPanel.transform.DOScale(panelOriginalScale, 0.5f).SetEase(Ease.OutBack));
        foreach (var icon in tempIcons) 
            openSeq.Append(icon.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));

        openSeq.OnComplete(() => Time.timeScale = 0); // Panel açılınca zamanı durdur

        yield return new WaitForSecondsRealtime(2.5f);

        Time.timeScale = 1; // Kapanırken zamanı başlat
        Sequence closeSeq = DOTween.Sequence();
        foreach (var icon in tempIcons) 
            closeSeq.Join(icon.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
        
        closeSeq.Append(levelStartPanel.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack));

        closeSeq.OnComplete(() => {
            levelStartPanel.SetActive(false);
            levelStartPanel.transform.localScale = panelOriginalScale;
            foreach (Transform child in startGoalContainer) Destroy(child.gameObject);
        });
    }

    public void OnBlockDestroyed(string spriteName)
    {
        if (isLevelComplete || GameData.currentMode != GameData.Mode.Level) return;

        foreach (var goal in activeGoals)
        {
            if (spriteName.Contains(goal.spriteName) && goal.currentAmount < goal.targetAmount)
            {
                goal.currentAmount++;
                goal.countText.transform.parent.DOKill(true); 
                goal.countText.transform.parent.localScale = Vector3.one;
                goal.countText.transform.parent.DOPunchScale(Vector3.one * 0.15f, 0.2f);
                UpdateGoalText(goal);
                CheckWinCondition();
                break;
            }
        }
    }

    void UpdateGoalText(ColorGoal goal)
    {
        int remaining = goal.targetAmount - goal.currentAmount;
        if (remaining <= 0) {
            goal.countText.text = "OK"; // Hata veren sembol yerine V harfi
            goal.countText.color = Color.green;
        } else {
            goal.countText.text = remaining.ToString();
        }
    }

    void CheckWinCondition()
    {
        bool allGoalsMet = true;
        foreach (var goal in activeGoals)
            if (goal.currentAmount < goal.targetAmount) { allGoalsMet = false; break; }

        if (allGoalsMet)
        {
            isLevelComplete = true;
            Invoke("ShowWinPanel", 0.8f);
        }
    }

    void ShowWinPanel()
    {
        levelCompletePanel.SetActive(true);
        levelCompletePanel.transform.localScale = Vector3.zero;
        levelCompletePanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true)
            .OnComplete(() => Time.timeScale = 0);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1;
        GameData.currentLevel++; // Bir sonraki seviyeye geç
    
        // YENİ: Mevcut seviyeyi cihazın hafızasına kaydet
        PlayerPrefs.SetInt("SavedLevel", GameData.currentLevel);
        PlayerPrefs.Save(); // Veriyi hemen diske yaz
    
        DOTween.KillAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}