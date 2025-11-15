using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private void Awake() => Instance = this;

    public List<QuestInstance> activeQuests = new();
    public List<QuestInstance> completedQuests = new();
    public List<QuestInstance> finishedQuests = new();

    public Inventory playerInventory;

    public QuestInstance trackedQuest;

    private void Start()
    {
        playerInventory.OnInventoryChanged += UpdateQuestsFromInventory;
    }
    public QuestInstance StartQuest(QuestData questData)
    {
        if (GetActiveQuest(questData) != null)
        {
            Debug.LogWarning($"Квест '{questData.questName}' уже активний!");
            return null;
        }

        var instance = new QuestInstance(questData);
        activeQuests.Add(instance);
        Debug.Log($"Квест '{questData.questName}' розпочато!");
        instance.UpdateStepStatus(playerInventory);
        CheckQuestCompletion(instance);
        return instance;
    }

    public QuestInstance GetActiveQuest(QuestData questData)
    {
        return activeQuests.Find(q => q.data == questData);
    }

    public void CompleteStep(QuestData questData, int stepIndex)
    {
        var instance = GetActiveQuest(questData);
        if (instance == null)
        {
            Debug.LogWarning($"Квест '{questData.questName}' не знайдено серед активних!");
            return;
        }

        if (stepIndex < 0 || stepIndex >= instance.steps.Count)
        {
            Debug.LogWarning($"Некоректний індекс кроку для '{questData.questName}'!");
            return;
        }

        var step = instance.steps[stepIndex];

        instance.CompleteStep(stepIndex);
        Debug.Log($"Крок {stepIndex + 1} у квесті '{questData.questName}' виконано!");

        if (step.stepType == QuestStepType.DeliverItems && step.removeItemsOnComplete)
        {
            foreach (var req in step.requiredItems)
            {
                playerInventory.RemoveItem(req.item, req.amount, notify: false);
            }
            InventoryUIManager.Instance.RefreshUI();
            Debug.Log($"Крок '{step.description}': предмети доставлено, інвентар оновлено.");
        }

        instance.UpdateStepStatus(playerInventory);
        CheckQuestCompletion(instance);
    }

    private void CheckQuestCompletion(QuestInstance instance)
    {
        if (instance.IsQuestCompleted())
        {
            activeQuests.Remove(instance);
            completedQuests.Add(instance);

            if (trackedQuest == instance) trackedQuest = null;

            Debug.Log($"Квест '{instance.data.questName}' виконано!");
            ClaimReward(instance);
        }
    }

    public void ClaimReward(QuestInstance instance)
    {
        if (!completedQuests.Contains(instance)) return;

        var reward = instance.data.reward;
        if (reward != null)
        {
            Debug.Log($"Гравець отримав: {reward.gold} золота, {reward.experience} досвіду!");
        }

        completedQuests.Remove(instance);
        finishedQuests.Add(instance);

        Debug.Log($"Квест '{instance.data.questName}' завершено!");
    }

    public bool IsQuestActive(QuestData questData)
    {
        return activeQuests.Exists(q => q.data == questData);
    }

    public bool IsQuestCompleted(QuestData questData)
    {
        return completedQuests.Exists(q => q.data == questData);
    }

    public bool IsQuestFinished(QuestData questData)
    {
        return finishedQuests.Exists(q => q.data == questData);
    }

    public bool IsStepCompleted(QuestData questData, int stepIndex)
    {
        var instance = GetActiveQuest(questData);
        if (instance == null) return false;
        return instance.IsStepCompleted(stepIndex);
    }

    public int GetMaxCompletedStep(QuestData questData)
    {
        var instance = GetActiveQuest(questData);
        if (instance == null) return -1;
        int maxStep = -1;
        for (int i = 0; i < instance.steps.Count; i++)
        {
            if (instance.steps[i].isComplete)
                maxStep = i;
            else
                break;
        }
        return maxStep;
    }

    private void UpdateQuestsFromInventory()
    {
        foreach (var quest in new List<QuestInstance>(activeQuests))
        {
            quest.UpdateStepStatus(InventoryUIManager.Instance.inventory);
            if (quest.IsQuestCompleted()) CheckQuestCompletion(quest);
        }
    }

    public void OnEnemyKilled(string enemyName)
    {
        foreach (var quest in activeQuests)
        {
            quest.UpdateStepStatus(killedEnemy: enemyName);
            if (quest.IsQuestCompleted())
                CheckQuestCompletion(quest);
        }
    }

    public void OnVisitedLocation(string locationName)
    {
        foreach (var quest in activeQuests)
        {
            quest.UpdateStepStatus(visitedLocation: locationName);
            if (quest.IsQuestCompleted())
                CheckQuestCompletion(quest);
        }
    }

    public void OnTalkedToNPC(string npcName)
    {
        foreach (var quest in activeQuests)
        {
            quest.UpdateStepStatus(interactedNPC: npcName);
            if (quest.IsQuestCompleted())
                CheckQuestCompletion(quest);
        }
    }

    public void TrackQuest(QuestInstance instance)
    {
        trackedQuest = instance;
    }

    public void UntrackQuest()
    {
        trackedQuest = null;
    }
}
