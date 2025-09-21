using UnityEngine;


[System.Flags]
public enum ItemCategory
{
    None = 0,
    Weapon = 1 << 0,
    Bow = 1 << 1,
    ArmourHead = 1 << 2,
    ArmourChest = 1 << 3,
    ArmourLegs = 1 << 4,
    ArmourBoots = 1 << 5,
    ArmourGloves = 1 << 6,
    ArmourBelt = 1 << 7,
    ArmourRing = 1 << 8,
    ArmourNecklace = 1 << 9,
    Food = 1 << 10,
    Potion = 1 << 11,
    Tool = 1 << 12,
    Material = 1 << 13,
    QuestItem = 1 << 14
}

[CreateAssetMenu(menuName = "RPG/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 1;
    public float weight = 1f;

    public GameObject itemPrefab;

    public ItemCategory categories;

    [TextArea(2, 5)]
    public string description;
}
