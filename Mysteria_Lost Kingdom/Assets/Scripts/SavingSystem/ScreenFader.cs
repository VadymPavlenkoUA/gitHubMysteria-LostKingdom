using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private Canvas canvas;
    private int defaultSortingOrder;

    private Coroutine currentFade;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvas != null) defaultSortingOrder = canvas.sortingOrder;
    }

    public void FadeInInstant()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void FadeOutInstant()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void FadeIn()
    {
        StartFade(1f);
    }

    public void FadeOut()
    {
        StartFade(0f);
    }

    private void StartFade(float target)
    {
        if (currentFade != null) StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeRoutine(target));
    }

    public IEnumerator FadeTo(float target)
    {
        if (currentFade != null) StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeRoutine(target));
        yield return currentFade;
    }


    private IEnumerator FadeRoutine(float target)
    {
        canvasGroup.blocksRaycasts = true;

        while (Mathf.Abs(canvasGroup.alpha - target) > 0.01f)
        {
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha,
                target,
                Time.deltaTime * fadeSpeed
            );
            yield return null;
        }

        canvasGroup.alpha = target;

        if (target == 0f) canvasGroup.blocksRaycasts = false;

        currentFade = null;
    }

    public void BringToFront()
    {
        if (canvas != null)
        {
            canvas.sortingOrder = 999;
        }

        transform.SetAsLastSibling();
    }

    public void SendToBack()
    {
        if (canvas == null) return;

        canvas.sortingOrder = defaultSortingOrder;
        transform.SetAsFirstSibling();
    }

    public void FadeAndLoad()
    {
        BringToFront();
        StartCoroutine(FadeAndLoadRoutine());
    }

    private IEnumerator FadeAndLoadRoutine()
    {
        yield return FadeTo(1f);
        SceneManager.LoadScene("Loading");
    }

}
