using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueOption
{
    public string playerResponse;
    public int nextLineID;

    [Header("Повторення / закриття лінії")]
    [Tooltip("Якщо true - після вибору цієї опції поточна репліка NPC більше не повторюється.")]
    public bool endsThisLinePermanently;

    [Tooltip("Якщо true - після вибору цієї опції буде показано заключну фразу NPC.")]
    public bool hasFinalResponse;
    [TextArea(2, 4)]
    public string finalNpcResponse;

    [Header("Дії з квестами")]
    public QuestData questToStart;      
    public QuestData questToComplete;
    public int questStepToComplete;

    [Header("Умови відображення (квести)")]
    public QuestData requiredQuest;      // потрібен активний квест
    public QuestData completedQuest;     // потрібен виконаний квест
    public bool requireQuestNotTaken;    // показувати тільки якщо квест ще не взято

    [Header("Умови по кроках квестів")]
    public QuestData questForStepCheck;     // квест, крок якого перевіряємо
    public int requiredStepIndex = -1;      // індекс кроку для перевірки
    public bool requireStepCompleted;       // true = показувати, якщо крок виконано; false = якщо ні

    [Header("Додаткові параметри")]
    public bool makeLineNonRepeatable;   // якщо треба зробити діалог разовим

    [Header("Одноразова опція")]
    public bool hideAfterSelect = false;      // якщо true — опція зникає назавжди після вибору
    public string optionID;
}

[System.Serializable]
public class DialogueLine
{
    public string npcName;
    [TextArea(2, 5)] public string text;
    public List<DialogueOption> options;
    public bool isRepeatable;

    [Header("Умови показу лінії")]
    public QuestData requiredActiveQuest;     // квест має бути активний
    public QuestData requiredCompletedQuest;  // квест має бути виконаний
    public bool invertCondition;              // якщо true — показувати коли умова НЕ виконана

    [Header("Квестові дії")]
    public QuestData questToStart;     
    public QuestData questToComplete;  
    public int stepIndexToComplete;     
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/NPC Dialogue")]
public class DialogueData : ScriptableObject
{
    public string npcName;
    public Sprite npcIcon;
    public List<DialogueLine> lines;
}
