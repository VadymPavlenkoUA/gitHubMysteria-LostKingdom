using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatScroll : MonoBehaviour
{
    public ScrollRect scrollRect;

    public void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
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
