using UnityEngine;
using TMPro;

public class OnlyNumbersInput : MonoBehaviour
{
    public TMP_InputField inputField;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputField.characterLimit = 3;
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        inputField.onEndEdit.AddListener(ValidateInput);
    }
    private void ValidateInput(string text)
    {
        if (string.IsNullOrEmpty(text) || int.Parse(text) < 1)
        {
            inputField.text = "1";
        }
    }
}
