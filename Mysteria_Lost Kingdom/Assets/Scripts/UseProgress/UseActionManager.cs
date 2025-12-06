using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UseActionManager : MonoBehaviour
{
    public static UseActionManager Instance;

    [Header("UI Elements")]
    public GameObject panel;       
    public Slider slider;
    public TextMeshProUGUI timerText;

    private float duration;
    private float timer;
    private Action onComplete;
    private Action onCancel;

    public bool isUsing { get; private set; }

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void StartUse(float duration, Action onComplete, Action onCancel = null)
    {
        if (isUsing) return;

        this.duration = duration;
        this.onComplete = onComplete;
        this.onCancel = onCancel;

        isUsing = true;
        timer = 0f;
        slider.value = 0f;
        panel.SetActive(true);
    }

    public void CancelUse()
    {
        if (!isUsing) return;

        isUsing = false;
        panel.SetActive(false);

        onCancel?.Invoke();
    }

    private void Update()
    {
        if (!isUsing) return;

        timer += Time.deltaTime;
        float progress = timer / duration;
        slider.value = progress;

        float left = duration - timer;
        timerText.text = left.ToString("0.0") + " c";

        if (timer >= duration)
        {
            CompleteUse();
        }
    }

    private void CompleteUse()
    {
        isUsing = false;
        panel.SetActive(false);

        onComplete?.Invoke();
    }
}
