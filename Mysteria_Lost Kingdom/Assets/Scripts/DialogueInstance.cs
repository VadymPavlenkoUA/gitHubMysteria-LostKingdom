//using System.Collections.Generic;
//using UnityEngine;

//public class DialogueInstance
//{
//    public string npcName;
//    public Sprite npcIcon;
//    public List<DialogueLine> lines = new List<DialogueLine>();
//    public int currentLineIndex = 0;

//    public DialogueInstance(DialogueData data)
//    {
//        npcName = data.npcName;
//        npcIcon = data.npcIcon;

//        // Глибоке копіювання ліній і опцій, щоб не змінювати оригінал
//        foreach (var line in data.lines)
//        {
//            DialogueLine newLine = new DialogueLine
//            {
//                npcName = line.npcName,
//                text = line.text,
//                isRepeatable = line.isRepeatable,
//                questToStart = line.questToStart,
//                questToComplete = line.questToComplete,
//                stepIndexToComplete = line.stepIndexToComplete,
//                options = new List<DialogueOption>()
//            };

//            foreach (var opt in line.options)
//            {
//                DialogueOption newOpt = new DialogueOption
//                {
//                    playerResponse = opt.playerResponse,
//                    nextLineID = opt.nextLineID,
//                    endsThisLinePermanently = opt.endsThisLinePermanently,
//                    hasFinalResponse = opt.hasFinalResponse,
//                    finalNpcResponse = opt.finalNpcResponse,
//                    questToStart = opt.questToStart,
//                    questToComplete = opt.questToComplete,
//                    questStepToComplete = opt.questStepToComplete,
//                    requiredQuest = opt.requiredQuest,
//                    completedQuest = opt.completedQuest,
//                    requireQuestNotTaken = opt.requireQuestNotTaken,
//                    questForStepCheck = opt.questForStepCheck,
//                    requiredStepIndex = opt.requiredStepIndex,
//                    requireStepCompleted = opt.requireStepCompleted,
//                    makeLineNonRepeatable = opt.makeLineNonRepeatable
//                };

//                newLine.options.Add(newOpt);
//            }

//            lines.Add(newLine);
//        }
//    }

//    public DialogueLine GetCurrentLine()
//    {
//        if (currentLineIndex < 0 || currentLineIndex >= lines.Count)
//            return null;

//        return lines[currentLineIndex];
//    }

//    public void MoveToLine(int index)
//    {
//        currentLineIndex = Mathf.Clamp(index, 0, lines.Count - 1);
//    }
//}
