using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public enum SubjectType { Math, English, Programming}

[Serializable]
public enum TaskKind { FreeText, MultipleChoice, Numeric, CodeSnippet}

[CreateAssetMenu(menuName = "Education/TaskRequirement")]
public class TaskRequirement : ScriptableObject
{
    public string id;
    public SubjectType subject;
    public TaskKind kind;
    [TextArea(3, 8)]
    public string questionText;
    public List<string> choices;
    public string correctAnswer;
    public float numericTolerance = 0.001f;
    public int difficulty = 1;
    public string hint;
    public int points;
    public string codeTemplate;
}
