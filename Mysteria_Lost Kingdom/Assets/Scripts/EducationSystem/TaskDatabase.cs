using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Education/TaskDatabase")]
public class TaskDatabase : ScriptableObject
{
    public List<TaskRequirement> allTasks;
    private Queue<TaskRequirement> recentTasks = new Queue<TaskRequirement>();
    private int historySize = 5;
    public TaskRequirement GetRandomTask()
    {
        if (allTasks == null || allTasks.Count == 0) return null;
        return allTasks[Random.Range(0, allTasks.Count)];
    }
    public TaskRequirement GetRandomTask(SubjectType subject)
    {
        var filtered = allTasks.FindAll(t => t.subject == subject);
        if (filtered.Count == 0) return null;

        var learning = PlayerLearningManager.Instance;

        if (learning == null) return filtered[Random.Range(0, filtered.Count)];

        float avgDifficulty = 0f;
        foreach (var t in filtered) avgDifficulty += t.difficulty;
        avgDifficulty /= filtered.Count;
        int baseDifficulty = Mathf.RoundToInt(avgDifficulty);

        int targetDifficulty = learning.GetAdaptiveDifficulty(subject, baseDifficulty);

        int minDiff = Mathf.Clamp(targetDifficulty - 1, 1, 10);
        int maxDiff = Mathf.Clamp(targetDifficulty + 1, 1, 10);

        var candidates = filtered.FindAll(t => t.difficulty >= minDiff && t.difficulty <= maxDiff);
        if (candidates.Count == 0) candidates = filtered;

        var candidatesFiltered = candidates.FindAll(t => !recentTasks.Contains(t));

        if (candidatesFiltered.Count == 0) candidatesFiltered = candidates;

        TaskRequirement bestTask = null;
        float bestScore = float.MinValue;

        if (!learning.State.subjectStats.TryGetValue(subject, out var stats)) stats = new SubjectStats();

        foreach (var t in candidatesFiltered)
        {
            float retention = learning.GetRetention(stats);

            // чим менше retention, тим більший пріоритет
            float forgettingScore = 1f - retention;

            // додаємо невелику випадковість, щоб не було завжди одна і та ж задача
            float score = forgettingScore + Random.Range(0f, 0.2f);

            if (score > bestScore)
            {
                bestScore = score;
                bestTask = t;
            }
        }

        var task = bestTask ?? candidatesFiltered[Random.Range(0, candidatesFiltered.Count)];

        recentTasks.Enqueue(task);
        historySize = Mathf.Min(5, filtered.Count - 1);
        if (recentTasks.Count > historySize) recentTasks.Dequeue();

        return task;
    }

    public TaskRequirement GetRandomTaskByDifficulty(SubjectType subject, int difficulty)
    {
        var filtered = allTasks.FindAll(t => t.subject == subject && t.difficulty == difficulty);
        if (filtered.Count == 0)
            return GetRandomTask(subject);

        return filtered[Random.Range(0, filtered.Count)];
    }
}
