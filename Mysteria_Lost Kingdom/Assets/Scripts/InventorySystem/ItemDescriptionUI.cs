using TMPro;
using UnityEngine;

public class ItemDescriptionUI : MonoBehaviour
{
    public static ItemDescriptionUI Instance;

    public TextMeshProUGUI descriptionText;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDescription(string text)
    {
        descriptionText.text = text;
    }

    public void ClearDescription()
    {
        descriptionText.text = "";
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
