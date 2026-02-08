using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable, ISaveable
{
    public Item item;
    public int amount = 1;

    public ItemInstance instance;

    private bool pickedUp = false;

    public bool isDropped = false;

    private SaveableEntity saveableEntity;

    private void Awake()
    {
        saveableEntity = GetComponent<SaveableEntity>();
        if (saveableEntity == null)
        {
            Debug.LogError($"[ItemPickup] Missing SaveableEntity on {gameObject.name}");
        }
    }

    public string GetSaveID() => saveableEntity.ID;

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

        if (isDropped && DroppedItemRegistry.Instance != null)
        {
            DroppedItemRegistry.Instance.Unregister(GetSaveID());
        }

        pickedUp = true;
        HidePickup();
    }

    public object CaptureState()
    {
        ItemPickupSaveData data = new ItemPickupSaveData
        {
            pickedUp = pickedUp,
            position = transform.position,
            rotation = transform.rotation
        };

        if (item != null)
        {
            data.itemID = item.itemID;
            data.amount = amount;

            if (item.isUnique && instance != null)
            {
                data.instanceData = new ItemInstanceSaveData
                {
                    itemID = instance.item.itemID,
                    count = instance.count,
                    durability = instance.currentDurability,
                    damage = instance.currentDamage,
                    balanceDamage = instance.currentBalanceDamage,
                    defenseMultiplier = instance.currentDefenseMultiplier,
                    armor = instance.currentArmor
                };
            }
        }

        return data;
    }


    public void RestoreState(object state)
    {
        Debug.Log($"[ItemPickup] RestoreState called on {name}");

        if (state is not ItemPickupSaveData data)
            return;

        pickedUp = data.pickedUp;

        if (pickedUp)
        {
            HidePickup();
            return;
        }

        ShowPickup();

        transform.position = data.position;
        transform.rotation = data.rotation;

        if (string.IsNullOrEmpty(data.itemID))
            return;

        Item loadedItem = ItemDatabaseHolder.Instance.GetItem(data.itemID);
        Debug.Log($"[ItemPickup] Loaded item = {loadedItem}");
        if (loadedItem == null)
        {
            Debug.LogWarning($"[ItemPickup] Missing item: {data.itemID}");
            return;
        }

        if (loadedItem.isUnique && data.instanceData != null)
        {
            ItemInstance inst = new ItemInstance(loadedItem, data.instanceData.count)
            {
                currentDurability = data.instanceData.durability,
                currentDamage = data.instanceData.damage,
                currentBalanceDamage = data.instanceData.balanceDamage,
                currentDefenseMultiplier = data.instanceData.defenseMultiplier,
                currentArmor = data.instanceData.armor
            };

            Init(inst);
        }
        else
        {
            Init(loadedItem, data.amount);
        }
    }

    private void HidePickup()
    {
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }

    private void ShowPickup()
    {
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = true;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
    }

    public void ForceHide()
    {
        pickedUp = true;
        HidePickup();
    }

}