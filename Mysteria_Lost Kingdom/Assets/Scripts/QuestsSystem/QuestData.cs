using UnityEngine;
using System;
using System.Collections.Generic;


public enum QuestStatus
{
    Inactive,
    Active,
    Completed,
    Finished,
    Failed
}

[Serializable]
public class QuestReward
{
    public int gold;
    public int experience;
    public List<Item> items;
}

[Serializable]
public class QuestStep
{
    public string description;
    public bool isComplete;
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea] public string description;
    public List<QuestStep> steps;
    public QuestReward reward;
}
