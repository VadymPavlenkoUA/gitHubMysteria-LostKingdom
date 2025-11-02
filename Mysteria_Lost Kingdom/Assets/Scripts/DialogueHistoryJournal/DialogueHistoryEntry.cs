using System;

[Serializable]
public class DialogueHistoryEntry
{
    public string speakerName;
    public string text;
    public bool isPlayer;
    public DateTime timestamp;

    public DialogueHistoryEntry(string speakerName, string text, bool isPlayer)
    {
        this.speakerName = speakerName;
        this.text = text;
        this.isPlayer = isPlayer;
        this.timestamp = DateTime.Now;
    }
}
