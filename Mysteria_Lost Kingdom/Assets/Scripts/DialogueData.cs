using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueOption
{
    public string playerResponse;
    public int nextLineID;
}

[System.Serializable]
public class DialogueLine
{
    public string npcName;
    [TextArea(2, 5)] public string text;
    public List<DialogueOption> options;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/NPC Dialogue")]
public class DialogueData : ScriptableObject
{
    public string npcName;
    public List<DialogueLine> lines;
}
