using System;
using System.Collections;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class TaskUIController : MonoBehaviour
{
    [Header("References")]
    public Image background;
    public Image subjectIcon;
    public Image feedBackImage;
    public Image levelImage;
    public TMP_Text subjectTitle;
    public TMP_Text questionText;
    public TMP_Text timerText;
    public TMP_Text feedbackText;
    public TMP_Text levelText;
    public Button submitButton;
    public Button hintButton;
    public Button skipButton;
    public Image hintCooldownOverlay;
    public Slider progressBar;

    [Header("Renderers")]
    public MathRenderer mathRenderer;
    public EnglishRenderer englishRenderer;
    public ProgrammingRenderer programmingRenderer;

    public Sprite mathImage;
    public Sprite programmingImage;
    public Sprite englishImage;
    public Sprite hintImage;
    public Sprite wrongImage;
    public Sprite correctImage;

    public Animator animator;
    public AudioSource sfxSource;
    public AudioClip correctSfx;
    public AudioClip wrongSfx;
    public AudioClip hintSfx;

    private TaskRequirement currentTask;
    private ITaskRenderer activeRenderer;
    private float startTime;
    private bool hintUsed;

    public event Action<TaskResult> OnTaskComplete;
    private Coroutine timerRoutine;

    private void Awake()
    {
        submitButton.onClick.AddListener(OnSubmit);
        hintButton.onClick.AddListener(OnHint);
        skipButton.onClick.AddListener(OnSkip);
    }

    public void ShowTask(TaskRequirement task, float progress = 0f)
    {
        MenuController.Instance.ShowEducationMenu();
        currentTask = task;
        if (task.subject == SubjectType.Math)
        {
            subjectTitle.text = "Математика";
            subjectTitle.color = Color.darkRed;
            subjectIcon.sprite = mathImage;
        }
        else if (task.subject == SubjectType.English)
        {
            subjectTitle.text = "Англійська мова";
            subjectTitle.color = Color.darkBlue;
            subjectIcon.sprite = englishImage;
        }
        else if (task.subject == SubjectType.Programming)
        {
            subjectTitle.text = "Програмування";
            subjectTitle.color = Color.darkGreen;
            subjectIcon.sprite = programmingImage;
        }
        else
        {
            subjectTitle.text = "Невідомо";
            subjectTitle.color = Color.grey;
            subjectIcon.sprite = null;
        }
        if (task.difficulty <= 3)
        {
            levelImage.color = Color.green;
            levelText.color = Color.green;
            levelText.text = "Легко";
        }
        else if (task.difficulty > 3 && task.difficulty <= 7)
        {
            levelImage.color = Color.orange;
            levelText.color = Color.orange;
            levelText.text = "Помірно";
        }
        else if (task.difficulty > 7 && task.difficulty <= 10)
        {
            levelImage.color = Color.red;
            levelText.color= Color.red;
            levelText.text = "Важко";
        }
        else
        {
            levelImage.color = Color.grey;
            levelText.text = "Невідомо";
        }
        questionText.text = task.questionText;
        //subjectTitle.text = task.subject.ToString();
        //progressBar.value = progress;
        hintUsed = false;
        hintCooldownOverlay.fillAmount = 100f;
        SetupRendererForTask(task);
        startTime = Time.time;
        //animator.Play("InProgress");
        timerText.text = "";
        feedbackText.text = "";
        feedBackImage.gameObject.SetActive(false);
        submitButton.interactable = true;
        skipButton.interactable = true;
        hintButton.interactable = true;

        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        if (task.timeLimit > 0)
        {
            timerRoutine = StartCoroutine(TaskTimerCoroutine(task.timeLimit));
        }
        else
        {
            timerText.text = "--:--:--";
        }
    }

    private void SetupRendererForTask(TaskRequirement task)
    {
        if (activeRenderer != null) activeRenderer.Clear();

        switch (task.subject)
        {
            case SubjectType.Math:
                activeRenderer = mathRenderer;
                break;

            case SubjectType.English:
                activeRenderer = englishRenderer;
                break;

            case SubjectType.Programming:
                activeRenderer = programmingRenderer;
                break;
        }

        activeRenderer.Render(task);
    }

    private string FormatTime(float time)
    {
        int total = Mathf.CeilToInt(time);
        int hours = total / 3600;
        int minutes = (total % 3600) / 60;
        int seconds = total % 60;

        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    private IEnumerator TaskTimerCoroutine(float timeLimit)
    {
        float remaining = timeLimit;
        while (remaining > 0f)
        {
            remaining -= Time.unscaledDeltaTime;
            if (remaining < 0) remaining = 0;
            timerText.text = FormatTime(remaining);
            yield return null;
        }

        var failResult = new TaskResult
        {
            correct = false,
            timeTaken = Time.time - startTime,
            pointsAwarded = 0,
            givenAnswer = ""
        };
        sfxSource.PlayOneShot(wrongSfx);
        StartCoroutine(FinishAfterDelay(failResult, 3f, "Час вийшов!", wrongImage));

    }

    private void OnSubmit()
    {
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        string answer = activeRenderer.GetAnswer();
        float timeTaken = Time.time - startTime;
        var result = TaskValidator.Validate(currentTask, answer, timeTaken);
        result.givenAnswer = answer;

        if (result.correct)
        {
            //animator.SetTrigger("Success");
            sfxSource.PlayOneShot(correctSfx);
        }
        else
        {
            //animator.SetTrigger("Fail");
            sfxSource.PlayOneShot(wrongSfx);
        }

        StartCoroutine(FinishAfterDelay(result, 3f, result.correct ? "Правильна відповідь!" : "Невірна відповідь!", result.correct ? correctImage : wrongImage));
    }

    private IEnumerator FinishAfterDelay(TaskResult result, float delay, string feedBack = "", Sprite feedBackIcon = null)
    {
        submitButton.interactable = false;
        skipButton.interactable = false;
        hintButton.interactable = false;
        if (!string.IsNullOrEmpty(feedBack))
        {
            feedbackText.text = feedBack;
            feedBackImage.gameObject.SetActive(true);
            feedBackImage.sprite = feedBackIcon;
        }
        yield return new WaitForSecondsRealtime(delay);
        OnTaskComplete?.Invoke(result);
        MenuController.Instance.HideEducationMenu();
    }

    private void OnHint()
    {
        if (hintUsed) return;
        hintUsed = true;
        sfxSource.PlayOneShot(hintSfx);
        feedBackImage.gameObject.SetActive(true);
        feedBackImage.sprite = hintImage;
        feedbackText.text = currentTask.hint;

        hintButton.interactable = false;
        hintCooldownOverlay.fillAmount = 0f;

        StartCoroutine(HintCooldownCoroutine(10f));
    }

    private IEnumerator HintCooldownCoroutine(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            hintCooldownOverlay.fillAmount = t / seconds;
            yield return null;
        }
        hintButton.interactable = true;
    }

    private void OnSkip()
    {
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        var result = new TaskResult { correct = false, timeTaken = Time.time - startTime, pointsAwarded = 0, givenAnswer = "" };
        //animator.SetTrigger("Fail");
        sfxSource.PlayOneShot(wrongSfx);
        StartCoroutine(FinishAfterDelay(result, 2f, "Завдання пропущено!", wrongImage));
    }

    public void Hide()
    {
        MenuController.Instance.HideEducationMenu();
    }
}
