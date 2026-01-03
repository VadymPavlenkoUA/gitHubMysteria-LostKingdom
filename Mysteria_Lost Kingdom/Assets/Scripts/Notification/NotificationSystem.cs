using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance;

    [Header("Setup")]
    public Transform notificationContainer;
    public GameObject notificationPrefab;
    public int maxNotifications = 3;

    [Header("Icons")]
    public Sprite questSprite;
    public Sprite expSprite;

    private readonly List<NotificationUI> activeNotifications = new();

    private void Awake()
    {
        Instance = this;
    }

    public void ShowNotification(Sprite icon, string message)
    {
        if (activeNotifications.Count >= maxNotifications)
        {
            Destroy(activeNotifications[0].gameObject);
            activeNotifications.RemoveAt(0);
        }

        GameObject go = Instantiate(notificationPrefab, notificationContainer);
        NotificationUI ui = go.GetComponent<NotificationUI>();
        ui.onDestroyed = HandleNotificationDestroyed;
        ui.Setup(icon, message);

        activeNotifications.Add(ui);

        UpdateTransparency();
    }

    private void HandleNotificationDestroyed(NotificationUI ui)
    {
        if (activeNotifications.Contains(ui))
        {
            activeNotifications.Remove(ui);
            UpdateTransparency();
        }
    }

    private void UpdateTransparency()
    {
        for (int i = 0; i < activeNotifications.Count; i++)
        {
            float alpha = 1f - (i * 0.2f); // нові = 1, старіші = 0.7, ще старіші = 0.4
            activeNotifications[i].SetAlpha(alpha);
        }
    }
}
