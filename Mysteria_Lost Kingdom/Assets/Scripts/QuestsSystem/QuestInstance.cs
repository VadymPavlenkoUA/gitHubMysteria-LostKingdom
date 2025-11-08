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
            steps.Add(new QuestStep { description = step.description, isComplete = false });
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
}
