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
        var line = currentDialogue.lines[currentLineIndex];
        npcNameText.text = line.npcName;
        npcIcon.sprite = currentDialogue.npcIcon;

        messages.Add($"<b><< {line.npcName}:</b> ");
        dialogueHistoryManager.AddEntry(line.npcName, line.text, false);
        chatHistoryText.text = string.Join("\n", messages);
        ClearOptions();

        if (line.options.Count > 0)
        {
            foreach(var option in line.options)
            {
                var btn = Instantiate(optionButtonPrefab, optionsParent);
                btn.GetComponentInChildren<TMP_Text>().text = option.playerResponse;
                int nextID = option.nextLineID;
                btn.onClick.AddListener(() => OnOptionSelected(option.playerResponse, nextID));
            }
        }
        else
        {
            var btn = Instantiate(optionButtonPrefab, optionsParent);
            btn.GetComponentInChildren<TMP_Text>().text = "Закінчити розмову";
            btn.onClick.AddListener(() => EndDialogue());
        }

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

    private void OnOptionSelected(string playerText, int nextLineID)
    {
        FinishCurrentTyping();
        messages.Add($"<b>>>{mainCharacterName}:</b> {playerText}");
        dialogueHistoryManager.AddEntry(mainCharacterName, playerText, true);
        chatHistoryText.text = string.Join("\n", messages);
        if (nextLineID < 0)
        {
            EndDialogue();
            return;
        }
        currentLineIndex = nextLineID;
        ShowLine();
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
