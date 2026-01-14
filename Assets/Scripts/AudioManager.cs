using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Ses Kaynağı")]
    public AudioSource efxSource;

    [Header("Ses Dosyaları")]
    public AudioClip grabSound;     // Bloğu tutunca
    public AudioClip placeSound;    // Bloğu bırakınca
    public AudioClip blastSound;    // Satır patlayınca
    public AudioClip comboSound;    // Kombo olunca
    public AudioClip gameOverSound; // Oyun bitince

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Standart ses çalma (Hafif pitch değişimi ile)
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && efxSource != null)
        {
            efxSource.pitch = Random.Range(0.9f, 1.1f);
            efxSource.PlayOneShot(clip);
        }
    }

    // Kombo sesi için özel fonksiyon (Kombo arttıkça ses incelir)
    public void PlayComboSound(int comboLevel)
    {
        if (comboSound != null && efxSource != null)
        {
            efxSource.pitch = 1.0f + (comboLevel * 0.1f); 
            efxSource.PlayOneShot(comboSound);
        }
    }
}