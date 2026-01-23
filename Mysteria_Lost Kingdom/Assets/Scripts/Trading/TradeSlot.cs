using UnityEngine;

[System.Serializable]
public class TradeSlot
{
    public Item item;            
    public ItemInstance itemInstance;
    public int amount;

    public bool IsEmpty =>
        (item == null && itemInstance == null) || amount <= 0;

    public bool IsUnique => itemInstance != null;

    public Item BaseItem =>
        itemInstance != null ? itemInstance.item : item;

    public void Set(ItemInstance instance)
    {
        itemInstance = instance;
        item = instance.item;
        amount = 1;
    }

    public int Add(Item item, int amount)
    {
        if (IsEmpty)
        {
            this.item = item;
            this.amount = amount;
            return amount;
        }

        if (this.item != item)
            return 0;

        int space = item.maxStack - this.amount;
        int toAdd = Mathf.Min(space, amount);

        this.amount += toAdd;
        return toAdd;
    }

    public void Clear()
    {
        item = null;
        itemInstance = null;
        amount = 0;
    }
}
