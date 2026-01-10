using UnityEngine;

[CreateAssetMenu(menuName = "Loot/Loot Item")]
public class LootItemData : ScriptableObject
{
    [Header("Chest Random Loot")]
    public Item item;

    [Header("Enemy Drop")]
    public GameObject worldPrefab; // префаб, який падає

    [Range(0f, 1f)]
    public float dropChance = 1f;  // 0.5 = 50%
    public int minAmount = 1;
    public int maxAmount = 1;
}
