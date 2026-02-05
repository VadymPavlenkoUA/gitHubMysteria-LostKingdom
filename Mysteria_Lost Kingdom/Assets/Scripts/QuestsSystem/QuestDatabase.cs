using UnityEngine;


[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quest/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    public QuestData[] allQuests;
}