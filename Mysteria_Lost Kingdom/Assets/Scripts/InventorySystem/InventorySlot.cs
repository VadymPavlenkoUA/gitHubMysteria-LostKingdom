using UnityEngine;

public class InventorySlot
{
    public ItemInstance instance;

    public bool IsEmpty => instance == null;
    public Item item => instance?.item;
    public int count => instance?.count ?? 0;

    public bool CanAddItem(Item newItem)
    {
        return !IsEmpty && item == newItem && count < item.maxStack;
    }

    public int AddItem(Item newItem, int amount = 1)
    {
        if (IsEmpty)
        {
            int added = Mathf.Min(amount, newItem.maxStack);
            instance = new ItemInstance(newItem, added);
            return amount - added;
        }

        if (item == newItem && newItem.maxStack > 1)
        {
            int spaceLeft = newItem.maxStack - instance.count;
            int added = Mathf.Min(spaceLeft, amount);
            instance.count += added;
            return amount - added;
        }

        return amount;
    }
    public void SetItem(Item newItem, int newCount = 1)
    {
        instance = new ItemInstance(newItem, newCount);
    }

    public void Clear()
    {
        instance = null;
    }
}
