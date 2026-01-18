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

    [Header("Blok Görselleri (Sprite)")]
    public Sprite spriteMavi;
    public Sprite spriteSarı;
    public Sprite spriteMor;
    public Sprite spritePembe;
    public Sprite spriteYeşil;

    [Header("Üst Hedef UI (Sabit)")]
    public Transform goalContainer;      
    public GameObject goalPrefab;        

    [Header("Seviye Başlangıç UI (Panel)")]
    public GameObject levelStartPanel;    
    public Transform startGoalContainer; 
    public TextMeshProUGUI levelTitleText;

    [Header("Diğer Paneller")]
    public GameObject levelCompletePanel; 
    public GameObject restartPanel; // Restart paneli referansı eklendi

    [Header("Seviye Tasarımı")]
    public List<ColorGoal> activeGoals = new List<ColorGoal>();

    private bool isLevelComplete = false;
    private Vector3 panelOriginalScale;
    private Dictionary<Transform, Vector3> originalGoalScales = new Dictionary<Transform, Vector3>();

    // Dışarıdan kontrol için public getter
    public bool IsLevelComplete => isLevelComplete;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        if (levelStartPanel != null) panelOriginalScale = levelStartPanel.transform.localScale;
    }

    void Start()
    {
        if (GameData.currentMode == GameData.Mode.Level)
        {
            HideUIElement("ScoreText");
            HideUIElement("HighScoreText");
            if (goalContainer != null) goalContainer.gameObject.SetActive(true);
            SetupLevel(GameData.currentLevel);
        }
        else
        {
            if (goalContainer != null) goalContainer.gameObject.SetActive(false);
        }
    }

    void HideUIElement(string elementName)
    {
        GameObject element = GameObject.Find(elementName);
        if (element != null) element.SetActive(false);
    }

    void SetupLevel(int level)
    {
        activeGoals.Clear();
        isLevelComplete = false;
        // Seviye kurulumları aynı... (Önceki kodun burası)
        AddGoalsByLevel(level);

        CreateGoalUI();
        StartCoroutine(ShowLevelStartUI());
    }

    // 100 Seviye Tasarımı - Her 5 seviyede zorluk artışı
    void AddGoalsByLevel(int level) {
        // Renk tanımları
        Color mavi = new Color(0f, 0.7f, 1f);
        Color mor = new Color(0.7f, 0.1f, 1f);
        Color sarı = new Color(1f, 0.9f, 0f);
        Color pembe = new Color(1f, 0.2f, 0.6f);
        Color yeşil = new Color(0.2f, 1f, 0.3f);

        switch (level) {
            // SEVİYE 1-5: Başlangıç (Tek renk, düşük sayılar)
            case 1: AddGoal("Mavi", 8, mavi); break;
            case 2: AddGoal("Mor", 10, mor); break;
            case 3: AddGoal("Sarı", 9, sarı); break;
            case 4: AddGoal("Pembe", 11, pembe); break;
            case 5: AddGoal("Yeşil", 12, yeşil); break;

            // SEVİYE 6-10: İki renk kombinasyonları
            case 6: AddGoal("Mavi", 8, mavi); AddGoal("Mor", 8, mor); break;
            case 7: AddGoal("Sarı", 10, sarı); AddGoal("Pembe", 10, pembe); break;
            case 8: AddGoal("Mavi", 12, mavi); AddGoal("Yeşil", 12, yeşil); break;
            case 9: AddGoal("Mor", 9, mor); AddGoal("Sarı", 9, sarı); break;
            case 10: AddGoal("Pembe", 11, pembe); AddGoal("Yeşil", 11, yeşil); break;

            // SEVİYE 11-15: Üç renk kombinasyonları
            case 11: AddGoal("Mavi", 8, mavi); AddGoal("Mor", 8, mor); AddGoal("Sarı", 8, sarı); break;
            case 12: AddGoal("Pembe", 10, pembe); AddGoal("Yeşil", 10, yeşil); AddGoal("Mavi", 10, mavi); break;
            case 13: AddGoal("Mor", 9, mor); AddGoal("Sarı", 9, sarı); AddGoal("Pembe", 9, pembe); break;
            case 14: AddGoal("Mavi", 12, mavi); AddGoal("Yeşil", 12, yeşil); AddGoal("Mor", 12, mor); break;
            case 15: AddGoal("Sarı", 11, sarı); AddGoal("Pembe", 11, pembe); AddGoal("Yeşil", 11, yeşil); break;

            // SEVİYE 16-20: Daha yüksek sayılar, çeşitli kombinasyonlar
            case 16: AddGoal("Mavi", 15, mavi); AddGoal("Mor", 15, mor); break;
            case 17: AddGoal("Sarı", 18, sarı); break;
            case 18: AddGoal("Pembe", 14, pembe); AddGoal("Yeşil", 14, yeşil); AddGoal("Mavi", 14, mavi); break;
            case 19: AddGoal("Mor", 16, mor); AddGoal("Sarı", 16, sarı); break;
            case 20: AddGoal("Mavi", 12, mavi); AddGoal("Mor", 12, mor); AddGoal("Sarı", 12, sarı); AddGoal("Pembe", 12, pembe); break;

            // SEVİYE 21-25: Dört renk kombinasyonları
            case 21: AddGoal("Mavi", 15, mavi); AddGoal("Mor", 15, mor); AddGoal("Sarı", 15, sarı); AddGoal("Pembe", 15, pembe); break;
            case 22: AddGoal("Yeşil", 20, yeşil); AddGoal("Mavi", 20, mavi); break;
            case 23: AddGoal("Mor", 13, mor); AddGoal("Sarı", 13, sarı); AddGoal("Pembe", 13, pembe); AddGoal("Yeşil", 13, yeşil); break;
            case 24: AddGoal("Mavi", 18, mavi); AddGoal("Mor", 18, mor); AddGoal("Sarı", 18, sarı); break;
            case 25: AddGoal("Pembe", 16, pembe); AddGoal("Yeşil", 16, yeşil); AddGoal("Mavi", 16, mavi); AddGoal("Mor", 16, mor); break;

            // SEVİYE 26-30: Çok yüksek sayılar
            case 26: AddGoal("Mavi", 25, mavi); AddGoal("Mor", 25, mor); break;
            case 27: AddGoal("Sarı", 22, sarı); AddGoal("Pembe", 22, pembe); AddGoal("Yeşil", 22, yeşil); break;
            case 28: AddGoal("Mavi", 20, mavi); AddGoal("Mor", 20, mor); AddGoal("Sarı", 20, sarı); AddGoal("Pembe", 20, pembe); break;
            case 29: AddGoal("Yeşil", 28, yeşil); break;
            case 30: AddGoal("Mavi", 18, mavi); AddGoal("Mor", 18, mor); AddGoal("Sarı", 18, sarı); AddGoal("Pembe", 18, pembe); AddGoal("Yeşil", 18, yeşil); break;

            // SEVİYE 31-35: Beş renk kombinasyonları, orta zorluk
            case 31: AddGoal("Mavi", 20, mavi); AddGoal("Mor", 20, mor); AddGoal("Sarı", 20, sarı); AddGoal("Pembe", 20, pembe); AddGoal("Yeşil", 20, yeşil); break;
            case 32: AddGoal("Mavi", 25, mavi); AddGoal("Mor", 25, mor); AddGoal("Sarı", 25, sarı); break;
            case 33: AddGoal("Pembe", 22, pembe); AddGoal("Yeşil", 22, yeşil); AddGoal("Mavi", 22, mavi); AddGoal("Mor", 22, mor); break;
            case 34: AddGoal("Sarı", 24, sarı); AddGoal("Pembe", 24, pembe); break;
            case 35: AddGoal("Mavi", 21, mavi); AddGoal("Mor", 21, mor); AddGoal("Sarı", 21, sarı); AddGoal("Pembe", 21, pembe); AddGoal("Yeşil", 21, yeşil); break;

            // SEVİYE 36-40: Çok zor kombinasyonlar
            case 36: AddGoal("Mavi", 30, mavi); AddGoal("Mor", 30, mor); AddGoal("Sarı", 30, sarı); break;
            case 37: AddGoal("Pembe", 28, pembe); AddGoal("Yeşil", 28, yeşil); AddGoal("Mavi", 28, mavi); break;
            case 38: AddGoal("Mor", 25, mor); AddGoal("Sarı", 25, sarı); AddGoal("Pembe", 25, pembe); AddGoal("Yeşil", 25, yeşil); break;
            case 39: AddGoal("Mavi", 32, mavi); AddGoal("Mor", 32, mor); break;
            case 40: AddGoal("Mavi", 24, mavi); AddGoal("Mor", 24, mor); AddGoal("Sarı", 24, sarı); AddGoal("Pembe", 24, pembe); AddGoal("Yeşil", 24, yeşil); break;

            // SEVİYE 41-45: Çok yüksek sayılar, çoklu renkler
            case 41: AddGoal("Mavi", 35, mavi); AddGoal("Mor", 35, mor); AddGoal("Sarı", 35, sarı); break;
            case 42: AddGoal("Pembe", 30, pembe); AddGoal("Yeşil", 30, yeşil); AddGoal("Mavi", 30, mavi); AddGoal("Mor", 30, mor); break;
            case 43: AddGoal("Sarı", 33, sarı); AddGoal("Pembe", 33, pembe); AddGoal("Yeşil", 33, yeşil); break;
            case 44: AddGoal("Mavi", 28, mavi); AddGoal("Mor", 28, mor); AddGoal("Sarı", 28, sarı); AddGoal("Pembe", 28, pembe); AddGoal("Yeşil", 28, yeşil); break;
            case 45: AddGoal("Mavi", 40, mavi); AddGoal("Mor", 40, mor); break;

            // SEVİYE 46-50: Zor kombinasyonlar
            case 46: AddGoal("Sarı", 36, sarı); AddGoal("Pembe", 36, pembe); AddGoal("Yeşil", 36, yeşil); break;
            case 47: AddGoal("Mavi", 32, mavi); AddGoal("Mor", 32, mor); AddGoal("Sarı", 32, sarı); AddGoal("Pembe", 32, pembe); break;
            case 48: AddGoal("Yeşil", 38, yeşil); AddGoal("Mavi", 38, mavi); AddGoal("Mor", 38, mor); break;
            case 49: AddGoal("Pembe", 34, pembe); AddGoal("Yeşil", 34, yeşil); AddGoal("Mavi", 34, mavi); AddGoal("Mor", 34, mor); AddGoal("Sarı", 34, sarı); break;
            case 50: AddGoal("Mavi", 30, mavi); AddGoal("Mor", 30, mor); AddGoal("Sarı", 30, sarı); AddGoal("Pembe", 30, pembe); AddGoal("Yeşil", 30, yeşil); break;

            // SEVİYE 51-55: Çok zor
            case 51: AddGoal("Mavi", 42, mavi); AddGoal("Mor", 42, mor); AddGoal("Sarı", 42, sarı); break;
            case 52: AddGoal("Pembe", 38, pembe); AddGoal("Yeşil", 38, yeşil); AddGoal("Mavi", 38, mavi); AddGoal("Mor", 38, mor); break;
            case 53: AddGoal("Sarı", 40, sarı); AddGoal("Pembe", 40, pembe); AddGoal("Yeşil", 40, yeşil); break;
            case 54: AddGoal("Mavi", 36, mavi); AddGoal("Mor", 36, mor); AddGoal("Sarı", 36, sarı); AddGoal("Pembe", 36, pembe); AddGoal("Yeşil", 36, yeşil); break;
            case 55: AddGoal("Mavi", 45, mavi); AddGoal("Mor", 45, mor); break;

            // SEVİYE 56-60: Aşırı zor
            case 56: AddGoal("Sarı", 44, sarı); AddGoal("Pembe", 44, pembe); AddGoal("Yeşil", 44, yeşil); AddGoal("Mavi", 44, mavi); break;
            case 57: AddGoal("Mor", 40, mor); AddGoal("Sarı", 40, sarı); AddGoal("Pembe", 40, pembe); break;
            case 58: AddGoal("Mavi", 38, mavi); AddGoal("Mor", 38, mor); AddGoal("Sarı", 38, sarı); AddGoal("Pembe", 38, pembe); AddGoal("Yeşil", 38, yeşil); break;
            case 59: AddGoal("Yeşil", 46, yeşil); AddGoal("Mavi", 46, mavi); AddGoal("Mor", 46, mor); break;
            case 60: AddGoal("Mavi", 42, mavi); AddGoal("Mor", 42, mor); AddGoal("Sarı", 42, sarı); AddGoal("Pembe", 42, pembe); AddGoal("Yeşil", 42, yeşil); break;

            // SEVİYE 61-65: Çok yüksek sayılar
            case 61: AddGoal("Mavi", 50, mavi); AddGoal("Mor", 50, mor); AddGoal("Sarı", 50, sarı); break;
            case 62: AddGoal("Pembe", 45, pembe); AddGoal("Yeşil", 45, yeşil); AddGoal("Mavi", 45, mavi); AddGoal("Mor", 45, mor); break;
            case 63: AddGoal("Sarı", 48, sarı); AddGoal("Pembe", 48, pembe); AddGoal("Yeşil", 48, yeşil); break;
            case 64: AddGoal("Mavi", 44, mavi); AddGoal("Mor", 44, mor); AddGoal("Sarı", 44, sarı); AddGoal("Pembe", 44, pembe); AddGoal("Yeşil", 44, yeşil); break;
            case 65: AddGoal("Mavi", 52, mavi); AddGoal("Mor", 52, mor); break;

            // SEVİYE 66-70: Çoklu renk, çok yüksek sayılar
            case 66: AddGoal("Sarı", 50, sarı); AddGoal("Pembe", 50, pembe); AddGoal("Yeşil", 50, yeşil); AddGoal("Mavi", 50, mavi); break;
            case 67: AddGoal("Mor", 46, mor); AddGoal("Sarı", 46, sarı); AddGoal("Pembe", 46, pembe); break;
            case 68: AddGoal("Mavi", 48, mavi); AddGoal("Mor", 48, mor); AddGoal("Sarı", 48, sarı); AddGoal("Pembe", 48, pembe); AddGoal("Yeşil", 48, yeşil); break;
            case 69: AddGoal("Yeşil", 54, yeşil); AddGoal("Mavi", 54, mavi); AddGoal("Mor", 54, mor); break;
            case 70: AddGoal("Mavi", 50, mavi); AddGoal("Mor", 50, mor); AddGoal("Sarı", 50, sarı); AddGoal("Pembe", 50, pembe); AddGoal("Yeşil", 50, yeşil); break;

            // SEVİYE 71-75: Aşırı zor kombinasyonlar
            case 71: AddGoal("Mavi", 55, mavi); AddGoal("Mor", 55, mor); AddGoal("Sarı", 55, sarı); break;
            case 72: AddGoal("Pembe", 52, pembe); AddGoal("Yeşil", 52, yeşil); AddGoal("Mavi", 52, mavi); AddGoal("Mor", 52, mor); break;
            case 73: AddGoal("Sarı", 54, sarı); AddGoal("Pembe", 54, pembe); AddGoal("Yeşil", 54, yeşil); break;
            case 74: AddGoal("Mavi", 50, mavi); AddGoal("Mor", 50, mor); AddGoal("Sarı", 50, sarı); AddGoal("Pembe", 50, pembe); AddGoal("Yeşil", 50, yeşil); break;
            case 75: AddGoal("Mavi", 58, mavi); AddGoal("Mor", 58, mor); break;

            // SEVİYE 76-80: Çok yüksek sayılar
            case 76: AddGoal("Sarı", 56, sarı); AddGoal("Pembe", 56, pembe); AddGoal("Yeşil", 56, yeşil); AddGoal("Mavi", 56, mavi); break;
            case 77: AddGoal("Mor", 52, mor); AddGoal("Sarı", 52, sarı); AddGoal("Pembe", 52, pembe); break;
            case 78: AddGoal("Mavi", 54, mavi); AddGoal("Mor", 54, mor); AddGoal("Sarı", 54, sarı); AddGoal("Pembe", 54, pembe); AddGoal("Yeşil", 54, yeşil); break;
            case 79: AddGoal("Yeşil", 60, yeşil); AddGoal("Mavi", 60, mavi); AddGoal("Mor", 60, mor); break;
            case 80: AddGoal("Mavi", 56, mavi); AddGoal("Mor", 56, mor); AddGoal("Sarı", 56, sarı); AddGoal("Pembe", 56, pembe); AddGoal("Yeşil", 56, yeşil); break;

            // SEVİYE 81-85: Aşırı zor
            case 81: AddGoal("Mavi", 62, mavi); AddGoal("Mor", 62, mor); AddGoal("Sarı", 62, sarı); break;
            case 82: AddGoal("Pembe", 58, pembe); AddGoal("Yeşil", 58, yeşil); AddGoal("Mavi", 58, mavi); AddGoal("Mor", 58, mor); break;
            case 83: AddGoal("Sarı", 60, sarı); AddGoal("Pembe", 60, pembe); AddGoal("Yeşil", 60, yeşil); break;
            case 84: AddGoal("Mavi", 58, mavi); AddGoal("Mor", 58, mor); AddGoal("Sarı", 58, sarı); AddGoal("Pembe", 58, pembe); AddGoal("Yeşil", 58, yeşil); break;
            case 85: AddGoal("Mavi", 65, mavi); AddGoal("Mor", 65, mor); break;

            // SEVİYE 86-90: Çok aşırı zor
            case 86: AddGoal("Sarı", 64, sarı); AddGoal("Pembe", 64, pembe); AddGoal("Yeşil", 64, yeşil); AddGoal("Mavi", 64, mavi); break;
            case 87: AddGoal("Mor", 60, mor); AddGoal("Sarı", 60, sarı); AddGoal("Pembe", 60, pembe); break;
            case 88: AddGoal("Mavi", 62, mavi); AddGoal("Mor", 62, mor); AddGoal("Sarı", 62, sarı); AddGoal("Pembe", 62, pembe); AddGoal("Yeşil", 62, yeşil); break;
            case 89: AddGoal("Yeşil", 68, yeşil); AddGoal("Mavi", 68, mavi); AddGoal("Mor", 68, mor); break;
            case 90: AddGoal("Mavi", 64, mavi); AddGoal("Mor", 64, mor); AddGoal("Sarı", 64, sarı); AddGoal("Pembe", 64, pembe); AddGoal("Yeşil", 64, yeşil); break;

            // SEVİYE 91-95: Neredeyse imkansız
            case 91: AddGoal("Mavi", 70, mavi); AddGoal("Mor", 70, mor); AddGoal("Sarı", 70, sarı); break;
            case 92: AddGoal("Pembe", 66, pembe); AddGoal("Yeşil", 66, yeşil); AddGoal("Mavi", 66, mavi); AddGoal("Mor", 66, mor); break;
            case 93: AddGoal("Sarı", 68, sarı); AddGoal("Pembe", 68, pembe); AddGoal("Yeşil", 68, yeşil); break;
            case 94: AddGoal("Mavi", 66, mavi); AddGoal("Mor", 66, mor); AddGoal("Sarı", 66, sarı); AddGoal("Pembe", 66, pembe); AddGoal("Yeşil", 66, yeşil); break;
            case 95: AddGoal("Mavi", 72, mavi); AddGoal("Mor", 72, mor); break;

            // SEVİYE 96-100: Master seviyeleri
            case 96: AddGoal("Sarı", 70, sarı); AddGoal("Pembe", 70, pembe); AddGoal("Yeşil", 70, yeşil); AddGoal("Mavi", 70, mavi); break;
            case 97: AddGoal("Mor", 68, mor); AddGoal("Sarı", 68, sarı); AddGoal("Pembe", 68, pembe); break;
            case 98: AddGoal("Mavi", 70, mavi); AddGoal("Mor", 70, mor); AddGoal("Sarı", 70, sarı); AddGoal("Pembe", 70, pembe); AddGoal("Yeşil", 70, yeşil); break;
            case 99: AddGoal("Yeşil", 75, yeşil); AddGoal("Mavi", 75, mavi); AddGoal("Mor", 75, mor); break;
            case 100: AddGoal("Mavi", 80, mavi); AddGoal("Mor", 80, mor); AddGoal("Sarı", 80, sarı); AddGoal("Pembe", 80, pembe); AddGoal("Yeşil", 80, yeşil); break;

            default: 
                // 100'den sonraki seviyeler için dinamik zorluk
                int baseAmount = 20 + (level * 2);
                int colorCount = (level % 5) + 1;
                if (colorCount > 5) colorCount = 5;
                
                if (colorCount >= 1) AddGoal("Mavi", baseAmount, mavi);
                if (colorCount >= 2) AddGoal("Mor", baseAmount, mor);
                if (colorCount >= 3) AddGoal("Sarı", baseAmount, sarı);
                if (colorCount >= 4) AddGoal("Pembe", baseAmount, pembe);
                if (colorCount >= 5) AddGoal("Yeşil", baseAmount, yeşil);
                break;
        }
    }

    void AddGoal(string sName, int amount, Color col)
    {
        activeGoals.Add(new ColorGoal { spriteName = sName, targetAmount = amount, currentAmount = 0, goalColor = col });
    }

    void CreateGoalUI()
    {
        if (goalContainer == null) return;
        foreach (Transform child in goalContainer) Destroy(child.gameObject);
        originalGoalScales.Clear();

        foreach (var goal in activeGoals)
        {
            GameObject gUI = Instantiate(goalPrefab, goalContainer);
            goal.countText = gUI.GetComponentInChildren<TextMeshProUGUI>();
            if (!originalGoalScales.ContainsKey(gUI.transform))
                originalGoalScales.Add(gUI.transform, gUI.transform.localScale);

            Image img = gUI.transform.Find("Icon")?.GetComponent<Image>();
            if (img != null) 
            {
                goal.iconImage = img;
                goal.iconImage.sprite = GetSpriteByName(goal.spriteName);
            }
            UpdateGoalText(goal);
        }
    }

    IEnumerator ShowLevelStartUI()
    {
        if (levelStartPanel == null || startGoalContainer == null) yield break;
        if(levelTitleText != null) levelTitleText.text = "LEVEL " + GameData.currentLevel;

        foreach (Transform child in startGoalContainer) Destroy(child.gameObject);
        List<GameObject> tempGoalObjs = new List<GameObject>();

        foreach (var goal in activeGoals)
        {
            GameObject gUI = Instantiate(goalPrefab, startGoalContainer);
            gUI.transform.localScale = Vector3.zero;
            tempGoalObjs.Add(gUI);
            
            TextMeshProUGUI txt = gUI.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = goal.targetAmount.ToString();
            
            Image img = gUI.transform.Find("Icon")?.GetComponent<Image>();
            if (img != null) img.sprite = GetSpriteByName(goal.spriteName);
        }

        levelStartPanel.SetActive(true);
        levelStartPanel.transform.localScale = Vector3.zero;

        Sequence openSeq = DOTween.Sequence().SetUpdate(true);
        openSeq.Append(levelStartPanel.transform.DOScale(panelOriginalScale, 0.5f).SetEase(Ease.OutBack));
        foreach (var obj in tempGoalObjs) openSeq.Join(obj.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));

        openSeq.OnComplete(() => Time.timeScale = 0);
        yield return new WaitForSecondsRealtime(2.5f);
        Time.timeScale = 1;
        
        Sequence closeSeq = DOTween.Sequence();
        foreach (var obj in tempGoalObjs) closeSeq.Join(obj.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
        closeSeq.Append(levelStartPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));

        closeSeq.OnComplete(() => {
            levelStartPanel.SetActive(false);
            levelStartPanel.transform.localScale = panelOriginalScale;
            foreach (var obj in tempGoalObjs) if (obj != null) Destroy(obj);
        });
    }

    public void OnBlockDestroyed(string spriteName)
{
    // Seviye bittiyse daha fazla işlem yapma
    if (isLevelComplete || GameData.currentMode != GameData.Mode.Level) return;

    foreach (var goal in activeGoals)
    {
        if (spriteName.Contains(goal.spriteName) && goal.currentAmount < goal.targetAmount)
        {
            goal.currentAmount++;
            UpdateGoalText(goal);
            CheckWinCondition(); // Her blokta kontrol et
            break;
        }
    }
}

    void UpdateGoalText(ColorGoal goal)
    {
        if (goal.countText == null) return;
        int remaining = goal.targetAmount - goal.currentAmount;
        goal.countText.text = (remaining <= 0) ? "OK" : remaining.ToString();
        if (remaining <= 0) goal.countText.color = Color.green;
    }

   void CheckWinCondition()
{
    if (isLevelComplete) return;

    bool allGoalsMet = true;
    foreach (var goal in activeGoals) 
        if (goal.currentAmount < goal.targetAmount) { allGoalsMet = false; break; }

    if (allGoalsMet)
    {
        isLevelComplete = true; // ANINDA TRUE YAP: GameOver'ı durdurur
        if (GameOverManager.instance != null) GameOverManager.instance.StopChecks(); 
        
        StartCoroutine(ShowWinPanelDelayed());
    }
}

    IEnumerator ShowWinPanelDelayed()
    {
        // 1 saniye bekleme süresi
        yield return new WaitForSeconds(1.0f);
        
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            levelCompletePanel.transform.localScale = Vector3.zero;
            levelCompletePanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true).OnComplete(() => Time.timeScale = 0);
        }
    }

    public void TriggerGameOver()
    {
        if (isLevelComplete) return;
        
        if (GameOverManager.instance != null && GameOverManager.instance.gameOverPanel != null)
        {
            StartCoroutine(ShowGameOverPanelDelayed());
        }
        else
        {
            Debug.LogError("HATA: GameOverManager veya gameOverPanel bulunamadı!");
        }
    }

    IEnumerator ShowGameOverPanelDelayed()
    {
        yield return new WaitForSeconds(1.0f);
        
        if (isLevelComplete) yield break;

        GameObject gameOverPanel = GameOverManager.instance.gameOverPanel;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
            gameOverPanel.transform.localScale = Vector3.zero;
            gameOverPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true).OnComplete(() => Time.timeScale = 0);
        }
        else
        {
            Debug.LogError("HATA: gameOverPanel NULL!");
        }
    }

    Sprite GetSpriteByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        
        if (name.Contains("Mavi")) return spriteMavi;
        if (name.Contains("Sarı")) return spriteSarı;
        if (name.Contains("Mor")) return spriteMor;
        if (name.Contains("Pembe")) return spritePembe;
        if (name.Contains("Yeşil")) return spriteYeşil;
        return null;
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1;
        GameData.currentLevel++;
        PlayerPrefs.SetInt("SavedLevel", GameData.currentLevel);
        PlayerPrefs.Save();
        DOTween.KillAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}