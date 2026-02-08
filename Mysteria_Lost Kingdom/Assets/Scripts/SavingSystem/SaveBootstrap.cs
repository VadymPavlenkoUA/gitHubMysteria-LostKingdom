using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Cinemachine;

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

        ScreenFader.Instance.FadeOut();

        SceneLoader.loadFromSave = false;
        SceneLoader.saveSlot = -1;
    }
}
