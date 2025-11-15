using UnityEngine;
using UnityEngine.UI;

public class QuestTrackToggle : MonoBehaviour
{
    public Toggle toggle;
    private QuestInstance quest;

    public void Init(QuestInstance quest)
    {
        this.quest = quest;
        toggle.isOn = (QuestManager.Instance.trackedQuest == quest);
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            QuestManager.Instance.TrackQuest(quest);
            QuestUIManager.Instance.RefreshQuestList();
        }
        else
        {
            if (QuestManager.Instance.trackedQuest == quest)
            {
                QuestManager.Instance.UntrackQuest();
                QuestUIManager.Instance.RefreshQuestList();
            }
        }
    }
}
