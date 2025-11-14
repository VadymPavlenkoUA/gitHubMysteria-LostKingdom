using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public class AIResponse
{
    public string response;
}

[System.Serializable]
public class AIRequest
{
    public string prompt;
    public AIRequest(string prompt)
    {
        this.prompt = prompt;
    }
}

public class AssistantAI : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public Inventory inventory;

    [Header("UI Elements")]
    public TMP_InputField inputField;
    public Button sendButton;
    public TextMeshProUGUI chatHistoryText;
    public ChatScroll chatScroll;

    [Header("Typing Settings")]
    public float typingSpeed = 0.02f;

    private List<string> messages = new List<string>();

    void Start()
    {
        sendButton.onClick.AddListener(OnSendMessage);
    }

    private void OnSendMessage()
    {
        string userMessage = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        AddMessage(">> Гравець", userMessage);
        inputField.text = "";

        StartCoroutine(SendAIRequest(userMessage));
    }

    private IEnumerator SendAIRequest(string prompt)
    {
        string url = "http://localhost:5000/ask";
        string statsInfo = GetStatsSummary();
        string questInfo = GetQuestSummary();
        string fullPrompt = $"[Question]\n{prompt}\n\n[Player Stats]\n{statsInfo}\n\n[Quests] (All quest descriptions are for AI context only.)\n{questInfo}";
        string json = JsonUtility.ToJson(new AIRequest(fullPrompt));

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            messages.Add($"<i>...Привид думає...</i>");
            chatHistoryText.text = string.Join("\n", messages);
            chatScroll.ScrollToBottom();

            yield return www.SendWebRequest();

            Debug.Log(www.downloadHandler.text);

            string aiText = "";

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                aiText = "Вибач, я зараз не можу відповісти.";
            }
            else
            {
                AIResponse response = JsonUtility.FromJson<AIResponse>(www.downloadHandler.text);
                aiText = response.response;

            }

            int messageIndex = messages.Count;
            // Додаємо нове повідомлення від привида, спочатку порожнє
            messages[messageIndex - 1] = $"<b><< Привид:</b> ";
            chatHistoryText.text = string.Join("\n", messages);
            chatScroll.ScrollToBottom();

            // Поступово додаємо текст по символах
            yield return StartCoroutine(TypeText(aiText, messageIndex - 1));
        }
    }

    private string GetStatsSummary()
    {
        if (playerStats == null) return "Player stats are unknown. Say you don't know.";
        
        return $"Level: {playerStats.level}\n" +
            $"Vitality: {playerStats.vitality}\n" +
            $"Strength: {playerStats.strength}\n" +
            $"Endurance: {playerStats.endurance}\n" +
            $"Agility: {playerStats.agility}\n" +
            $"Intellect: {playerStats.intellect}\n" +
            $"Faith: {playerStats.faith}\n" +
            $"Health points: {playerStats.currentHealth}/{playerStats.maxHealth}\n" +
            $"Stamina: {playerStats.currentStamina}/{playerStats.maxStamina}\n" +
            $"Current / Max Weight: {inventory.CurrentWeight} / {playerStats.maxWeight}";
    }

    private string GetQuestSummary()
    {
        if (QuestManager.Instance == null) return "Quest data unavailable.";

        StringBuilder sb = new StringBuilder();

        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            sb.AppendLine($"[Active Quest] AI Description: {quest.data.aiDescription}");
            sb.AppendLine("Steps:");
            foreach (var step in quest.steps)
            {
                string status = step.isComplete ? "Completed" : "Not completed";
                sb.AppendLine($"- {status}: {step.description}");
            }
            sb.AppendLine();
        }

        foreach (var quest in QuestManager.Instance.finishedQuests)
        {
            sb.AppendLine($"[Finished Quest] AI Description: {quest.data.aiDescription}");
            sb.AppendLine("All steps completed.\n");
        }

        return sb.Length > 0 ? sb.ToString() : "No quests found.";
    }

    private IEnumerator TypeText(string text, int index)
    {
        StringBuilder sb = new StringBuilder(messages[index]);
        foreach (char c in text)
        {
            sb.Append(c);
            messages[index] = sb.ToString();
            chatHistoryText.text = string.Join("\n", messages);
            chatScroll.ScrollToBottom();
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void AddMessage(string sender, string message)
    {
        messages.Add($"<b>{sender}:</b> {message}");
        chatHistoryText.text = string.Join("\n", messages);
        chatScroll.ScrollToBottom();
    }
}



//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;
//using System.Text;
//using System.Collections.Generic;

//public class AssistantAI : MonoBehaviour
//{
//    [Header("UI Elements")]
//    public TMP_InputField inputField;
//    public Button sendButton;
//    public TextMeshProUGUI chatHistoryText;
//    public ChatScroll chatScroll;

//    private List<string> messages = new List<string>();

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        sendButton.onClick.AddListener(OnSendMessage);
//    }

//    private void OnSendMessage()
//    {
//        string userMessage = inputField.text.Trim();
//        if (string.IsNullOrEmpty(userMessage)) return;

//        AddMessage(">> Гравець", userMessage);

//        FindFirstObjectByType<AIConnector>().AskAI(userMessage);

//        inputField.text = "";
//    }

//    internal void AddMessage(string sender, string message)
//    {
//        string formatted = $"<b>{sender}:</b> {message}";
//        messages.Add(formatted);

//        chatHistoryText.text = string.Join("\n", messages);
//        chatScroll.ScrollToBottom();
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
