using UnityEngine;

public class CraftingStation : MonoBehaviour, IInteractable, IClosableInteraction
{
    public CraftingStationType stationType;
    public string stationName = "Крафтова станція";
    public Transform InteractionTransform => transform;

    public void OnInteractionClosed()
    {
        if (!MenuController.Instance.isGMOpen) return;

        CraftingUIManager.Instance.UpdateCloseCraftUI();
        MenuController.Instance.OpenGameMenu();
        Debug.Log("Крафт закрито через відстань");
    }

    public string GetInteractionNameText()
    {
        return $"Використати {stationName}";
    }

    public string GetInteractionBTNText()
    {
        return "Натисніть \"E\"";
    }

    public void Interact()
    {
        if (CraftingUIManager.Instance != null)
        {
            UseActionManager.Instance.StartUse(2f, () => 
            {
                CraftingUIManager.Instance.OpenFromStation(stationType);
                InteractionDistanceWatcher.Instance.StartWatching(this);
            }, 
            () => Debug.Log("Скасовано!")
);
        }
        else
        {
            Debug.LogWarning("CraftingUIManager ще не готовий!");
        }
    }
}
