using System;
using UnityEngine;

public class PlayerLearningManager : MonoBehaviour, ISaveable
{
    public static PlayerLearningManager Instance;

    private SaveableEntity saveableEntity;

    public PlayerLearningState State = new();

    private void Awake()
    {
        Instance = this;
        saveableEntity = GetComponent<SaveableEntity>();

        if (saveableEntity == null) Debug.LogError("PlayerLearningManager missing SaveableEntity!");
    }

    public string GetSaveID() => saveableEntity.ID;

    public object CaptureState()
    {
        LearningSaveData data = new LearningSaveData
        {
            mathSkill = State.mathSkill,
            englishSkill = State.englishSkill,
            programmingSkill = State.programmingSkill,

            correctAnswers = State.correctAnswers,
            wrongAnswers = State.wrongAnswers,

            streakCorrect = State.streakCorrect,
            streakWrong = State.streakWrong,

            hintsUsed = State.hintsUsed,
            skips = State.skips,

            averageResponseTime = State.averageResponseTime
        };

        // Dictionary -> List
        foreach (var kvp in State.subjectStats)
        {
            data.subjects.Add(new SubjectSaveEntry
            {
                subject = kvp.Key,
                stats = kvp.Value
            });
        }

        return data;
    }

    public void RestoreState(object state)
    {
        if (state is not LearningSaveData data)
            return;

        State.mathSkill = data.mathSkill;
        State.englishSkill = data.englishSkill;
        State.programmingSkill = data.programmingSkill;

        State.correctAnswers = data.correctAnswers;
        State.wrongAnswers = data.wrongAnswers;

        State.streakCorrect = data.streakCorrect;
        State.streakWrong = data.streakWrong;

        State.hintsUsed = data.hintsUsed;
        State.skips = data.skips;

        State.averageResponseTime = data.averageResponseTime;

        State.subjectStats.Clear();

        // List -> Dictionary
        foreach (var entry in data.subjects)
        {
            State.subjectStats[entry.subject] = entry.stats;
        }
    }

    public void RegisterResult(TaskRequirement task, TaskResult result)
    {
        var subject = task.subject;

        if (!State.subjectStats.ContainsKey(subject))
            State.subjectStats[subject] = new SubjectStats();

        var subjectData = State.subjectStats[subject];

        subjectData.totalAttempts++;

        if (result.correct)
        {
            subjectData.correct++;
            subjectData.streakSubjectCorrect++;
            subjectData.streakSubjectWrong = 0;
        }
        else
        {
            subjectData.wrong++;
            subjectData.streakSubjectWrong++;
            subjectData.streakSubjectCorrect = 0;
        }

        // середній час (нормальний)
        subjectData.averageResponseSubjectTime =
            ((subjectData.averageResponseSubjectTime * (subjectData.totalAttempts - 1)) + result.timeTaken)
            / subjectData.totalAttempts;

        State.totalAttempts++;

        if (result.correct)
        {
            State.correctAnswers++;
            State.streakCorrect++;
            State.streakWrong = 0;
        }
        else
        {
            State.wrongAnswers++;
            State.streakWrong++;
            State.streakCorrect = 0;
        }

        State.averageResponseTime =
            ((State.averageResponseTime * (State.totalAttempts - 1)) + result.timeTaken)
            / State.totalAttempts;


        float delta = result.correct ? 0.02f : -0.01f;

        switch (subject)
        {
            case SubjectType.Math:
                State.mathSkill = Mathf.Clamp01(State.mathSkill + delta);
                break;

            case SubjectType.English:
                State.englishSkill = Mathf.Clamp01(State.englishSkill + delta);
                break;

            case SubjectType.Programming:
                State.programmingSkill = Mathf.Clamp01(State.programmingSkill + delta);
                break;
        }
    }

    public void RegisterHint(SubjectType subject)
    {
        State.hintsUsed++;

        if (!State.subjectStats.ContainsKey(subject))
            State.subjectStats[subject] = new SubjectStats();

        State.subjectStats[subject].hintsSubjectUsed++;
    }

    public void RegisterSkip(SubjectType subject)
    {
        State.skips++;

        if (!State.subjectStats.ContainsKey(subject))
            State.subjectStats[subject] = new SubjectStats();

        var subjectData = State.subjectStats[subject];

        subjectData.skipsSubject++;
        subjectData.streakSubjectWrong++;
        subjectData.streakSubjectCorrect = 0;

        // глобально теж
        State.streakWrong++;
        State.streakCorrect = 0;
    }
}