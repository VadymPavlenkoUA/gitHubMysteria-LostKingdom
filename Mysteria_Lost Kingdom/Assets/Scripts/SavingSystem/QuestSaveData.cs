using System;
using System.Collections.Generic;

[Serializable]
public class QuestSaveData
{
    public string questID;
    public QuestStatus status;
    public List<bool> completedSteps; 
}

[Serializable]
public class QuestSaveDataList
{
    public List<QuestSaveData> quests;
    public string trackedQuestID;
}
