using Unity.VisualScripting;
using UnityEngine;

public class InventorySlot
{
    public ItemInstance instance;

    public bool IsEmpty => instance == null;
    public Item item => instance?.item;
    public int count => instance?.count ?? 0;

    public bool IsUnique => item != null && item.isUnique;

    //public bool CanAddItem(Item newItem)
    //{
    //    return !IsEmpty && item == newItem && count < item.maxStack;
    //}

    public bool CanAddItem(Item itemNew)
    {
        if (IsEmpty) return true;

        if (itemNew.isUnique)
            return false;

        return item == itemNew && count < item.maxStack;
    }

    //public int AddItem(Item newItem, int amount = 1)
    //{
    //    if (IsEmpty)
    //    {
    //        int added = Mathf.Min(amount, newItem.maxStack);
    //        instance = new ItemInstance(newItem, added);
    //        return amount - added;
    //    }

    //    if (item == newItem && newItem.maxStack > 1)
    //    {
    //        int spaceLeft = newItem.maxStack - instance.count;
    //        int added = Mathf.Min(spaceLeft, amount);
    //        instance.count += added;
    //        return amount - added;
    //    }

    //    return amount;
    //}

    public int AddItem(Item item, int amount)
    {
        if (item.isUnique)
            return amount;

        if (IsEmpty)
        {
            int added = Mathf.Min(amount, item.maxStack);
            instance = new ItemInstance(item, added);
            return amount - added;
        }

        int space = item.maxStack - instance.count;
        int toAdd = Mathf.Min(space, amount);
        instance.count += toAdd;
        return amount - toAdd;
    }

    public void SetItem(Item newItem, int newCount = 1)
    {
        instance = new ItemInstance(newItem, newCount);
    }

    public void SetItem(ItemInstance inst)
    {
        instance = inst;
    }

    public void Clear()
    {
        instance = null;
    }
}
