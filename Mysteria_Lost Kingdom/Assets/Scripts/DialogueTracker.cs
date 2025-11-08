using System.Collections.Generic;
using UnityEngine;

public class DialogueTracker : MonoBehaviour
{
    public static DialogueTracker Instance;
    private HashSet<string> completedLines = new HashSet<string>();

    private void Awake()
    {
        Instance = this;
    }

    public bool IsLineCompleted(string dialogueName, int lineIndex)
    {
        return completedLines.Contains($"{dialogueName}_{lineIndex}");
    }

    public void MarkLineCompleted(string dialogueName, int lineIndex)
    {
        completedLines.Add($"{dialogueName}_{lineIndex}");
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
