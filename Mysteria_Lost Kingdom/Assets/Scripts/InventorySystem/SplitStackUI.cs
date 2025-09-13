using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class SplitStackUI : MonoBehaviour
{
    public Slider slider;
    public TMP_InputField inputField;
    public Button confirmButton;
    public Button cancelButton;

    private Action<int> onConfirm;
    private int maxAmount;

    private void Awake()
    {
        slider.onValueChanged.AddListener(OnSliderChanged);
        inputField.onEndEdit.AddListener(OnInputChanged);

        confirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke((int)slider.value);
            gameObject.SetActive(false);
        });

        cancelButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void Show(int max, Action<int> confirmCallback)
    {
        maxAmount = max;
        onConfirm = confirmCallback;

        slider.minValue = 1;
        slider.maxValue = maxAmount;
        slider.value = maxAmount / 2;

        inputField.text = slider.value.ToString();

        gameObject.SetActive(true);
    }

    private void OnSliderChanged(float value)
    {
        inputField.text = ((int)value).ToString();
    }

    private void OnInputChanged(string value)
    {
        if (int.TryParse(value, out int num))
        {
            num = Mathf.Clamp(num, 1, maxAmount);
            slider.value = num;
            inputField.text = num.ToString();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
