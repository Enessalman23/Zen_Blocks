using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    public void Setup(string text, Color color)
    {
        TextMeshPro tmp = GetComponent<TextMeshPro>();
        tmp.text = text;
        tmp.color = color;

        // Yukarı doğru süzülme ve kaybolma efekti
        transform.DOMoveY(transform.position.y + 1f, 0.8f);
        tmp.DOFade(0, 0.8f).OnComplete(() => Destroy(gameObject));
    }
}