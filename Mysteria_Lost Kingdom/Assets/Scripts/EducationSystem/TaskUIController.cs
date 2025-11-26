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
    public TMP_Text subjectTitle;
    public TMP_Text questionText;
    public TMP_Text timerText;
    public TMP_Text feedbackText;
    public Button submitButton;
    public Button hintButton;
    public Button skipButton;
    public Image hintCooldownOverlay;
    public Slider progressBar;

    [Header("Renderers")]
    public MathRenderer mathRenderer;
    public EnglishRenderer englishRenderer;
    public ProgrammingRenderer programmingRenderer;

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
        questionText.text = task.questionText;
        subjectTitle.text = task.subject.ToString();
        //progressBar.value = progress;
        hintUsed = false;
        hintCooldownOverlay.fillAmount = 0f;
        SetupRendererForTask(task);
        startTime = Time.time;
        //animator.Play("InProgress");
        timerText.text = "";
        feedbackText.text = "";
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

    private void OnSubmit()
    {
        string answer = activeRenderer.GetAnswer();
        float timeTaken = Time.time - startTime;
        var result = TaskValidator.Validate(currentTask, answer, timeTaken);
        result.givenAnswer = answer;

        if (result.correct)
        {
            //animator.SetTrigger("Success");
            //sfxSource.PlayOneShot(correctSfx);
        }
        else
        {
            //animator.SetTrigger("Fail");
            //sfxSource.PlayOneShot(wrongSfx);
        }

        StartCoroutine(FinishAfterDelay(result, 0.65f));
    }

    private IEnumerator FinishAfterDelay(TaskResult result, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        OnTaskComplete?.Invoke(result);
        MenuController.Instance.HideEducationMenu();
    }

    private void OnHint()
    {
        if (hintUsed) return;
        hintUsed = true;
        //sfxSource.PlayOneShot(hintSfx);
        feedbackText.text = currentTask.hint;
        StartCoroutine(HintCooldownCoroutine(10f));
    }

    private IEnumerator HintCooldownCoroutine(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            hintCooldownOverlay.fillAmount = t / seconds;
            yield return null;
        }
    }

    private void OnSkip()
    {
        var result = new TaskResult { correct = false, timeTaken = Time.time - startTime, pointsAwarded = 0, givenAnswer = "" };
        //animator.SetTrigger("Fail");
        //sfxSource.PlayOneShot(wrongSfx);
        StartCoroutine(FinishAfterDelay(result, 0.4f));
    }

    public void Hide()
    {
        MenuController.Instance.HideEducationMenu();
    }
}
