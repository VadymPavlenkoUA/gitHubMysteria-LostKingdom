using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class LoadingManager : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup fadeCanvas;
    public Slider progressBar;
    public TMP_Text progressText;

    [Header("Settings")]
    public float fadeSpeed = 1.5f;
    public float minLoadingTime = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string sceneToLoad = SceneLoader.nextSceneName;
        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        yield return StartCoroutine(FadeIn());

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;

        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;
            progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

            if (operation.progress >= 0.9f && timer >= minLoadingTime)
            {
                yield return StartCoroutine(FadeOut());
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        fadeCanvas.alpha = 1f;
        fadeCanvas.gameObject.SetActive(true);

        while (fadeCanvas.alpha > 0f)
        {
            fadeCanvas.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        fadeCanvas.alpha = 0f;
    }

    private IEnumerator FadeOut()
    {
        fadeCanvas.gameObject.SetActive(true);
        fadeCanvas.alpha = 0f;

        while (fadeCanvas.alpha < 1f)
        {
            fadeCanvas.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        fadeCanvas.alpha = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
