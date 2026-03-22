using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class AIResponse
{
    public string response;

    // Додати поля, які реально повертає сервер
    public float trust_change;
    public float mood_change;
    public float mystery_change;

    // Ці поля для сумісності з твоїм UI
    public float trust => trust_change;
    public float mystery => mystery_change;
    public float mood => mood_change;
}

[System.Serializable]
public class AIRequest
{
    public string prompt;
    public GhostStats ghostStats;
    public string context;

    public AIRequest(string prompt, GhostStats stats, string context)
    {
        this.prompt = prompt;
        this.ghostStats = stats;
        this.context = context;
    }
}

[System.Serializable]
public class GhostStats
{
    public float trust = 0.5f;
    public float mystery = 0.8f;
    public float mood = 0.5f;

    public GhostStats() { }

    public GhostStats(float trust, float mood, float mystery)
    {
        this.trust = trust;
        this.mood = mood;
        this.mystery = mystery;
    }
}

public enum AIRequestType
{
    RPG,
    Education
}

public class AssistantAI : MonoBehaviour, ISaveable
{
    public static AssistantAI Instance;
    private SaveableEntity saveableEntity;

    [Header("Main Character NickName")]
    public string mainCharacterName;

    [Header("References")]
    public PlayerStats playerStats;
    public Inventory inventory;

    [Header("UI Elements")]
    public TMP_InputField inputField;
    public Button sendButton;
    public TextMeshProUGUI chatHistoryText;
    public ChatScroll chatScroll;
    public TMP_InputField inputEduField;
    public Button sendEduButton;
    public TextMeshProUGUI chatEduHistoryText;
    public ChatScroll chatEduScroll;

    [Header("Ghost Stats UI")]
    public Slider trustSlider;
    public Slider mysterySlider;
    public TextMeshProUGUI ghostMoodText;
    public Slider trustEduSlider;
    public Slider mysteryEduSlider;
    public TextMeshProUGUI ghostMoodEduText;

    [Header("Typing Settings")]
    public float typingSpeed = 0.02f;

    [Header("Ghost State")]
    public GhostStats ghostStats = new GhostStats();

    private List<string> messagesRPG = new List<string>();
    private List<string> messagesEdu = new List<string>();

    private TaskRequirement currentEduTask;

    private bool isWaitingResponse = false;
    float inactivityTimer = 0f;
    float inactivityLimit = 600f;
    bool inactivityActive = false;

    void Start()
    {
        sendButton.onClick.AddListener(OnSendMessage);
        sendEduButton.onClick.AddListener(OnSendEduMessage);
        ShowIntroMessage();
        UpdateGhostUI();
    }

    public void SetEducationTask(TaskRequirement task)
    {
        currentEduTask = task;
    }

    private void Awake()
    {
        Instance = this;
        saveableEntity = GetComponent<SaveableEntity>();

        if (saveableEntity == null) Debug.LogError("AssistantAI missing SaveableEntity!");
    }

    void Update()
    {
        if (!inactivityActive) return;

        inactivityTimer += Time.deltaTime;

        if (inactivityTimer >= inactivityLimit)
        {
            StartCoroutine(ResetAIConversation());
            inactivityActive = false;
        }
    }

    public string GetSaveID() => saveableEntity.ID;

    public object CaptureState()
    {
        return new GhostSaveData
        {
            trust = ghostStats.trust,
            mystery = ghostStats.mystery,
            mood = ghostStats.mood
        };
    }

    public void RestoreState(object state)
    {
        if (state is not GhostSaveData data)
            return;

        ghostStats.trust = data.trust;
        ghostStats.mystery = data.mystery;
        ghostStats.mood = data.mood;

        UpdateGhostUI();
    }


    private void ShowIntroMessage()
    {
        messagesRPG.Add($"<i> ...Ви чуєте відлуння... Дух готовий слухати вас...</i>");
        chatHistoryText.text = string.Join("\n", messagesRPG);
        chatScroll.ScrollToBottom();
        messagesEdu.Add($"<i> ...Чим я можу тобі допомогти...</i>");
        chatEduHistoryText.text = string.Join("\n", messagesEdu);
        chatEduScroll.ScrollToBottom();
    }

    public void SetPlayerName(string nickname)
    {
        mainCharacterName = string.IsNullOrWhiteSpace(nickname) ? "Гравець" : nickname;
    }

    private void OnSendMessage()
    {
        if (isWaitingResponse) return;
        inactivityTimer = 0f;

        string userMessage = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        AddMessage(messagesRPG, chatHistoryText, chatScroll, $">> {mainCharacterName}", userMessage);
        inputField.text = "";

        StartCoroutine(SendAIRequest(userMessage, AIRequestType.RPG, null));
    }

    private void OnSendEduMessage()
    {
        if (isWaitingResponse) return;
        inactivityTimer = 0f;

        string userMessage = inputEduField.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        AddMessage(messagesEdu, chatEduHistoryText, chatEduScroll, $">> {mainCharacterName}", userMessage);
        inputEduField.text = "";

        StartCoroutine(SendAIRequest(userMessage, AIRequestType.Education, currentEduTask));
    }

    IEnumerator ResetAIConversation()
    {
        string url = "http://localhost:5000/reset";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();
        }

        messagesRPG.Clear();
        messagesEdu.Clear();

        string resetText = "<i> ...Тиша огортає вас... Дух не пам'ятатиме вашу розмову...</i>";

        messagesRPG.Add(resetText);
        messagesEdu.Add(resetText);

        chatHistoryText.text = string.Join("\n", messagesRPG);
        chatScroll.ScrollToBottom();

        chatEduHistoryText.text = string.Join("\n", messagesEdu);
        chatEduScroll.ScrollToBottom();

        UpdateGhostUI();
    }

    private string BuildRPGPrompt(string prompt)
    {
        string lower = prompt.ToLower();

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("[Question]");
        sb.AppendLine(prompt);

        bool needsBoth = NeedsBoth(lower);
        bool needsStats = NeedsStats(lower);
        bool needsQuests = NeedsQuests(lower);
        bool needsLearning = NeedsLearning(lower);
        bool weakPlayer = NeedsLearningFromStats();
        bool needsSubjectLearning = NeedsSubjectLearning(lower);

        if (needsBoth)
        {
            sb.AppendLine("\n[Player Stats]");
            sb.AppendLine(GetStatsSummary());

            sb.AppendLine("\n[Quests]");
            sb.AppendLine(GetQuestSummary());
        }
        else
        {
            if (needsStats)
            {
                sb.AppendLine("\n[Player Stats]");
                sb.AppendLine(GetStatsSummary());
            }

            if (needsQuests)
            {
                sb.AppendLine("\n[Quests]");
                sb.AppendLine(GetQuestSummary());
            }
        }

        if (needsLearning || weakPlayer)
        {
            sb.AppendLine("\n[Player Learning]");
            sb.AppendLine(GetLearningSummary());

            sb.AppendLine("\n[Instruction]");
            sb.AppendLine("Adapt your explanation to the player's knowledge level, if needed.");
        }

        if (needsSubjectLearning)
        {
            sb.AppendLine("\n[Subject Learning]");
            sb.AppendLine(GetSubjectLearningSummary());

            sb.AppendLine("\n[Instruction]");
            sb.AppendLine("Adapt advice based on subject strengths and weaknesses, if needed.");
        }

        if (!needsStats && !needsQuests && !needsLearning)
        {
            sb.AppendLine("\n[Info]");
            sb.AppendLine("If you need player stats or quests to answer — say it.");
        }

        return sb.ToString();
    }

    private bool ContainsAny(string text, params string[] keywords)
    {
        foreach (var word in keywords)
        {
            if (text.Contains(word))
                return true;
        }
        return false;
    }

    private bool NeedsStats(string text)
    {
        return ContainsAny(text,
            "stat", "stats", "character stats", "my stats", "my build", "build",

            "level", "lvl", "exp", "experience",

            "health", "hp", "hit points",

            "stamina", "energy",

            "strength", "power", "damage",

            "agility", "dexterity", "speed",

            "endurance", "vitality",

            "intellect", "intelligence", "magic", "mana",

            "faith", "spirit",

            "attributes", "character sheet", "my character",

            "how strong am i", "how good is my",
            "am i strong", "am i weak"
        );
    }

    private bool NeedsQuests(string text)
    {
        return ContainsAny(text,
            "quest", "quests", "mission", "missions", "task", "tasks",

            "objective", "objectives", "goal", "goals", "target",

            "what should i do", "what do i do", "what to do",
            "what now", "what next", "what should i do next",

            "next step", "next steps",

            "where should i go", "where to go",

            "progress", "quest progress",

            "current quest", "active quest",

            "help me with quest", "stuck", "i'm stuck",

            "how to complete", "how to finish",

            "guide me", "what is my objective"
        );
    }

    private bool NeedsLearning(string text)
    {
        return ContainsAny(text,
            "learn", "learning",
            "explain", "explanation",
            "teach me",
            "i don't understand",
            "i dont understand",
            "why",
            "how does it work",
            "how it works",

            "help me understand",
            "can you explain",
            "explain this",

            "math", "science", "physics",
            "formula", "equation",

            "hard", "difficult",
            "easy", "too hard",

            "am i good at",
            "how am i doing",
            "my progress",
            "my knowledge",

            "test me",
            "quiz me"
        );
    }

    private bool NeedsBoth(string text)
    {
        return ContainsAny(text,
            "am i ready",
            "can i handle",
            "can i beat",
            "can i win",
            "should i go",
            "should i start",
            "am i strong enough",
            "is it too hard",
            "is this hard",
            "will i survive"
        );
    }

    private bool NeedsLearningFromStats()
    {
        var learning = PlayerLearningManager.Instance;
        if (learning == null) return false;

        var state = learning.State;

        int total = state.correctAnswers + state.wrongAnswers;
        if (total < 3) return false;

        float accuracy = (float)state.correctAnswers / total;

        return accuracy < 0.5f || state.streakWrong >= 3;
    }

    private string GetLearningSummary()
    {
        var learning = PlayerLearningManager.Instance;

        if (learning == null)
            return "No learning data.";

        var state = learning.State;

        int total = state.correctAnswers + state.wrongAnswers;
        float accuracy = total > 0 ? (float)state.correctAnswers / total : 0f;

        return
            $"[Global Learning State]\n" +
            $"Accuracy: {accuracy}\n" +
            $"Total Correct: {state.correctAnswers}\n" +
            $"Total Wrong: {state.wrongAnswers}\n" +
            $"Streak Correct: {state.streakCorrect}\n" +
            $"Streak Wrong: {state.streakWrong}\n";
    }

    private string GetSubjectLearningSummary()
    {
        var learning = PlayerLearningManager.Instance;

        if (learning == null)
            return "No subject learning data.";

        var state = learning.State;

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("[Subjects Knowledge]");

        foreach (var pair in state.subjectStats)
        {
            string subject = pair.Key.ToString();
            var stats = pair.Value;

            int total = stats.correct + stats.wrong;
            float baseKnowledge = total > 0
                ? (stats.correct + 0.5f * stats.streakSubjectCorrect) / (total + 1f)
                : 0f;

            float retention = GetRetention(stats, stability: 24f);

            float knowledge = stats.knowledge.pKnow * retention;

            sb.AppendLine($"\n[Subject Knowledge: {subject}]");
            sb.AppendLine($"BKT Knowledge: {stats.knowledge.pKnow:F2}");
            sb.AppendLine($"Correct: {stats.correct}");
            sb.AppendLine($"Wrong: {stats.wrong}");
            sb.AppendLine($"Streak Correct: {stats.streakSubjectCorrect}");
            sb.AppendLine($"Streak Wrong: {stats.streakSubjectWrong}");
        }

        return sb.ToString();
    }

    private string GetSubjectLearningSummary(TaskRequirement task)
    {
        var learning = PlayerLearningManager.Instance;

        if (learning == null || task == null)
            return "No subject learning data.";

        var state = learning.State;

        if (!state.subjectStats.TryGetValue(task.subject, out var stats))
        {
            stats = new SubjectStats();
        }

        int total = stats.correct + stats.wrong;
        float baseKnowledge = total > 0
            ? (stats.correct + 0.5f * stats.streakSubjectCorrect) / (total + 1f)
            : 0f;

        float retention = GetRetention(stats, stability: 24f);
        float knowledge = stats.knowledge.pKnow * retention;

        string level = knowledge < 0.3f ? "weak" :
               knowledge < 0.7f ? "medium" : "strong";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[Subject Knowledge: {task.subject}]");
        sb.AppendLine($"BKT Knowledge: {stats.knowledge.pKnow:F2}");
        sb.AppendLine($"[Knowledge Level] {level}");
        sb.AppendLine($"Correct: {stats.correct}");
        sb.AppendLine($"Wrong: {stats.wrong}");
        sb.AppendLine($"Streak Correct: {stats.streakSubjectCorrect}");
        sb.AppendLine($"Streak Wrong: {stats.streakSubjectWrong}");
        sb.AppendLine($"Knowledge * Retention: {knowledge:F2}");
        sb.AppendLine($"Hints Used: {stats.hintsSubjectUsed}");

        return sb.ToString();
    }

    private bool NeedsSubjectLearning(string text)
    {
        return ContainsAny(text,
            "math", "language", "programming", "english",

            "am i good at",
            "how am i doing in",
            "my progress in",

            "which subject",
            "what should i learn",

            "where am i weak",
            "what am i bad at"
        );
    }

    // Модель Еббінгауза
    private float GetRetention(SubjectStats stats, float stability = 24f)
    {
        // Якщо гравець ще не проходив предмет — 0 знання
        if (stats.lastReviewed == DateTime.MinValue) return 0f;

        // Час в годинах від останнього повторення
        float hoursPassed = (float)(DateTime.Now - stats.lastReviewed).TotalHours;

        // Експоненційна крива забування
        float retention = Mathf.Exp(-hoursPassed / stability);

        return Mathf.Clamp01(retention);
    }

    private string BuildEducationPrompt(string prompt, TaskRequirement task)
    {
        var learning = PlayerLearningManager.Instance;

        if (learning == null || task == null) return $"[Question]\n{prompt}\n\nNo learning data.";

        var state = learning.State;

        SubjectStats subjectStats;

        if (!state.subjectStats.TryGetValue(task.subject, out subjectStats))
        {
            subjectStats = new SubjectStats();
        }

        string globalSummary = GetLearningSummary();
        string subjectSummary = GetSubjectLearningSummary(task);

        int total = state.correctAnswers + state.wrongAnswers;
        float accuracy = total > 0 ? (float)state.correctAnswers / total : 0f;

        return
            $"[Question]\n{prompt}\n\n" +

            $"[Task]\n" +
            $"Subject: {task.subject}\n" +
            $"Difficulty: {task.difficulty}\n" +
            $"Question: {task.questionText}\n\n" +

            $"[Player Learning Stats]\n" +
            $"{subjectSummary}\n\n" +

            $"[Global Learning]\n" +
            $"{globalSummary}\n\n" +

            $"[Knowledge Interpretation]\n" +
            $"Use BKT_Knowledge as the main indicator of player understanding.\n" +
            $"Adjust explanation depth accordingly.";
    }

    private IEnumerator SendAIRequest(string prompt, AIRequestType type, TaskRequirement task)
    {
        isWaitingResponse = true;

        if (type == AIRequestType.Education && task == null)
        {
            Debug.LogWarning("No task provided for Education AI");
            isWaitingResponse = false;
            yield break;
        }

        TMP_InputField activeInput = type == AIRequestType.RPG ? inputField : inputEduField;
        Button activeButton = type == AIRequestType.RPG ? sendButton : sendEduButton;
        TextMeshProUGUI activeText = type == AIRequestType.RPG ? chatHistoryText : chatEduHistoryText;
        ChatScroll activeScroll = type == AIRequestType.RPG ? chatScroll : chatEduScroll;
        List<string> activeMessages = type == AIRequestType.RPG ? messagesRPG : messagesEdu;

        activeButton.interactable = false;
        activeInput.interactable = false;

        string url = "http://localhost:5000/ask";
        string fullPrompt = type == AIRequestType.RPG
        ? BuildRPGPrompt(prompt)
        : BuildEducationPrompt(prompt, task);

        string json = JsonConvert.SerializeObject(new AIRequest(fullPrompt, ghostStats, type.ToString()));

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            activeMessages.Add($"<i>...Дух думає...</i>");
            activeText.text = string.Join("\n", activeMessages);
            activeScroll.ScrollToBottom();

            yield return www.SendWebRequest();

            string aiText = "";

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                aiText = "Щось не так... Зв'язок між нами нестабільний... Я поки не можу тобі допомогти...";
            }
            else
            {
                string jsonText = www.downloadHandler.text;

                AIResponse response = JsonConvert.DeserializeObject<AIResponse>(jsonText) ?? new AIResponse();

                aiText = string.IsNullOrEmpty(response.response) ? "<i>...Дух мовчить...</i>" : response.response;

                ghostStats.trust = Mathf.Clamp01(ghostStats.trust + response.trust_change);
                ghostStats.mood = Mathf.Clamp01(ghostStats.mood + response.mood_change);
                ghostStats.mystery = Mathf.Clamp01(ghostStats.mystery + response.mystery_change);

                UpdateGhostUI();
            }

            int msgIndex = activeMessages.Count - 1;
            activeMessages[msgIndex] = $"<b><< Дух:</b> ";
            activeText.text = string.Join("\n", activeMessages);
            activeScroll.ScrollToBottom();

            yield return StartCoroutine(TypeText(aiText, msgIndex, activeMessages, activeText, activeScroll));

            isWaitingResponse = false;
            activeButton.interactable = true;
            activeInput.interactable = true;
            activeInput.Select();
            activeInput.ActivateInputField();
            inactivityTimer = 0f;
            inactivityActive = true;
        }
    }

    private void UpdateGhostUI()
    {
        if (trustSlider != null) StartCoroutine(SmoothSlider(trustSlider, ghostStats.trust));
        if (mysterySlider != null) StartCoroutine(SmoothSlider(mysterySlider, ghostStats.mystery));
        if (ghostMoodText != null) ghostMoodText.text = MoodToText(ghostStats.mood);
        if (trustEduSlider != null) StartCoroutine(SmoothSlider(trustEduSlider, ghostStats.trust));
        if (mysteryEduSlider != null) StartCoroutine(SmoothSlider(mysteryEduSlider, ghostStats.mystery));
        if (ghostMoodEduText != null) ghostMoodEduText.text = MoodToText(ghostStats.mood);
    }

    private string MoodToText(float mood)
    {
        if (mood < 0.33f) return "Настрій духа: Засмучений";
        if (mood < 0.66f) return "Настрій духа: Спокійний";
        return "Настрій духа: Веселий";
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

    private IEnumerator TypeText(string text, int index, List<string> messages, TextMeshProUGUI textUI, ChatScroll scroll)
    {
        if (index < 0 || index >= messages.Count) yield break;
        StringBuilder sb = new StringBuilder(messages[index]);

        foreach (char c in text)
        {
            if (index < 0 || index >= messages.Count) yield break;

            sb.Append(c);
            messages[index] = sb.ToString();
            textUI.text = string.Join("\n", messages);
            scroll.ScrollToBottom();
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }

    private void AddMessage(List<string> list, TextMeshProUGUI textUI, ChatScroll scroll, string sender, string message)
    {
        list.Add($"<b>{sender}:</b> {message}");
        textUI.text = string.Join("\n", list);
        scroll.ScrollToBottom();
    }

    IEnumerator SmoothSlider(Slider slider, float target)
    {
        float start = slider.value;
        float time = 0f;

        while (time < 0.5f)
        {
            time += Time.deltaTime;
            slider.value = Mathf.Lerp(start, target, time / 0.5f);
            yield return null;
        }

        slider.value = target;
    }
}