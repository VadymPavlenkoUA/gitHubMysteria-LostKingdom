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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
