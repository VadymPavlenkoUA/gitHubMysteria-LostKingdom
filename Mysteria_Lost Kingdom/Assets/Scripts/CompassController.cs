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

    [Header("QuestMarker")]
    public RectTransform questMarker;

    [Header("CompassSettings")]
    public float compassScale = 4;

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
            float posX = (angle / 180f) * halfWidth * compassScale;
            directionLabels[i].rectTransform.anchoredPosition = new Vector2(posX, 0);
        }
    }

    void UpdateQuestMarker()
    {
        var trackedQuest = QuestManager.Instance.trackedQuest;
        if (trackedQuest == null)
        {
            questMarker.gameObject.SetActive(false);
            return;
        }
        var step = trackedQuest.GetFirstIncompleteStep();
        if (step == null)
        {
            questMarker.gameObject.SetActive(false);
            return;
        }

        Transform target = null;
        Debug.Log($"{step.targetName}");
        if (!string.IsNullOrEmpty(step.targetName))
        {
            GameObject obj = GameObject.Find(step.targetName);
            if (obj != null) target = obj.transform;
        }
        else if (!string.IsNullOrEmpty(step.targetTag))
        {
            GameObject obj = GameObject.FindWithTag(step.targetTag);
            if (obj != null) target = obj.transform;
        }

        if (target == null)
        {
            questMarker.gameObject.SetActive(false);
            return;
        }

        questMarker.gameObject.SetActive(true);
        Vector3 dirToTarget = (target.position - cameraPlayer.position);
        dirToTarget.y = 0;
        Vector3 forward = new Vector3(cameraPlayer.forward.x, 0, cameraPlayer.forward.z).normalized;
        float angle = Vector3.SignedAngle(forward, dirToTarget.normalized, Vector3.up);
        float posX = Mathf.Clamp((angle / 180f) * halfWidth * compassScale, -halfWidth, halfWidth);
        questMarker.anchoredPosition = new Vector2(posX, 0);
    }
}
