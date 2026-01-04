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
    public List<ItemReward> items;
    public List<ProfessionExp> professions;
}

[System.Serializable]
public struct ProfessionExp
{
    public CraftingProfession profession;
    public string proffesionName;
    public float exp;
}

[System.Serializable]
public struct ItemReward
{
    public Item item;
    public int amount;
}

public enum QuestStepType
{
    CollectItems,   // Зібрати предмети
    DeliverItems,   // Доставити предмети
    KillEnemy,      // Вбити ворога
    VisitLocation,  // Прийти в певну точку
    TalkToNPC,      // Поговорити з NPC
    SolvePuzzle     // Розв’язати головоломку
}

public enum KillCountMode
{
    Lifetime,   // враховує всі вбивства (унікальні, лор)
    SinceQuest  // тільки з моменту взяття квесту (daily, bounty)
}

[Serializable]
public class QuestStep
{
    public string description;
    public bool isComplete;
    public QuestStepType stepType;

    public List<RequiredItem> requiredItems;
    public bool removeItemsOnComplete;
    public string targetNPC;
    public string locationName;
    public string targetEnemy;
    public int requiredAmount;

    public KillCountMode killCountMode;

    [Header("Navigation")]
    public string targetName;
    public string targetTag;

    [Header("Step dependencies")]
    [Tooltip("Якщо будь-який із цих кроків виконано, цей крок автоматично вважається виконаним")]
    public List<int> linkedStepIndices = new List<int>();
}

[Serializable]
public class RequiredItem
{
    public Item item;
    public int amount = 1;
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea] public string description;
    public List<QuestStep> steps;
    public QuestReward reward;

    [TextArea(3, 10)]
    public string aiDescription;
}
