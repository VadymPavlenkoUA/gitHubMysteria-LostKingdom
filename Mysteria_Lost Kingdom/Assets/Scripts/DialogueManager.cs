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
    public GameObject dialogueUI;
    public TMP_Text chatHistoryText;
    public TMP_Text npcNameText;
    public Transform optionsParent;
    public Button optionButtonPrefab;
    public float typingSpeed = 0.02f;

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

        messages.Add($"<b><< {line.npcName}:</b> ");
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

        StartCoroutine(TypeText(line.text, messages.Count - 1));
    }

    private IEnumerator TypeText(string text, int index)
    {
        StringBuilder sb = new StringBuilder(messages[index]);
        foreach (char c in text)
        {
            sb.Append(c);
            messages[index] = sb.ToString();
            chatHistoryText.text = string.Join("\n", messages);
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void OnOptionSelected(string playerText, int nextLineID)
    {
        messages.Add($"<b>> Гравець:</b> {playerText}");
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
        dialogueUI.SetActive(false);
        currentDialogue = null;
        messages.Clear();
        cinemachineInput.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
