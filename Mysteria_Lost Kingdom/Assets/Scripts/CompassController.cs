using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CompassController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPlayer;
    public RectTransform compassLine;

    [Header("DirectionLabels")]
    public TextMeshProUGUI[] directionLabels;

    private readonly float[] directionAngels = { 0, 45, 90, 135, 180, -135, -90, -45 };

    private float halfWidth;

    [Header("Quest Marker")]
    public RectTransform questMarker;
    public Transform questTarget;
    public bool showQuestMarker = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        halfWidth = compassLine.rect.width / 2f;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDirectionLabels();
        UpdateQuestMarker();
    }

    void UpdateDirectionLabels()
    {
        Vector3 forward = new Vector3(cameraPlayer.forward.x, 0, cameraPlayer.forward.z).normalized;
        for (int i = 0; i < directionLabels.Length; i++)
        {
            Vector3 dir = Quaternion.Euler(0, directionAngels[i], 0) * Vector3.forward;
            float angle = Vector3.SignedAngle(forward, dir, Vector3.up);
            float posX = (angle / 180f) * halfWidth;
            directionLabels[i].rectTransform.anchoredPosition = new Vector2(posX, 0);
        }
    }

    void UpdateQuestMarker()
    {
        if (!showQuestMarker || questTarget == null)
        {
            questMarker.gameObject.SetActive(false);
            return;
        }

        questMarker.gameObject.SetActive(true);

        Vector3 forward = new Vector3(cameraPlayer.forward.x, 0, cameraPlayer.forward.z).normalized;
        Vector3 dirToTarget = (questTarget.position - cameraPlayer.position);
        dirToTarget.y = 0;
        dirToTarget.Normalize();
        float angle = Vector3.SignedAngle(forward, dirToTarget, Vector3.up);
        float posX = Mathf.Clamp((angle / 180f) * halfWidth, -halfWidth, halfWidth);
        questMarker.anchoredPosition = new Vector2(posX, 0);
    }

    public void ShowQuestMarker(Transform target)
    {
        questTarget = target;
        showQuestMarker = true;
    }

    public void HideQuestMarker()
    {
        showQuestMarker = false;
    }
}
