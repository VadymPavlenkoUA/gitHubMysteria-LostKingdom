using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
                requiredItems = step.requiredItems != null ? new List<RequiredItem>(step.requiredItems) : new List<RequiredItem>(),
                removeItemsOnComplete = step.removeItemsOnComplete,
                targetNPC = step.targetNPC,
                locationName = step.locationName,
                targetEnemy = step.targetEnemy,
                requiredAmount = step.requiredAmount,
                targetName = step.targetName,
                targetTag = step.targetTag,
                linkedStepIndices = step.linkedStepIndices != null ? new List<int>(step.linkedStepIndices) : new List<int>()
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

    public QuestStep GetFirstIncompleteStep()
    {
        foreach (var step in steps) if (!step.isComplete) return step;
        return null;
    }

    public void UpdateStepStatus(
        Inventory inventory = null,
        string interactedNPC = null,
        string visitedLocation = null,
        string killedEnemy = null)
    {
        foreach (var step in steps)
        {
            switch (step.stepType)
            {
                case QuestStepType.CollectItems:
                    bool hasAllItems = true;

                    foreach (var req in step.requiredItems)
                    {
                        if (!inventory.HasItem(req.item, req.amount))
                        {
                            hasAllItems = false;
                            break;
                        }
                    }

                    if (!hasAllItems && step.linkedStepIndices != null)
                    {
                        foreach (int linkedIndex in step.linkedStepIndices)
                        {
                            if (linkedIndex >= 0 && linkedIndex < steps.Count)
                            {
                                if (steps[linkedIndex].isComplete)
                                {
                                    hasAllItems = true;
                                    break;
                                }
                            }
                        }
                    }

                    step.isComplete = hasAllItems;

                    if (hasAllItems && step.removeItemsOnComplete)
                    {
                        foreach (var req in step.requiredItems)
                        {
                            inventory.RemoveItem(req.item, req.amount);
                        }
                        Debug.Log($"Крок '{step.description}': предмети забрано.");
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
