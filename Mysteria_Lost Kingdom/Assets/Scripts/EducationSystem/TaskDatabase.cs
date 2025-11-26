using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Education/TaskDatabase")]
public class TaskDatabase : ScriptableObject
{
    public List<TaskRequirement> allTasks;
    public TaskRequirement GetRandomTask()
    {
        if (allTasks == null || allTasks.Count == 0) return null;
        return allTasks[Random.Range(0, allTasks.Count)];
    }
    public TaskRequirement GetRandomTask(SubjectType subject)
    {
        var filtered = allTasks.FindAll(t => t.subject == subject);
        if (filtered.Count == 0) return null;

        return filtered[Random.Range(0, filtered.Count)];
    }

    public TaskRequirement GetRandomTaskByDifficulty(SubjectType subject, int difficulty)
    {
        var filtered = allTasks.FindAll(t => t.subject == subject && t.difficulty == difficulty);
        if (filtered.Count == 0)
            return GetRandomTask(subject);

        return filtered[Random.Range(0, filtered.Count)];
    }
}
