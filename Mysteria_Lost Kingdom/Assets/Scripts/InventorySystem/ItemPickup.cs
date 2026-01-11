using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    public Item item;
    public int amount = 1;

    public ItemInstance instance;

    public void Init(ItemInstance inst)
    {
        instance = inst;
        item = inst.item;
        amount = Mathf.Max(1, inst.count);
    }

    public void Init(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
        instance = null;
    }

    public string GetInteractionNameText()
    {
        if (amount < 1) amount = 1;
        return $"{item.itemName} {amount}x";
    }

    public string GetInteractionBTNText()
    {
        return $"Натисніть \"E\"";
    }

    public void Interact()
    {
        var inv = FindAnyObjectByType<PlayerInventory>();
        var playerAudio = FindAnyObjectByType<PlayerAudio>();

        if (item.isUnique)
        {
            inv.PickUpInstance(instance);
        }
        else
        {
            inv.PickUpItem(item, amount);
        }
        playerAudio.PlayItemSound(item.categories, ItemAction.Pickup);
        NotificationSystem.Instance.ShowNotification(item.icon, $"Підібрано {item.itemName} x{amount}");
        Destroy(gameObject);
    }
}
