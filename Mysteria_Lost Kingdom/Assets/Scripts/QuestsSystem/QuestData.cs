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

public enum QuestStepType
{
    CollectItems,   // Зібрати предмети
    KillEnemy,      // Вбити ворога
    VisitLocation,  // Прийти в певну точку
    TalkToNPC,      // Поговорити з NPC
    SolvePuzzle     // Розв’язати головоломку
}

[Serializable]
public class QuestStep
{
    public string description;
    public bool isComplete;
    public QuestStepType stepType;

    public List<Item> requiredItems;
    public string targetNPC;
    public string locationName;
    public string targetEnemy;
    public int requiredAmount;
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea] public string description;
    public List<QuestStep> steps;
    public QuestReward reward;
}
