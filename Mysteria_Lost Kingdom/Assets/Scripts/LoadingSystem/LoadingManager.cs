using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class LoadingManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider progressBar;
    public TMP_Text progressText;

    [Header("Settings")]
    public float fadeSpeed = 1.5f;
    public float minLoadingTime = 1.5f;

    [Header("Background")]
    public Image backgroundImage;     
    public Sprite[] loadingBackgrounds;

    private bool isLoading = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (loadingBackgrounds != null && loadingBackgrounds.Length > 0 && backgroundImage != null)
        {
            int index = Random.Range(0, loadingBackgrounds.Length);
            backgroundImage.sprite = loadingBackgrounds[index];
        }

        Debug.Log($"[LOADING] LoadingManager started in scene: {SceneManager.GetActiveScene().name}");
        if (isLoading) return;

        isLoading = true;
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.BringToFront();
            ScreenFader.Instance.FadeInInstant();
        }
        string sceneToLoad = SceneLoader.nextSceneName;
        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        ScreenFader.Instance.FadeOut();

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
                yield return ScreenFader.Instance.FadeTo(1f);
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
