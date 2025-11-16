using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class CraftQuantitySelector : MonoBehaviour
{
    public Button minusBtn;
    public Button plusBtn;
    public TMP_InputField inputField;

    public int quantity = 1;
    public int maxQuantity = 99;

    public Action<int> onQuantityChanged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        minusBtn.onClick.AddListener(() => ChangeQuantity(-1));
        plusBtn.onClick.AddListener(() => ChangeQuantity(1));
        inputField.characterLimit = 2;
        inputField.onEndEdit.AddListener(ValidateInputField);
        UpdateInputField();
    }

    private void ChangeQuantity(int delta)
    {
        quantity += delta;
        quantity = Mathf.Clamp(quantity, 1, maxQuantity);
        UpdateInputField();
        onQuantityChanged?.Invoke(quantity);
    }
    private void UpdateInputField()
    {
        inputField.text = quantity.ToString();
    }

    private void ValidateInputField(string text)
    {
        int val = 1;
        if (!int.TryParse(text, out val) || val < 1) val = 1;
        quantity = Mathf.Clamp(val, 1, maxQuantity);
        UpdateInputField();
        onQuantityChanged?.Invoke(quantity);
    }

    internal void SetQuantity(int number)
    {
        quantity = Mathf.Clamp(number, 1, maxQuantity);
        inputField.text = quantity.ToString();
        onQuantityChanged?.Invoke(quantity);
    }
}
