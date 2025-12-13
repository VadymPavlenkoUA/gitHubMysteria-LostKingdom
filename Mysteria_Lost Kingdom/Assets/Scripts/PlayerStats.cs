using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class CombatStats
{
    public float totalDamage;
    public float totalArmor;
}

public class PlayerStats : MonoBehaviour
{
    [Header("LevelUpSettings")]
    public int level = 1;
    public int statPointsPerLevel = 3;
    public int availableStatPoints = 0;

    [Header("PendingStats")]
    public int pendingVitality;
    public int pendingStrength;
    public int pendingEndurance;
    public int pendingAgility;
    public int pendingIntellect;
    public int pendingFaith;

    [Header("RPG Stats")]
    public int vitality = 5;
    public int strength = 5;  
    public int endurance = 5; 
    public int agility = 5;   
    public int intellect = 5; 
    public int faith = 5;

    [Header("ExperienceSettings")]
    public float currentExp = 0;
    public float totalExp = 0;
    public float expToNextLevel = 120;
    public float expGrowthRate = 1.2f;

    [Header("SatietySettings")]
    public float baseSatiety;
    public float maxSatiety = 100f;
    public float currentSatiety = 100f;

    [Header("WeightSettings")]
    public float baseWeight = 35f;
    public float maxWeight = 35f;

    [Header("HealthSettings")]
    public float baseHealth = 100f;
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("StaminaSettings")]
    public float baseStamina = 100f;
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegen = 5f;
    public float regenDelay = 2f;

    [Header("ManaSettings")]
    public float baseMana = 100f;
    public float maxMana = 100f;
    public float currentMana;
    public float manaRegen = 5f;
    public float manaRegenDelay = 2f;

    [Header("Gold")]
    public int gold = 0;

    [Header("Professions")]
    public List<ProfessionStat> professions;

    [Header("UI")]
    public Slider healthBar;
    public Slider staminaBar;
    public Image manaBar;

    public delegate void OnLevelUp();
    public event OnLevelUp LevelUpEvent;

    public delegate void OnStatsChanged();
    public event OnStatsChanged StatsChanged;

    public delegate void OnHealthChanged();
    public event OnHealthChanged HealthChanged;

    public delegate void OnSatietyChanged();
    public event OnSatietyChanged SatietyChanged;

    public delegate void OnStaminaChanged();
    public event OnStaminaChanged StaminaChanged;

    public delegate void OnManaChanged();
    public event OnManaChanged ManaChanged;

    public delegate void OnCombarChanged();
    public event OnCombarChanged CombatChanged;

    public float regenTimer;
    private float manaRegenTimer;
    private float satietyTimer = 0f;
    private float healthRegenTimer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CheckLevelProgress();
        CalculateDerivedStats();

        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentMana = maxMana;
        currentSatiety = maxSatiety;

        healthBar.maxValue = maxHealth;
        staminaBar.maxValue = maxStamina;


        StatsChanged?.Invoke();

        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentStamina < maxStamina)
        {
            regenTimer -= Time.deltaTime;
            if (regenTimer <= 0)
            {
                float regenAmount = GetStaminaRegen();
                float oldStamina = currentStamina;
                currentStamina += regenAmount * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

                if (Mathf.Abs(currentStamina - oldStamina) > 0.01f) StaminaChanged?.Invoke();
            }
        }

        if (currentMana < maxMana)
        {
            manaRegenDelay -= Time.deltaTime;
            if (manaRegenTimer <= 0)
            {
                float regenAmount = GetManaRegen();
                float oldMana = currentMana;
                currentMana += regenAmount * Time.deltaTime;
                currentMana = Mathf.Clamp(currentMana, 0, maxMana);
                if (Mathf.Abs(currentMana - oldMana) > 0.01f) ManaChanged?.Invoke();
            }
        }

        satietyTimer += Time.deltaTime;
        if (satietyTimer >= 60f)
        {
            satietyTimer = 0f;
            if (currentSatiety > 0)
            {
                currentSatiety -= 1f;
                currentSatiety = Mathf.Clamp(currentSatiety, 0f, maxSatiety);
                SatietyChanged?.Invoke();
            }
        }

        healthRegenTimer += Time.deltaTime;
        if (healthRegenTimer >= 10f)
        {
            healthRegenTimer = 0f;
            if (currentHealth < maxHealth && currentSatiety >= 30)
            {
                currentHealth += 0.1f;
                currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
                HealthChanged?.Invoke();
            }
        }

        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        HealthChanged?.Invoke();
        UpdateUI();
    }

    public void Heal (float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        HealthChanged?.Invoke();
        UpdateUI();
    }

    public void IncreaseSatiety(float amount)
    {
        currentSatiety += amount;
        currentSatiety = Mathf.Clamp(currentSatiety, 0, maxSatiety);
        SatietyChanged?.Invoke();
        UpdateUI();
    }

    public bool TryUseStamina(float amount)
    {
        float adjustedAmount = amount * GetStaminaCostMultiplier();
        if (currentStamina >= adjustedAmount)
        {
            currentStamina -= adjustedAmount;
            regenTimer = regenDelay;
            StaminaChanged?.Invoke();
            UpdateUI();
            return true;
        }
        return false;
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        regenTimer = regenDelay;
        StaminaChanged?.Invoke();
        UpdateUI();
    }

    public void AddExperience(float amount)
    {
        currentExp += amount;
        totalExp += amount;

        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }

        StatsChanged?.Invoke();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        StatsChanged?.Invoke();
    }

    public void CheckLevelProgress()
    {
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        availableStatPoints += statPointsPerLevel;
        expToNextLevel = Mathf.Round(expToNextLevel * expGrowthRate);

        CalculateDerivedStats();

        Debug.Log($"Level Up! {level}");
        LevelUpEvent?.Invoke();
        StatsChanged?.Invoke();
    }

    public void AddPendingStat (string statName)
    {
        if (availableStatPoints <= 0) return;

        switch (statName)
        {
            case "vitality": pendingVitality++; break;
            case "strength": pendingStrength++; break;
            case "endurance": pendingEndurance++; break;
            case "agility": pendingAgility++; break;
            case "intellect": pendingIntellect++; break;
            case "faith": pendingFaith++; break;
        }

        availableStatPoints--;
        StatsChanged?.Invoke();
    }

    public void RemovePendingStat(string statName)
    {
        switch (statName)
        {
            case "vitality":
                if (pendingVitality > 0) { pendingVitality--; availableStatPoints++; }
                break;
            case "strength":
                if (pendingStrength > 0) { pendingStrength--; availableStatPoints++; }
                break;
            case "endurance":
                if (pendingEndurance > 0) { pendingEndurance--; availableStatPoints++; }
                break;
            case "agility":
                if (pendingAgility > 0) { pendingAgility--; availableStatPoints++; }
                break;
            case "intellect":
                if (pendingIntellect > 0) { pendingIntellect--; availableStatPoints++; }
                break;
            case "faith":
                if (pendingFaith > 0) { pendingFaith--; availableStatPoints++; }
                break;
        }

        StatsChanged?.Invoke();
    }

    public void ConfirmPendingStats()
    {
        vitality += pendingVitality;
        strength += pendingStrength;
        endurance += pendingEndurance;
        agility += pendingAgility;
        intellect += pendingIntellect;
        faith += pendingFaith;

        pendingVitality = 0;
        pendingStrength = 0;
        pendingEndurance = 0;
        pendingAgility = 0;
        pendingIntellect = 0;
        pendingFaith = 0;

        CalculateDerivedStats();
        StatsChanged?.Invoke();
    }

    public void CancelPendingStats()
    {
        availableStatPoints += pendingVitality + pendingStrength + pendingEndurance + pendingAgility + pendingIntellect + pendingFaith;

        pendingVitality = 0;
        pendingStrength = 0;
        pendingEndurance = 0;
        pendingAgility = 0;
        pendingIntellect = 0;
        pendingFaith = 0;

        StatsChanged?.Invoke();
    }

    private void Die()
    {
        Debug.Log("Player Died!");
    }

    public void CalculateDerivedStats()
    {
        maxHealth = baseHealth + vitality * 2f;     // сила = +2 HP за 1
        maxStamina = baseStamina + endurance * 5f;  // стійкість = +5 stamina за 1
        maxWeight = baseWeight + strength * 1f;     // сила = +1 kg за 1
        maxMana = baseMana + intellect * 3f;        // інтелект = +3 mana за 1
        StatsChanged?.Invoke();
    }

    public float GetStaminaRegen()
    {
        return staminaRegen + endurance * 0.5f;
    }

    public float GetManaRegen()
    {
        return manaRegen + intellect + faith * 0.5f;
    }
    
    private float GetStaminaCostMultiplier()
    {
        return Mathf.Clamp(1f - agility * 0.03f, 0.6f, 1f);
    }

    private void UpdateUI()
    {
        healthBar.value = currentHealth;
        staminaBar.value = currentStamina;
        manaBar.fillAmount = currentMana / maxMana;
    }

    public ProfessionStat GetProfession(CraftingProfession prof)
    {
        return professions.Find(p => p.profession == prof);
    }

    public void AddProfessionExp(CraftingProfession prof, float amount)
    {
        var p = GetProfession(prof);
        p.exp += amount;
        while (p.exp >= p.expToNext)
        {
            p.exp -= p.expToNext;
            p.level++;
            p.expToNext = Mathf.Round(p.expToNext * p.growth);
        }
        StatsChanged?.Invoke();
    }

    public CombatStats CalculateCombatStats()
    {
        CombatStats combat = new CombatStats();
        var eq = EquipmentManager.Instance;

        // 1. УРОН
        float durabilityMod = 1f;

        if (eq.equippedRightItem != null && eq.equippedRightItem.categories != ItemCategory.Shield)
        {
            //durabilityMod = eq.equippedRightItem.currentDurability /
            //                eq.equippedRightItem.maxDurability;

            combat.totalDamage +=
                eq.equippedRightItem.baseDamage * durabilityMod +
                strength * 1.5f +
                agility * 0.5f;
        }

        if (eq.equippedLeftItem != null && eq.equippedLeftItem.categories != ItemCategory.Shield && eq.equippedLeftItem.weaponHandType != WeaponHandType.TwoHand)
        {
            //durabilityMod = eq.equippedRightItem.currentDurability /
            //                eq.equippedRightItem.maxDurability;

            combat.totalDamage +=
                eq.equippedLeftItem.baseDamage * durabilityMod +
                strength * 1.5f +
                agility * 0.5f;
        }

        if (eq.equippedRightItem == null && eq.equippedLeftItem == null)
        {
            // Урон кулаками
            combat.totalDamage = strength * 0.5f;
        }



        // ---------------------------
        // 2. ЗАХИСТ
        // ---------------------------
        float gearArmorSum = 0f;

        Item[] armourItems =
        {
        eq.equippedHeadArmourItem,
        eq.equippedChestArmourItem,
        eq.equippedLegArmourItem,
        eq.equippedBootsItem,
        eq.equippedGlovesItem,
        eq.equippedBeltItem
    };

        foreach (var item in armourItems)
        {
            if (item == null) continue;

            //float itemDurability = item.currentDurability / item.maxDurability;
            gearArmorSum += item.baseArmor;
        }

        combat.totalArmor =
            gearArmorSum +
            vitality * 0.5f +
            endurance * 0.25f;

        return combat;
    }

    public void InvokeCombatChanged()
    {
        CombatChanged?.Invoke();
    }

}
