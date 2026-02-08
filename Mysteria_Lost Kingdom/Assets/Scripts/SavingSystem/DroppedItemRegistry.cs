using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroppedItemRegistry : MonoBehaviour
{
    public static DroppedItemRegistry Instance;

    private List<DroppedItemSaveData> droppedItems = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(ItemPickup pickup)
    {
        var data = (ItemPickupSaveData)pickup.CaptureState();
        if (data.pickedUp) return;

        droppedItems.Add(new DroppedItemSaveData
        {
            uniqueID = pickup.GetSaveID(),
            itemID = data.itemID,
            amount = data.amount,
            instanceData = data.instanceData,
            position = pickup.transform.position,
            rotation = pickup.transform.rotation
        });
    }

    public void Unregister(string uniqueID)
    {
        droppedItems.RemoveAll(d => d.uniqueID == uniqueID);
    }

    public void Restore(List<DroppedItemSaveData> data)
    {
        droppedItems = data.ToList();
    }

    public void RestoreAll()
    {

        foreach (var pickup in FindObjectsByType<ItemPickup>(FindObjectsSortMode.None))
        {
            if (!pickup.isDropped) continue;

            pickup.ForceHide();
            Destroy(pickup.gameObject);
        }

        foreach (var data in droppedItems)
        {
            Item item = ItemDatabaseHolder.Instance.GetItem(data.itemID);
            if (item == null)
            {
                Debug.LogWarning($"Item {data.itemID} not found in database!");
                continue;
            }

            GameObject go = Instantiate(item.itemPrefab, data.position, data.rotation);

            var pickup = go.GetComponent<ItemPickup>();
            var saveable = go.GetComponent<SaveableEntity>();

            //saveable.GenerateID();
            saveable.SetID(data.uniqueID);

            if (item.isUnique && data.instanceData != null)
            {
                var inst = new ItemInstance(item, data.instanceData.count)
                {
                    currentDurability = data.instanceData.durability,
                    currentDamage = data.instanceData.damage,
                    currentBalanceDamage = data.instanceData.balanceDamage,
                    currentDefenseMultiplier = data.instanceData.defenseMultiplier,
                    currentArmor = data.instanceData.armor
                };

                pickup.Init(inst);
            }
            else
            {
                pickup.Init(item, data.amount);
            }

            pickup.isDropped = true;
        }
    }

    public List<DroppedItemSaveData> Capture() => droppedItems;
}
