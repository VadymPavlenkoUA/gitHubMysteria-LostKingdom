using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private void Awake() => Instance = this;

    public List<QuestInstance> activeQuests = new();
    public List<QuestInstance> completedQuests = new();
    public List<QuestInstance> finishedQuests = new();

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

        instance.CompleteStep(stepIndex);
        Debug.Log($"Крок {stepIndex + 1} у квесті '{questData.questName}' виконано!");
        CheckQuestCompletion(instance);
    }

    private void CheckQuestCompletion(QuestInstance instance)
    {
        if (instance.IsQuestCompleted())
        {
            activeQuests.Remove(instance);
            completedQuests.Add(instance);
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
}
