using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        OllamaManager.StartOllama();
    }

    void OnApplicationQuit()
    {
        OllamaManager.StopOllama();
    }
}
