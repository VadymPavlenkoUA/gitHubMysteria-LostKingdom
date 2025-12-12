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
    QuestItem = 1 << 14,
    Shield = 1 << 15
}

public enum WeaponHandType
{
    None,
    OneHand,
    TwoHand
}

[CreateAssetMenu(menuName = "RPG/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 1;
    public float weight = 1f;

    public GameObject itemPrefab;
    public GameObject itemPrefabEquip;

    public int meshIndex = -1;

    [Header("Use Settings")]
    public float useDuration = 1.5f;

    [Header("Combat Stats")]
    public float baseDamage = 0f;       // ÿךשמ צו חבנמÿ
    public float baseArmor = 0f;        // ÿךשמ צו בנמםÿ

    [Header("Durability")]
    public float maxDurability = 100f;
    public float currentDurability = 100f;

    public ItemCategory categories;

    public WeaponHandType weaponHandType = WeaponHandType.None;

    [Header("Inactive Position Settings")]
    public Vector3 inactivePosition;
    public Vector3 inactiveRotation;

    [Header("Alt. Inactive Position Settings (for left hands)")]
    public Vector3 inactiveAltPosition;
    public Vector3 inactiveAltRotation;

    [Header("Right Hand Position Settings")]
    public Vector3 rightHandPosition;
    public Vector3 rightHandRotation;

    [Header("Left Hand Position Settings")]
    public Vector3 leftHandPosition;
    public Vector3 leftHandRotation;

    [Header("Food Settings")]
    public float satietyRestore = 0f;
    public float healthRestore = 0f;

    [TextArea(2, 5)]
    public string description;
}
