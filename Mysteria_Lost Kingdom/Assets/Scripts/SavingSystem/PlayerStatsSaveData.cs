using UnityEngine;



[System.Serializable]
public class PlayerStatsSaveData
{
    public int level;
    public int availableStatPoints;

    // Base stats
    public int vitality;
    public int strength;
    public int endurance;
    public int agility;
    public int intellect;
    public int faith;

    // Pending
    public int pendingVitality;
    public int pendingStrength;
    public int pendingEndurance;
    public int pendingAgility;
    public int pendingIntellect;
    public int pendingFaith;

    // EXP
    public float currentExp;
    public float totalExp;
    public float expToNextLevel;

    // Resources
    public float currentHealth;
    public float currentStamina;
    public float currentMana;
    public float currentSatiety;

    // Gold
    public int gold;

    // Position
    public Vector3 position;
    public Quaternion rotation;

    public InventorySaveData inventory;

    public bool rightHandDrawn;
    public bool leftHandDrawn;
    public bool twoHandEquipped;
}
