using System.Collections.Generic;

[System.Serializable]
public class PlayerLearningState
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
    public int totalAttempts;

    
    public Dictionary<SubjectType, SubjectStats> subjectStats = new();
}

[System.Serializable]
public class SubjectStats
{
    public int correct;
    public int wrong;

    public int streakSubjectCorrect;
    public int streakSubjectWrong;

    public int hintsSubjectUsed;
    public int skipsSubject;

    public float averageResponseSubjectTime;
    public int totalAttempts;
}