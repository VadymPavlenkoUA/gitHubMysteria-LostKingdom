//using UnityEngine;
//using UnityEngine.Networking;
//using System.Collections;

//[System.Serializable]
//public class AIResponse
//{
//    public string response;
//}

//[System.Serializable]
//public class AIRequest
//{
//    public string prompt;
//    public AIRequest(string prompt)
//    {
//        this.prompt = prompt;
//    }
//}

//public class AIConnector : MonoBehaviour
//{
//    public AssistantAI assistantAI;

//    public void AskAI(string prompt)
//    {
//        StartCoroutine(SendRequest(prompt));
//    }

//    private IEnumerator SendRequest(string prompt)
//    {
//        string url = "http://localhost:5000/ask";
//        string json = JsonUtility.ToJson(new AIRequest(prompt));

//        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
//        {
//            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
//            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
//            www.downloadHandler = new DownloadHandlerBuffer();
//            www.SetRequestHeader("Content-Type", "application/json");

//            yield return www.SendWebRequest();

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                Debug.LogError(www.error);
//                assistantAI.AddMessage("<< Привид", "Вибач, я зараз не можу відповісти.");
//            }
//            else
//            {
//                AIResponse response = JsonUtility.FromJson<AIResponse>(www.downloadHandler.text);
//                assistantAI.AddMessage("<< Привид", response.response);
//            }
//        }
//    }
//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
