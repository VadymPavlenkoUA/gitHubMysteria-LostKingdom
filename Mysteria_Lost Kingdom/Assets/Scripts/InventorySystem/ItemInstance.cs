[System.Serializable]
public class ItemInstance
{
    public Item item;
    public float currentDurability;
    public int count;
    public float currentDamage;

    public ItemInstance(Item item, int count = 1)
    {
        this.item = item;
        this.count = count;
        currentDurability = item.maxDurability;
        currentDamage = item.baseDamage;
    }

    public bool HasDurability =>
        (item.categories & ItemCategory.Weapon) != 0 ||
        (item.categories & ItemCategory.ArmourHead) != 0 ||
        (item.categories & ItemCategory.ArmourChest) != 0 ||
        (item.categories & ItemCategory.ArmourLegs) != 0 ||
        (item.categories & ItemCategory.ArmourBoots) != 0 ||
        (item.categories & ItemCategory.ArmourGloves) != 0 ||
        (item.categories & ItemCategory.ArmourBelt) != 0 ||
        (item.categories & ItemCategory.Shield) != 0;
}
