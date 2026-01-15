using UnityEngine;

public class TimeWheelUI : MonoBehaviour
{
    [SerializeField] private RectTransform wheel;

    void Update()
    {
        if (!TimeOfDayManager.Instance) return;

        float time = TimeOfDayManager.Instance.timeOfDay;
        float t = time / 24f;

        float angle = t * 360f - 90;

        wheel.localRotation = Quaternion.Lerp(wheel.localRotation, Quaternion.Euler(0f, 0f, -angle), Time.deltaTime * 3f);
    }
}
