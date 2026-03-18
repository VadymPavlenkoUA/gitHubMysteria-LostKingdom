using System.Collections.Generic;



[System.Serializable]
public class LearningSaveData
{
    public float mathSkill;
    public float englishSkill;
    public float programmingSkill;

    public int correctAnswers;
    public int wrongAnswers;

    public int streakCorrect;
    public int streakWrong;

    public int hintsUsed;
    public int skips;

    public float averageResponseTime;

    public List<SubjectSaveEntry> subjects = new();
}

[System.Serializable]
public class SubjectSaveEntry
{
    public SubjectType subject;
    public SubjectStats stats;
}