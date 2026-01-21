using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header ("UI Elements")]
    public DialogueHistoryManager dialogueHistoryManager;
    public GameObject dialogueUI;
    public TMP_Text chatHistoryText;
    public TMP_Text npcNameText;
    public Image npcIcon;
    public Transform optionsParent;
    public Button optionButtonPrefab;
    public string mainCharacterName;
    public float typingSpeed = 0.02f;
    string npcColor = "#E6C48A";
    string playerColor = "#9FC5E8";

    private Coroutine typingCoroutine;
    private DialogueData currentDialogue;
    private int currentLineIndex;
    private List<string> messages = new List<string>();
    private Inventory traderInventory;
    [HideInInspector]
    internal bool isDialogueOpen;
    [SerializeField] private CinemachineInputAxisController cinemachineInput;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        Instance = this;
        inputActions = MenuController.Instance.inputActions;
        dialogueUI.SetActive(false);
    }

    public void StartDialogue(DialogueData dialogue, Inventory npcInventory = null)
    {
        if (npcInventory != null) traderInventory = npcInventory;
        if (MenuController.Instance.isGMOpen) MenuController.Instance.OpenGameMenu();
        InteractionBlocker.Block(InteractionBlockReason.Dialogue);
        isDialogueOpen = true;
        currentDialogue = dialogue;
        currentLineIndex = 0;
        messages.Clear();
        dialogueUI.SetActive(true);
        cinemachineInput.enabled = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        inputActions.Combat.Disable();
        ShowLine();
    }

    private bool CanShowLine(DialogueLine line)
    {
        var qm = QuestManager.Instance;
        bool result = true;

        if (line.requiredActiveQuest != null)
            result &= qm.IsQuestActive(line.requiredActiveQuest);

        if (line.requiredCompletedQuest != null)
            result &= qm.IsQuestFinished(line.requiredCompletedQuest);

        if (line.invertCondition)
            result = !result;

        return result;
    }


    private void ShowLine()
    {
        if (currentDialogue == null || currentLineIndex >= currentDialogue.lines.Count)
        {
            EndDialogue();
            return;
        }

        var line = currentDialogue.lines[currentLineIndex];

        if (!CanShowLine(line))
        {
            currentLineIndex++;
            ShowLine();
            return;
        }

        if (DialogueTracker.Instance.IsLineCompleted(currentDialogue.name, currentLineIndex))
        {
            Debug.Log($"Рядок '{currentDialogue.name}' [{currentLineIndex}] уже був показаний і не повторюється.");
            currentLineIndex++;
            ShowLine();
            return;
        }

        npcNameText.text = line.npcName;
        npcIcon.sprite = currentDialogue.npcIcon;

        if (!line.isRepeatable) DialogueTracker.Instance.MarkLineCompleted(currentDialogue.name, currentLineIndex);

        messages.Add($"<color={npcColor}><b><< {line.npcName}: </b></color>");
        dialogueHistoryManager.AddEntry(line.npcName, line.text, false);
        chatHistoryText.text = string.Join("\n", messages);
        ClearOptions();

        if (line.options.Count > 0)
        {
            foreach (var option in line.options)
            {
                if (!CanShowOption(option)) continue;

                var btn = Instantiate(optionButtonPrefab, optionsParent);
                btn.GetComponentInChildren<TMP_Text>().text = option.playerResponse;
                btn.onClick.AddListener(() => OnOptionSelected(option));
            }
        }
        else
        {
            var btn = Instantiate(optionButtonPrefab, optionsParent);
            btn.GetComponentInChildren<TMP_Text>().text = "Закінчити розмову";
            btn.onClick.AddListener(() => EndDialogue());
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line.text, messages.Count - 1));
    }

    private IEnumerator TypeText(string text, int index)
    {
        if (index < 0 || index >= messages.Count) yield break;

        StringBuilder sb = new StringBuilder(messages[index]);
        foreach (char c in text)
        {
            if (index < 0 || index >= messages.Count) yield break;

            sb.Append(c);
            messages[index] = sb.ToString();
            chatHistoryText.text = string.Join("\n", messages);
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null;
    }

    private void OnOptionSelected(DialogueOption option)
    {
        FinishCurrentTyping();

        switch (option.actionType)
        {
            case DialogueActionType.OpenTrade:
                EndDialogue();
                if (traderInventory == null)
                {
                    Debug.LogError("NPC не має інвентарю торгівлі!");
                    return;
                }
                TradeManager.Instance.OpenTrade(traderInventory);
                return;
        }

        if (option.hideAfterSelect) DialogueTracker.Instance.MarkOptionUsed(option.optionID);

        messages.Add($"<color={playerColor}><b>>> {mainCharacterName}: </b></color>{option.playerResponse}");
        dialogueHistoryManager.AddEntry(mainCharacterName, option.playerResponse, true);
        chatHistoryText.text = string.Join("\n", messages);

        if (option.questToComplete != null)
            QuestManager.Instance.CompleteStep(option.questToComplete, option.questStepToComplete);

        if (option.questToStart != null)
        {
            var questInstance = QuestManager.Instance.StartQuest(option.questToStart);
            Debug.Log($"Видано квест: {questInstance.data.questName}");
        }

        if (option.makeLineNonRepeatable || option.endsThisLinePermanently)
            DialogueTracker.Instance.MarkLineCompleted(currentDialogue.name, currentLineIndex);

        if (option.hasFinalResponse && !string.IsNullOrEmpty(option.finalNpcResponse))
        {
            ShowFinalResponse(option.finalNpcResponse);
            dialogueHistoryManager.AddEntry(currentDialogue.npcName, option.finalNpcResponse, false);
            chatHistoryText.text = string.Join("\n", messages);
            return;
        }

        if (option.nextLineID >= 0)
        {
            currentLineIndex = option.nextLineID;
            ShowLine();
        }
        else
        {
            EndDialogue();
        }

    }

    private bool CanShowOption(DialogueOption option)
    {
        var qm = QuestManager.Instance;

        if (option.hideAfterSelect && DialogueTracker.Instance.IsOptionUsed(option.optionID)) return false;

        if (option.requiredQuest != null && !qm.IsQuestActive(option.requiredQuest)) return false;

        if (option.completedQuest != null && !qm.IsQuestFinished(option.completedQuest)) return false;

        if (option.requireQuestNotTaken && qm.IsQuestActive(option.requiredQuest)) return false;

        if (option.questForStepCheck != null && option.requiredStepIndex >= 0)
        {
            int maxCompletedStep = qm.GetMaxCompletedStep(option.questForStepCheck);
            Debug.Log($"{maxCompletedStep} - {option.requiredStepIndex}");
            if (maxCompletedStep > option.requiredStepIndex) { Debug.Log($"{maxCompletedStep} - {option.requiredStepIndex}"); return false; }

            bool isComplete = qm.IsStepCompleted(option.questForStepCheck, option.requiredStepIndex);
            if (option.requireStepCompleted && !isComplete) return false;
            if (!option.requireStepCompleted && isComplete) return false;
        }
        if (option.questToComplete != null)
        {
            if (QuestManager.Instance.IsQuestFinished(option.completedQuest)) return false;
        }
        return true;
    }


    private void ShowFinalResponse(string npcResponse)
    {
        ClearOptions();
        var npcName = currentDialogue.npcName;

        string initialText = $"<color={npcColor}><b><< {npcName}: </b></color>";
        messages.Add(initialText);
        int messageIndex = messages.Count - 1;
        chatHistoryText.text = string.Join("\n", messages);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(npcResponse, messageIndex));

        var btn = Instantiate(optionButtonPrefab, optionsParent);
        btn.GetComponentInChildren<TMP_Text>().text = "Закінчити розмову";
        btn.onClick.AddListener(() => EndDialogue());
    }

    internal void EndDialogue()
    {
        InteractionBlocker.Unblock(InteractionBlockReason.Dialogue);
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueUI.SetActive(false);
        currentDialogue = null;
        messages.Clear();
        cinemachineInput.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isDialogueOpen = false;
        inputActions.Combat.Enable();
    }

    private void FinishCurrentTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            int index = messages.Count - 1;
            var line = currentDialogue.lines[currentLineIndex];
            messages[index] = $"<color={npcColor}><b><< {line.npcName}: </b></color>{line.text}";
            chatHistoryText.text = string.Join("\n", messages);

            typingCoroutine = null;
        }
    }

    private void ClearOptions()
    {
        foreach (Transform child in optionsParent)
            Destroy(child.gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
