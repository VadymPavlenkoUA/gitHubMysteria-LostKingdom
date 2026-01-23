using UnityEngine;
using TMPro;
using JetBrains.Annotations;

public class DialogueHistoryUI : MonoBehaviour
{
    [Header("References")]
    public DialogueHistoryManager historyManager;
    public Transform contentParent;
    public GameObject entryPrefab;

    private void OnEnable()
    {
        RefreshHistory();
    }

    public void RefreshHistory()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        foreach (var entry in historyManager.entries)
        {
            var go = Instantiate(entryPrefab, contentParent);
            var text = go.GetComponent<TMP_Text>();

            if (entry.isPlayer)
                text.text = $"<color=#2A4B8D><b>ерэъ:</b></color> {entry.text}";
            else
                text.text = $"<color=#FF6C00><b>{entry.speakerName}:</b></color> {entry.text}";
        }
    }

    public void ClearHistory()
    {
        historyManager.ClearHistory();
        RefreshHistory();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
