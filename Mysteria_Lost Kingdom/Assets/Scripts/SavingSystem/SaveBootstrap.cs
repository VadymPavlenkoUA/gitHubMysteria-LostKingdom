using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveBootstrap : MonoBehaviour
{
    private static SaveBootstrap instance;
    private CinemachineBrain brain;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (scene.name != SceneLoader.nextSceneName) return;

        if (SceneLoader.isNewGame && !SceneLoader.loadFromSave)
        {
            StartCoroutine(SetupNewGame());
            return;
        }

        if (!SceneLoader.loadFromSave)
        {
            ScreenFader.Instance.SendToBack();
            ScreenFader.Instance.FadeOut();
            return;
        }

        var cam = Camera.main;
        if (cam != null) brain = cam.GetComponent<CinemachineBrain>();

        StartCoroutine(LoadAfterSceneReady());
    }

    private IEnumerator LoadAfterSceneReady()
    {
        ScreenFader.Instance.SendToBack();
        if (brain != null)
        {
            brain.enabled = false;
        }

        yield return null;
        yield return new WaitForEndOfFrame();

        SaveManager.Instance.LoadGame(SceneLoader.saveSlot);

        yield return null;

        if (brain != null)
        {
            brain.enabled = true;
        }

        yield return null;

        var mainPlayers = FindObjectsByType<CharacterCustomizer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(p => !p.isRTCharacter);

        foreach (var player in mainPlayers)
        {
            RTCharacterManager.Instance?.ApplyCustomizationFromMain(player);
        }

        MenuController.Instance?.ForceResumeGameState();

        ScreenFader.Instance.FadeOut();

        SceneLoader.loadFromSave = false;
        SceneLoader.saveSlot = -1;
    }

    private IEnumerator SetupNewGame()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        var allPlayers = FindObjectsByType<CharacterCustomizer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var player in allPlayers)
        {
            if (SceneLoader.newGameCustomization != null)
            {
                player.ApplyCustomization(SceneLoader.newGameCustomization);
            }
        }

        ScreenFader.Instance?.FadeOut();

        SceneLoader.isNewGame = false;
    }
}
