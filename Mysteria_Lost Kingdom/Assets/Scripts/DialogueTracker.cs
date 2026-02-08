using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueTracker : MonoBehaviour, ISaveable
{
    public static DialogueTracker Instance;
    private HashSet<string> completedLines = new HashSet<string>();
    private HashSet<string> usedOptions = new HashSet<string>();

    private SaveableEntity saveableEntity;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        saveableEntity = GetComponent<SaveableEntity>();
        if (saveableEntity == null)
        {
            Debug.LogError("[DialogueTracker] Missing SaveableEntity!");
        }
        //DontDestroyOnLoad(gameObject);
    }

    public string GetSaveID() => saveableEntity.ID;

    public bool IsOptionUsed(string optionID)
    {
        if (string.IsNullOrEmpty(optionID)) return false;
        return usedOptions.Contains(optionID);
    }

    public void MarkOptionUsed(string optionID)
    {
        if (string.IsNullOrEmpty(optionID)) return;
        usedOptions.Add(optionID);
    }

    public bool IsLineCompleted(string dialogueName, int lineIndex)
    {
        return completedLines.Contains($"{dialogueName}_{lineIndex}");
    }

    public void MarkLineCompleted(string dialogueName, int lineIndex)
    {
        completedLines.Add($"{dialogueName}_{lineIndex}");
    }

    public object CaptureState()
    {
        return new DialogueSaveData
        {
            completedLines = completedLines.ToList(),
            usedOptions = usedOptions.ToList()
        };
    }
    public void RestoreState(object state)
    {
        var data = (DialogueSaveData)state;

        completedLines = new HashSet<string>(data.completedLines);
        usedOptions = new HashSet<string>(data.usedOptions);
    }
}
