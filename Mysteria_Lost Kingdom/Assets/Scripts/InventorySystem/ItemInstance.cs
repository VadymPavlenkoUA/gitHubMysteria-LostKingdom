[System.Serializable]
public class ItemInstance
{
    public Item item;
    public float currentDurability;
    public int count;
    public float currentDamage;
    public float currentDefenseMultiplier;
    public float currentArmor;

    public ItemInstance(Item item, int count = 1)
    {
        this.item = item;
        this.count = count;
        currentDurability = item.maxDurability;
        currentDamage = item.baseDamage;
        currentDefenseMultiplier = item.baseDefenseMultiplier;
        currentArmor = item.baseArmor;
    }

    public bool HasDurability => item.isUnique;
}
