using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceButton : MonoBehaviour
{
    public TMP_Text label;
    public Button button;

    public void Setup(string text, System.Action onClick)
    {
        label.text = text;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());
    }
}
