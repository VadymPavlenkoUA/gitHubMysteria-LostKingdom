using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject mainMenu;
    public GameObject settings;
    public GameObject loads;

    [Header("Cameras")]
    public CinemachineCamera mainMenuCam;
    public CinemachineCamera settingsCam;
    public CinemachineCamera loadCam;
    public CinemachineCamera newGameCam;
    public CinemachineBrain brain;

    [Header("Fade Effect")]
    public CanvasGroup fadeCanvas; 
    public float fadeSpeed = 1.5f; 
    public void PlayGame()
    {
        StartCoroutine(StartGameSequence());
    }
    void OnEnable()
    {
        ResetGlobalState();
    }

    void ResetGlobalState()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (MenuController.Instance != null)
        {
            MenuController.Instance.inputBlocked = false;
        }
    }
    private IEnumerator StartGameSequence()
    {
        mainMenu.SetActive(false);
        mainMenuCam.Priority = 0;
        newGameCam.Priority = 10;

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeToBlack());

        //SceneLoader.LoadScene("MainScene");
        SceneLoader.isNewGame = true;
        SceneLoader.newGameCustomization = null;
        SceneLoader.LoadScene("CharacterCreation");
    }
    private IEnumerator FadeToBlack()
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
    public void Settings()
    {
        StartCoroutine(SwitchToSettings());
    }
    private IEnumerator SwitchToSettings()
    {
        mainMenu.SetActive(false);
        mainMenuCam.Priority = 0;
        settingsCam.Priority = 10;

        yield return new WaitForSeconds(2f);

        settings.SetActive(true);
    }
    public void ReturnSettings()
    {
        StartCoroutine(SwitchToMenuSettings());
    }
    private IEnumerator SwitchToMenuSettings()
    {
        settings.SetActive(false);
        mainMenuCam.Priority = 10;
        settingsCam.Priority = 0;

        yield return new WaitForSeconds(2f);

        mainMenu.SetActive(true);
    }
    public void Loads()
    {
        StartCoroutine(SwitchToLoads());
    }
    public void ReturnLoads()
    {
        StartCoroutine(SwitchToMenuLoads());
    }
    private IEnumerator SwitchToLoads()
    {
        mainMenu.SetActive(false);
        mainMenuCam.Priority = 0;
        loadCam.Priority = 10;

        yield return new WaitForSeconds(2f);

        loads.SetActive(true);
    }
    private IEnumerator SwitchToMenuLoads()
    {
        loads.SetActive(false);
        mainMenuCam.Priority = 10;
        loadCam.Priority = 0;

        yield return new WaitForSeconds(2f);

        mainMenu.SetActive(true);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
