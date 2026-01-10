using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [Header("Quests List")]
    public Transform contentParent;
    public GameObject questEntryPrefab;
    public GameObject categoryButtonPrefab;

    [Header("Quests Details")]
    public TMP_Text questTitleText;
    public TMP_Text questDescriptionText;
    public TMP_Text questStepsText;
    public TMP_Text questRewardText;
    public ScrollRect detailsScroll;

    private bool showActive = true;
    private bool showFinished = true;

    private void OnEnable()
    {
        RefreshQuestList();
        ClearDetails();

        if (QuestManager.Instance.trackedQuest != null)
        {
            ShowQuestDetails(QuestManager.Instance.trackedQuest);
        }
    }

    public void RefreshQuestList()
    {
        foreach (Transform child in contentParent)Destroy(child.gameObject);

        var activeHeader = Instantiate(categoryButtonPrefab, contentParent);
        activeHeader.GetComponentInChildren<TMP_Text>().text = $"<color=#FF9700>Активні квести ({QuestManager.Instance.activeQuests.Count})</color>";
        var activeBtn = activeHeader.GetComponent<Button>();
        activeBtn.onClick.AddListener(() =>
        {
            showActive = !showActive;
            RefreshQuestList();
        });
        var arrowActive = activeHeader.transform.Find("Arrow");
        if (arrowActive != null)
        {
            arrowActive.localRotation = Quaternion.Euler(0, 0, showActive ? 0f : 90f);
        }

        if (showActive) AddQuestEntries(QuestManager.Instance.activeQuests, hideTracker: false);


        var finishedHeader = Instantiate(categoryButtonPrefab, contentParent);
        finishedHeader.GetComponentInChildren<TMP_Text>().text = $"<color=#675B4C>Завершені квести ({QuestManager.Instance.finishedQuests.Count})</color>";
        var finishedBtn = finishedHeader.GetComponent<Button>();
        finishedBtn.onClick.AddListener(() =>
        {
            showFinished = !showFinished;
            RefreshQuestList();
        });
        var arrowFinish = finishedHeader.transform.Find("Arrow");
        if (arrowFinish != null)
        {
            arrowFinish.localRotation = Quaternion.Euler(0, 0, showFinished ? 0f : 90f);
        }

        if (showFinished) AddQuestEntries(QuestManager.Instance.finishedQuests, hideTracker: true);
    }

    private void AddQuestEntries(List<QuestInstance> quests, bool hideTracker)
    {
        foreach (var questInstance in quests)
        {
            var go = Instantiate(questEntryPrefab, contentParent);
            TMP_Text[] texts = go.GetComponentsInChildren<TMP_Text>();
            TMP_Text nameText = texts[0];
            TMP_Text statusText = texts.Length > 1 ? texts[1] : null;

            nameText.text = questInstance.data.questName;
            statusText.text = $"{CompletedStepsCount(questInstance)}/{questInstance.steps.Count}";

            var tracker = go.GetComponent<QuestTrackToggle>();
            if (tracker != null)
            {
                if (hideTracker)
                {
                    tracker.toggle.gameObject.SetActive(false);
                }
                else
                {
                    ShowQuestDetails(questInstance);
                    tracker.Init(questInstance);
                }
            }

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => ShowQuestDetails(questInstance));
        }
    }

    private int CompletedStepsCount(QuestInstance quest)
    {
        int count = 0;
        foreach (var step in quest.steps)
            if (step.isComplete) count++;
        return count;
    }

    private void ShowQuestDetails(QuestInstance quest)
    {
        if (quest == null || quest.data == null)
        {
            ClearDetails();
            return;
        }

        questTitleText.text = $"<color=#ad4402><b>{quest.data.questName}</b></color>";
        questDescriptionText.text = $"<color=#503622>{quest.data.description}</color>";

        questStepsText.text = "";
        for (int i = 0; i < quest.steps.Count; i++)
        {
            var step = quest.steps[i];
            string status = step.isComplete
                ? "<color=#0e630a>Виконано</color>"
                : "<color=#e80909>Не виконано</color>";
            questStepsText.text += $"{status} {step.description}\n";
        }

        questRewardText.text = GetRewardText(quest.data.reward);
        Canvas.ForceUpdateCanvases();
        detailsScroll.verticalNormalizedPosition = 1;
    }

    private string GetRewardText(QuestReward reward)
    {
        if (reward == null) return "Нагороди немає";

        string result = "";
        if (reward.gold > 0) result += $"<color=#d6c20d>Золото:</color> {reward.gold}\n";
        if (reward.experience > 0) result += $"<color=#007ae6>Досвід:</color> {reward.experience}\n"; ;
        if (reward.professions != null && reward.professions.Count > 0)
        {
            result += "<color=#e88b00>Професійний досвід:</color>\n";
            foreach (var prof in reward.professions)
                result += $"- {prof.proffesionName}: {prof.exp}\n";
        }
        if (reward.items != null && reward.items.Count > 0)
        {
            result += "<color=#20a837>Предмети:</color>\n";
            foreach (var item in reward.items)
                result += $"- {item.item.itemName} x{item.amount}\n";
        }
        return result.TrimEnd();
    }

    private void ClearDetails()
    {
        questTitleText.text = "";
        questDescriptionText.text = "";
        questStepsText.text = "";
        questRewardText.text = "";
    }
}
