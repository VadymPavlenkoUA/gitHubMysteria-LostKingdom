using UnityEngine;
using System;

public class TimeOfDayManager : MonoBehaviour
{
    public static TimeOfDayManager Instance;

    [Header("Time")]
    [Range(0f, 24f)]
    [SerializeField] internal float timeOfDay = 12f;
    [SerializeField] internal float dayLengthInMinutes = 20f;

    public bool IsNight => timeOfDay < 6f || timeOfDay >= 18f;

    public event Action<int> OnHourChanged;
    public event Action OnDayStarted;
    public event Action OnNightStarted;

    private int lastHour;
    private bool lastNightState;

    private void Awake()
    {
        Instance = this;
        lastHour = Mathf.FloorToInt(timeOfDay);
        lastNightState = IsNight;
    }

    private void Update()
    {
        float timeSpeed = 24f / (dayLengthInMinutes * 60f);
        timeOfDay += Time.deltaTime * timeSpeed;

        timeOfDay %= 24f;

        int hour = Mathf.FloorToInt(timeOfDay);
        if (hour != lastHour)
        {
            lastHour = hour;
            OnHourChanged?.Invoke(hour);
        }

        if (IsNight != lastNightState)
        {
            lastNightState = IsNight;
            if (IsNight) OnNightStarted?.Invoke();
            else OnDayStarted?.Invoke();
        }
    }
}
