using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CreateAssetMenu(fileName = "DialogueHistory", menuName = "Journal/Dialogue History")]
public class DialogueHistoryManager : ScriptableObject
{
    public List<DialogueHistoryEntry> entries = new();
    public void AddEntry(string speaker, string text, bool isPlayer)
    {
        entries.Add(new DialogueHistoryEntry(speaker, text, isPlayer));
    }
    public void ClearHistory()
    {
        entries.Clear();
    }
}
