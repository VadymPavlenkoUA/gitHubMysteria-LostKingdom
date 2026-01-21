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

    public void Clear()
    {
        item = null;
        itemInstance = null;
        amount = 0;
    }
}
