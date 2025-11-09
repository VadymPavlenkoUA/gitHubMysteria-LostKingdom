using System;
using System.Collections.Generic;

[Serializable]
public class QuestInstance
{
    public QuestData data { get; private set; }
    public QuestStatus status;
    public List<QuestStep> steps;

    public QuestInstance(QuestData questData)
    {
        data = questData;
        status = QuestStatus.Active;
        steps = new List<QuestStep>();

        foreach (var step in questData.steps)
        {
            steps.Add(new QuestStep
            {
                description = step.description,
                isComplete = false,
                stepType = step.stepType,
                requiredItems = step.requiredItems != null ? new List<Item>(step.requiredItems) : new List<Item>(),
                targetNPC = step.targetNPC,
                locationName = step.locationName,
                targetEnemy = step.targetEnemy,
                requiredAmount = step.requiredAmount
            });
        }
    }

    public bool IsStepCompleted(int index)
    {
        if (index < 0 || index >= steps.Count) return false;
        return steps[index].isComplete;
    }

    public void CompleteStep(int index)
    {
        if (index >= 0 && index < steps.Count)
            steps[index].isComplete = true;
    }

    public bool IsQuestCompleted()
    {
        foreach (var step in steps)
            if (!step.isComplete) return false;
        return true;
    }

    public void UpdateStepStatus(
        Inventory inventory = null,
        string interactedNPC = null,
        string visitedLocation = null,
        string killedEnemy = null)
    {
        foreach (var step in steps)
        {
            if (step.isComplete) continue;

            switch (step.stepType)
            {
                case QuestStepType.CollectItems:
                    if (inventory != null && step.requiredItems != null && step.requiredItems.Count > 0)
                    {
                        bool hasAllItems = true;
                        foreach (var item in step.requiredItems)
                        {
                            if (!inventory.HasItem(item))
                            {
                                hasAllItems = false;
                                break;
                            }
                        }
                        step.isComplete = hasAllItems;
                    }
                    break;

                case QuestStepType.KillEnemy:
                    if (!string.IsNullOrEmpty(killedEnemy) && killedEnemy == step.targetEnemy)
                    {
                        step.requiredAmount--;
                        if (step.requiredAmount <= 0)
                            step.isComplete = true;
                    }
                    break;

                case QuestStepType.VisitLocation:
                    if (!string.IsNullOrEmpty(visitedLocation) && visitedLocation == step.locationName)
                        step.isComplete = true;
                    break;

                case QuestStepType.TalkToNPC:
                    if (!string.IsNullOrEmpty(interactedNPC) && interactedNPC == step.targetNPC)
                        step.isComplete = true;
                    break;

                case QuestStepType.SolvePuzzle:
                    break;
            }
        }
    }
}
