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
            var runtimeStep = new QuestStep
            {
                description = step.description,
                isComplete = false,
                stepType = step.stepType,
                requiredItems = step.requiredItems != null ? new List<RequiredItem>(step.requiredItems) : new(),
                removeItemsOnComplete = step.removeItemsOnComplete,
                targetNPC = step.targetNPC,
                locationName = step.locationName,
                targetEnemy = step.targetEnemy,
                requiredAmount = step.requiredAmount,
                targetName = step.targetName,
                targetTag = step.targetTag,
                linkedStepIndices = step.linkedStepIndices != null ? new List<int>(step.linkedStepIndices) : new(),
                killCountMode = step.killCountMode
            };

            if (runtimeStep.stepType == QuestStepType.KillEnemy &&
                runtimeStep.killCountMode == KillCountMode.Lifetime)
            {
                int killed = PlayerKillStats.Instance.GetKills(runtimeStep.targetEnemy);

                runtimeStep.requiredAmount = Mathf.Max(0, runtimeStep.requiredAmount - killed);

                runtimeStep.isComplete = runtimeStep.requiredAmount == 0;
            }

            steps.Add(runtimeStep);
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
                    if (inventory != null)
                    {
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
                    }
                    break;



                case QuestStepType.KillEnemy:
                    if (step.isComplete) break;

                    if (step.killCountMode == KillCountMode.SinceQuest ||
                        step.killCountMode == KillCountMode.Lifetime)
                    {
                        if (!string.IsNullOrEmpty(killedEnemy) &&
                            killedEnemy == step.targetEnemy)
                        {
                            step.requiredAmount--;
                            if (step.requiredAmount <= 0)
                                step.isComplete = true;
                        }
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

    public QuestSaveData ToSaveData()
    {
        QuestSaveData dataSave = new QuestSaveData();
        dataSave.questID = data.uniqueQuestID;
        dataSave.status = status;
        dataSave.completedSteps = new List<bool>();
        foreach (var step in steps) dataSave.completedSteps.Add(step.isComplete);

        return dataSave;
    }

    public void LoadFromSaveData(QuestSaveData saveData, QuestData questDataRef)
    {
        if (saveData == null) throw new ArgumentNullException(nameof(saveData));

        status = saveData.status;
        steps = new List<QuestStep>();

        for (int i = 0; i < questDataRef.steps.Count; i++)
        {
            var originalStep = questDataRef.steps[i];
            QuestStep runtimeStep = new QuestStep
            {
                description = originalStep.description,
                stepType = originalStep.stepType,
                requiredItems = originalStep.requiredItems != null ? new List<RequiredItem>(originalStep.requiredItems) : new(),
                removeItemsOnComplete = originalStep.removeItemsOnComplete,
                targetNPC = originalStep.targetNPC,
                locationName = originalStep.locationName,
                targetEnemy = originalStep.targetEnemy,
                requiredAmount = originalStep.requiredAmount,
                targetName = originalStep.targetName,
                targetTag = originalStep.targetTag,
                linkedStepIndices = originalStep.linkedStepIndices != null ? new List<int>(originalStep.linkedStepIndices) : new(),
                killCountMode = originalStep.killCountMode,
                isComplete = i < saveData.completedSteps.Count ? saveData.completedSteps[i] : false
            };

            steps.Add(runtimeStep);

            Debug.Log($"[LoadFromSaveData] Квест {questDataRef.questName}, крок {i} ({runtimeStep.description}): isComplete={runtimeStep.isComplete}");
        }
    }


}
