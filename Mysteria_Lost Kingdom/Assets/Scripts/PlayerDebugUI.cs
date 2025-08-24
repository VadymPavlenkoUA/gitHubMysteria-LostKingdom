using UnityEngine;
using TMPro;

public class PlayerDebugUI : MonoBehaviour
{
    public Transform player;
    public TextMeshProUGUI debugText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null && debugText != null)
        {
            Vector3 pos = player.position;
            debugText.text = $"Player Pos: {pos.x:F2}, {pos.y:F2}, {pos.z:F2}";
        }
    }
}
