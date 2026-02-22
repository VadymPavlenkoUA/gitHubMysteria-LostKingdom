using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    public static string nextSceneName;
    public static string newGameNickName;
    public static int saveSlot = -1;
    public static bool loadFromSave = false;
    public static bool returnToMenu = false;

    public static bool isNewGame = false;
    public static CharacterCustomizationData newGameCustomization;
    public static void LoadScene(string sceneName)
    {
        loadFromSave = false;
        saveSlot = -1;
        nextSceneName = sceneName;
        SceneManager.LoadScene("Loading");
    }

    public static void LoadGameFromSave(string sceneName, int slot)
    {
        nextSceneName = sceneName;
        saveSlot = slot;
        loadFromSave = true;

        Time.timeScale = 1f;
        //ScreenFader.Instance.FadeAndLoad();
        SceneManager.LoadScene("Loading");
    }
}
