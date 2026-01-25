using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/NPC Trader")]
public class TraderData : ScriptableObject
{
    public string traderName;

    [Header("Inventory Settings")]
    public int inventorySlots = 20;

    [Header("Economy")]
    public int startGold = 500;
    public float maxWeight = 1000f; // лог≥чно Ї, але ≥гноруЇмо

    [Header("Pricing")]
    public float buyPriceMultiplier = 1.0f;   // на ск≥льки в≥н купуЇ у гравц€
    public float sellPriceMultiplier = 1.0f;  // на ск≥льки продаЇ гравцев≥

    [Header("Start Items")]
    public List<TraderItemEntry> items;
}

[System.Serializable]
public class TraderItemEntry
{
    public Item item;
    public int minAmount = 1;
    public int maxAmount = 5;
}
