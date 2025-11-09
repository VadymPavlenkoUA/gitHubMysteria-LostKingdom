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

    private Coroutine typingCoroutine;
    private DialogueData currentDialogue;
    private int currentLineIndex;
    private List<string> messages = new List<string>();
    [SerializeField] private CinemachineInputAxisController cinemachineInput;

    private void Awake()
    {
        Instance = this;
        dialogueUI.SetActive(false);
    }

    public void StartDialogue(DialogueData dialogue)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        messages.Clear();
        dialogueUI.SetActive(true);
        cinemachineInput.enabled = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        ShowLine();
    }

    private void ShowLine()
    {
        if (currentDialogue == null || currentLineIndex >= currentDialogue.lines.Count)
        {
            EndDialogue();
            return;
        }

        var line = currentDialogue.lines[currentLineIndex];

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

        messages.Add($"<b><< {line.npcName}:</b> ");
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


    //private void ShowLine()
    //{
    //    if (currentDialogue == null || currentLineIndex >= currentDialogue.lines.Count)
    //    {
    //        EndDialogue();
    //        return;
    //    }

    //    var line = currentDialogue.lines[currentLineIndex];

    //    if (DialogueTracker.Instance.IsLineCompleted(currentDialogue.name, currentLineIndex))
    //    {
    //        Debug.Log($"Рядок '{currentDialogue.name}' [{currentLineIndex}] уже був показаний і не повторюється.");
    //        currentLineIndex++;
    //        ShowLine();
    //        return;
    //    }

    //    if (!line.isRepeatable) DialogueTracker.Instance.MarkLineCompleted(currentDialogue.name, currentLineIndex);

    //    npcNameText.text = line.npcName;
    //    npcIcon.sprite = currentDialogue.npcIcon;

    //    messages.Add($"<b><< {line.npcName}:</b> ");
    //    dialogueHistoryManager.AddEntry(line.npcName, line.text, false);
    //    chatHistoryText.text = string.Join("\n", messages);
    //    ClearOptions();

    //    if (line.options.Count > 0)
    //    {
    //        foreach(var option in line.options)
    //        {
    //            var btn = Instantiate(optionButtonPrefab, optionsParent);
    //            btn.GetComponentInChildren<TMP_Text>().text = option.playerResponse;
    //            int nextID = option.nextLineID;
    //            btn.onClick.AddListener(() => OnOptionSelected(option.playerResponse, nextID));
    //        }
    //    }
    //    else
    //    {
    //        var btn = Instantiate(optionButtonPrefab, optionsParent);
    //        btn.GetComponentInChildren<TMP_Text>().text = "Закінчити розмову";
    //        btn.onClick.AddListener(() => EndDialogue());
    //    }

    //    typingCoroutine = StartCoroutine(TypeText(line.text, messages.Count - 1));
    //    HandleQuestActions(line);
    //}

    //private void HandleQuestActions(DialogueLine line)
    //{
    //    if (line.questToStart != null)
    //    {
    //        QuestManager.Instance.StartQuest(line.questToStart);
    //        Debug.Log($"Квест видано: {line.questToStart.questName}");
    //    }

    //    if (line.questToComplete != null)
    //    {
    //        QuestManager.Instance.CompleteStep(line.questToComplete, line.stepIndexToComplete);
    //        Debug.Log($"Оновлено квест: {line.questToComplete.questName}, крок {line.stepIndexToComplete}");
    //    }
    //}

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

        messages.Add($"<b>>>{mainCharacterName}:</b> {option.playerResponse}");
        dialogueHistoryManager.AddEntry(mainCharacterName, option.playerResponse, true);
        chatHistoryText.text = string.Join("\n", messages);

        if (option.questToStart != null)
            QuestManager.Instance.StartQuest(option.questToStart);

        if (option.questToComplete != null)
            QuestManager.Instance.CompleteStep(option.questToComplete, option.questStepToComplete);

        if (option.makeLineNonRepeatable || option.endsThisLinePermanently)
            DialogueTracker.Instance.MarkLineCompleted(currentDialogue.name, currentLineIndex);

        if (option.hasFinalResponse && !string.IsNullOrEmpty(option.finalNpcResponse))
        {
            ShowFinalResponse(option.finalNpcResponse);
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

    //private void OnOptionSelected(string playerText, int nextLineID)
    //{
    //    FinishCurrentTyping();
    //    messages.Add($"<b>>>{mainCharacterName}:</b> {playerText}");
    //    dialogueHistoryManager.AddEntry(mainCharacterName, playerText, true);
    //    chatHistoryText.text = string.Join("\n", messages);

    //    DialogueLine currentLine = currentDialogue.lines[currentLineIndex];
    //    DialogueOption selectedOption = currentLine.options.Find(o => o.playerResponse == playerText);

    //    if (selectedOption == null)
    //    {
    //        EndDialogue();
    //        return;
    //    }

    //    if (selectedOption.endsThisLinePermanently)
    //    {
    //        DialogueTracker.Instance.MarkLineCompleted(currentDialogue.name, currentLineIndex);
    //    }

    //    if (selectedOption.questToStart != null)
    //    {
    //        QuestManager.Instance.StartQuest(selectedOption.questToStart);
    //        Debug.Log($"Видано квест: {selectedOption.questToStart.questName}");
    //    }

    //    if (selectedOption.hasFinalResponse && !string.IsNullOrEmpty(selectedOption.finalNpcResponse))
    //    {
    //        ShowFinalResponse(selectedOption.finalNpcResponse);
    //        return;
    //    }

    //    if (nextLineID < 0)
    //    {
    //        EndDialogue();
    //        return;
    //    }
    //    currentLineIndex = nextLineID;
    //    ShowLine();
    //}

    private bool CanShowOption(DialogueOption option)
    {
        var qm = QuestManager.Instance;

        if (option.requiredQuest != null && !qm.IsQuestActive(option.requiredQuest)) return false;

        if (option.completedQuest != null && !qm.IsQuestCompleted(option.completedQuest)) return false;

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

        string initialText = $"<b><< {npcName}:</b> ";
        messages.Add(initialText);
        int messageIndex = messages.Count - 1;
        chatHistoryText.text = string.Join("\n", messages);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(npcResponse, messageIndex));

        var btn = Instantiate(optionButtonPrefab, optionsParent);
        btn.GetComponentInChildren<TMP_Text>().text = "Закінчити розмову";
        btn.onClick.AddListener(() => EndDialogue());
    }

    private void EndDialogue()
    {
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
    }

    private void FinishCurrentTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            int index = messages.Count - 1;
            var line = currentDialogue.lines[currentLineIndex];
            messages[index] = $"<b><< {line.npcName}:</b> {line.text}";
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
