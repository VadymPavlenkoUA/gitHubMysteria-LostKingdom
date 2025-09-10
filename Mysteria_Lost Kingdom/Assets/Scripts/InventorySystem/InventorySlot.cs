using UnityEngine;

public class InventorySlot
{
    public Item item;
    public int count;

    public bool IsEmpty => item == null;

    public bool CanAddItem(Item newItem)
    {
        return !IsEmpty && item == newItem && count < item.maxStack;
    }

    public int AddItem(Item newItem, int amount = 1)
    {
        if (IsEmpty)
        {
            item = newItem;
            int added = Mathf.Min(amount, item.maxStack);
            count = added;
            return amount - added;
        }
        else if (item == newItem)
        {
            int spaceLeft = item.maxStack - count;
            int added = Mathf.Min(spaceLeft, amount);
            count += added;
            return amount - added;
        }

        return amount;
    }
    public void Clear()
    {
        item = null;
        count = 0;
    }
}
