using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    public static string nextSceneName;
    public static void LoadScene(string sceneName)
    {
        nextSceneName = sceneName;
        SceneManager.LoadScene("Loading");
    }
}
