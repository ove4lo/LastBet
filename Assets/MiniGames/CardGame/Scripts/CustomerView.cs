using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerView : MonoBehaviour
{
    [Header("UI")]
    public Image portraitImage;

    public TextMeshProUGUI bubbleText;

    public void Show(Sprite portrait, string text)
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
        }

        if (bubbleText != null)
        {
            bubbleText.text = text;
        }
    }

    public void Clear()
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }

        if (bubbleText != null)
        {
            bubbleText.text = "";
        }
    }
}