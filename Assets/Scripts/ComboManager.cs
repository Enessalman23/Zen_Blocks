using UnityEngine;
using TMPro;
using DG.Tweening;

public class ComboManager : MonoBehaviour
{
    public static ComboManager instance;
    public GameObject comboTextPrefab;
    public Transform canvasTransform;

    [Header("Ayarlar")]
    public float animationDuration = 1.5f; // Toplam süre 1.5s
    public float floatDistance = 150f;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void ShowCombo(int multiClearCount, int streakCount)
    {
        if (comboTextPrefab == null || canvasTransform == null) return;
        
        string message = "";
        Color textColor = Color.yellow;

        if (streakCount >= 2 && multiClearCount <= 1)
        {
            message = "STREAK x" + streakCount;
            textColor = Color.cyan;
        }
        else if (multiClearCount >= 2)
        {
            message = multiClearCount + " LINES!";
            if(streakCount >= 2) message = "COMBO x" + (multiClearCount + streakCount);
            textColor = Color.magenta;
        }

        GameObject go = Instantiate(comboTextPrefab, canvasTransform);
        
        // Ekranın ortasında rastgele konum
        go.transform.position = new Vector3(Screen.width / 2 + Random.Range(-100, 100), Screen.height / 2 + Random.Range(-100, 100), 0);
    
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = message;
        tmp.color = textColor;

        // --- ANIMASYON GÜNCELLEMESİ (1.5 Saniye) ---
        go.transform.localScale = Vector3.zero;
        
        // 1. Hızlıca büyü (0.3s)
        go.transform.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack);
        
        // 2. Yavaşça yukarı süzül (1.5s boyunca)
        go.transform.DOMoveY(go.transform.position.y + floatDistance, 1.5f).SetEase(Ease.OutSine);
        
        // 3. Ekranda bekle ve sonra kaybol (1.2s bekle, 0.3s kaybol = Toplam 1.5s)
        tmp.DOFade(0, 0.3f)
            .SetDelay(1.2f) 
            .OnComplete(() => Destroy(go));
    }
}