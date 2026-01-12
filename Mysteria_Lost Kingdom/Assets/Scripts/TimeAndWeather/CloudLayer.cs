using UnityEngine;

public class CloudLayer : MonoBehaviour
{
    [Header("Movement")]
    public Vector2 speed = new Vector2(0.001f, 0.0005f);

    [Header("Material")]
    public Material cloudMaterial;

    [Header("Time Of Day Colors")]
    public Color dayColor = new Color(1f, 1f, 1f, 0.5f);
    public Color nightColor = new Color(0.4f, 0.4f, 0.5f, 0.35f);

    private Vector2 offset;

    void Update()
    {
        offset += speed * Time.deltaTime;
        cloudMaterial.mainTextureOffset = offset;

        float t = TimeOfDayManager.Instance.timeOfDay / 24f;
        t = Mathf.SmoothStep(0f, 1f, t);
        Color c = Color.Lerp(nightColor, dayColor, t);
        cloudMaterial.color = c;
    }
}
