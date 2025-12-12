using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI messageText;
    public CanvasGroup canvasGroup;

    [Header("Timing")]
    public float displayTime = 2.5f;
    public float fadeDuration = 1f;

    private float timer;

    public System.Action<NotificationUI> onDestroyed;

    public void Setup(Sprite icon, string msg)
    {
        iconImage.sprite = icon;
        iconImage.gameObject.SetActive(icon != null);

        messageText.text = msg;
        canvasGroup.alpha = 1f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= displayTime)
        {
            float t = (timer - displayTime) / fadeDuration;
            canvasGroup.alpha = 1f - t;

            if (canvasGroup.alpha <= 0)
            {
                onDestroyed?.Invoke(this);
                Destroy(gameObject);
            }
        }
    }

    public void SetAlpha(float a)
    {
        if (this == null) return;
        canvasGroup.alpha = a;
    }
}
