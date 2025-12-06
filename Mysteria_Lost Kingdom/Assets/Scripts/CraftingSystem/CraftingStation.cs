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
            UseActionManager.Instance.StartUse(2f, () => CraftingUIManager.Instance.OpenFromStation(stationType), () => Debug.Log("Скасовано!"));
        }
        else
        {
            Debug.LogWarning("CraftingUIManager ще не готовий!");
        }
    }
}
