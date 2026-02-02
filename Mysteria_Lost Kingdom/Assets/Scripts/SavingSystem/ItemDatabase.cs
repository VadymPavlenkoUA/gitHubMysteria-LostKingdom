using System.Collections.Generic;
using System.Linq;
using UnityEngine;



[CreateAssetMenu(menuName = "Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items;

    private Dictionary<string, Item> lookup;

    public void Init()
    {
        lookup = new Dictionary<string, Item>();
        foreach (var item in items)
        {
            if (!lookup.ContainsKey(item.itemID)) lookup.Add(item.itemID, item);
        }
    }

    public Item Get(string id)
    {
        lookup ??= new Dictionary<string, Item>();
        return lookup.TryGetValue(id, out var item) ? item : null;
    }
}


[System.Serializable]
public class ItemInstanceSaveData
{
    public string itemID;
    public int count;
    public InventorySlotUI.SlotSpecification slotSpec;
    public float durability;
    public float damage;
    public float balanceDamage;
    public float defenseMultiplier;
    public float armor;
}

[System.Serializable]
public class InventorySaveData
{
    public List<ItemInstanceSaveData> slots = new();
    public List<ItemInstanceSaveData> equipSlots = new();
}