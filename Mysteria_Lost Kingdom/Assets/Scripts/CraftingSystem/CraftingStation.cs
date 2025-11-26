using UnityEngine;

public class CraftingStation : MonoBehaviour, IInteractable
{
    public CraftingStationType stationType;
    public string stationName = "Крафтова станція";

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
            CraftingUIManager.Instance.OpenFromStation(stationType);
        }
        else
        {
            Debug.LogWarning("CraftingUIManager ще не готовий!");
        }
    }
}
