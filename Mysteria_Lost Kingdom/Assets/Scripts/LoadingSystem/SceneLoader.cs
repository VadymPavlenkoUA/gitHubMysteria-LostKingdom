using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    public static string nextSceneName;
    public static int saveSlot = -1;
    public static bool loadFromSave = false;
    public static void LoadScene(string sceneName)
    {
        loadFromSave = false;
        nextSceneName = sceneName;
        SceneManager.LoadScene("Loading");
    }

    //public static void LoadFromSave(string sceneName, int slot)
    //{
    //    loadFromSave = true;
    //    saveSlot = slot;
    //    nextSceneName = sceneName;
    //    SceneManager.LoadScene("Loading");
    //}
}
