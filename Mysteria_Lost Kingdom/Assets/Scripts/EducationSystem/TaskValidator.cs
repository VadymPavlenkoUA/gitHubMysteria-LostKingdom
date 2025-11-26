using System;
using UnityEngine;

public static class TaskValidator
{
    public static TaskResult Validate(TaskRequirement task, string givenAnswer, float timeTaken)
    {
        var res = new TaskResult { correct = false, timeTaken = timeTaken, pointsAwarded = 0, givenAnswer = givenAnswer };
        if (task == null) return res;

        switch (task.kind)
        {
            case TaskKind.FreeText:
                res.correct = string.Equals(task.correctAnswer?.Trim(), givenAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
                break;
            case TaskKind.MultipleChoice:
                res.correct = string.Equals(task.correctAnswer?.Trim(), givenAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
                break;
            case TaskKind.Numeric:
                if (double.TryParse(task.correctAnswer, out var correctNum) && double.TryParse(givenAnswer, out var givenNum))
                {
                    res.correct = Math.Abs(correctNum - givenNum) <= task.numericTolerance;
                }
                break;
            case TaskKind.CodeSnippet:
                // simple heuristic: check if required token exists (task.correctAnswer holds a token)
                res.correct = !string.IsNullOrEmpty(givenAnswer) &&
                  !string.IsNullOrEmpty(task.correctAnswer) &&
                  givenAnswer.Contains(task.correctAnswer, StringComparison.Ordinal);
                break;
        }

        if (res.correct)
        {
            // award points scaled by difficulty & speed (example)
            float speedBonus = Mathf.Clamp01(Mathf.Max(0, 1.5f - (timeTaken / Mathf.Max(1f, task.difficulty))));
            res.pointsAwarded = Mathf.RoundToInt(task.points * (1f + 0.25f * (task.difficulty - 1)) * (1f + speedBonus));
        }
        else
        {
            res.pointsAwarded = 0;
        }

        return res;
    }
}
