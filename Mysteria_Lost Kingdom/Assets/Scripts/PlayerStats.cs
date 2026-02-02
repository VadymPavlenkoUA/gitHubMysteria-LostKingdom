using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using static NSubstitute.Arg;

public class CombatStats
{
    public float totalDamage;
    public float totalBalanceDamage;
    public float totalArmor;
}

public class PlayerStats : MonoBehaviour, ISaveable
{
    public PlayerController controller;
    public PlayerCombat playerCombat;
    public EquipmentManager equipmentManager;
    public Inventory inventory;

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
    [SerializeField] private float satietyLossPerHour = 10f;
    private float satietyMinuteAccumulator = 0f;

    [Header("WeightSettings")]
    public float baseWeight = 35f;
    public float maxWeight = 35f;

    [Header("HealthSettings")]
    public float baseHealth = 100f;
    public float maxHealth = 100f;
    public float currentHealth;
    [SerializeField] private float hpRegenPerHour = 6f;
    [SerializeField] private float hpRegenTick = 0.1f;
    private float hpRegenMinuteAccumulator = 0f;

    [Header("StaminaSettings")]
    public float baseStamina = 100f;
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegen = 5f;
    public float regenDelay = 2f;
    [SerializeField] private float staminaRegenPerHour = 120f;
    [SerializeField] private float staminaRegenTick = 1f;
    private float staminaRegenMinuteAccumulator = 0f;
    private float staminaRegenDelayTimer = 0f;

    [Header("ManaSettings")]
    public float baseMana = 100f;
    public float maxMana = 100f;
    public float currentMana;
    public float manaRegen = 5f;
    public float manaRegenDelay = 2f;
    [SerializeField] private float manaRegenPerHour = 12f;
    [SerializeField] private float manaRegenTick = 0.2f;
    private float manaRegenMinuteAccumulator = 0f;
    private float manaRegenDelayTimer = 0f;

    [Header("Balance / Poise")]
    public float baseBalance = 100f;
    public float maxBalance;
    public float currentBalance;
    public float balanceRegenRate = 15f;
    public float balanceRegenDelay = 2f;
    public float staggerThreshold = 0f;
    private float balanceRegenTimer;

    [Header("Gold")]
    public int gold = 0;

    [Header("Professions")]
    public List<ProfessionStat> professions;

    [Header("UI")]
    public Slider healthBar;
    public Slider staminaBar;
    public Image manaBar;

    [HideInInspector] public float blockMultiplier = 1f;

    private bool loadedFromSave = false;

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CheckLevelProgress();
        CalculateDerivedStats();

        if (!loadedFromSave)
        {
            currentHealth = maxHealth;
            currentStamina = maxStamina;
            currentMana = maxMana;
            currentSatiety = maxSatiety;
        }

        healthBar.maxValue = maxHealth;
        staminaBar.maxValue = maxStamina;


        StatsChanged?.Invoke();

        UpdateUI();
    }

    public string GetSaveID()
    {
        return "PLAYER_STATS";
    }

    public object CaptureState()
    {
        return new PlayerStatsSaveData
        {
            level = level,
            availableStatPoints = availableStatPoints,

            vitality = vitality,
            strength = strength,
            endurance = endurance,
            agility = agility,
            intellect = intellect,
            faith = faith,

            pendingVitality = pendingVitality,
            pendingStrength = pendingStrength,
            pendingEndurance = pendingEndurance,
            pendingAgility = pendingAgility,
            pendingIntellect = pendingIntellect,
            pendingFaith = pendingFaith,

            currentExp = currentExp,
            totalExp = totalExp,
            expToNextLevel = expToNextLevel,

            currentHealth = currentHealth,
            currentStamina = currentStamina,
            currentMana = currentMana,
            currentSatiety = currentSatiety,

            gold = gold,

            position = transform.position,
            rotation = transform.rotation,

            inventory = (InventorySaveData)inventory.CaptureState(),

            rightHandDrawn = equipmentManager.isRightHandDrawn,
            leftHandDrawn = equipmentManager.isLeftHandDrawn,
            twoHandEquipped = equipmentManager.twoHandEquipped
        };
    }

    public void RestoreState(object state)
    {
        var data = (PlayerStatsSaveData)state;

        loadedFromSave = true;

        inventory.RestoreState(data.inventory);

        equipmentManager.RestoreFromInventory(
            inventory.equipSlots,
            data.rightHandDrawn,
            data.leftHandDrawn,
            data.twoHandEquipped
        );

        level = data.level;
        availableStatPoints = data.availableStatPoints;

        vitality = data.vitality;
        strength = data.strength;
        endurance = data.endurance;
        agility = data.agility;
        intellect = data.intellect;
        faith = data.faith;

        pendingVitality = data.pendingVitality;
        pendingStrength = data.pendingStrength;
        pendingEndurance = data.pendingEndurance;
        pendingAgility = data.pendingAgility;
        pendingIntellect = data.pendingIntellect;
        pendingFaith = data.pendingFaith;

        currentExp = data.currentExp;
        totalExp = data.totalExp;
        expToNextLevel = data.expToNextLevel;

        currentHealth = data.currentHealth;
        currentStamina = data.currentStamina;
        currentMana = data.currentMana;
        currentSatiety = data.currentSatiety;

        gold = data.gold;

        transform.position = data.position;
        transform.rotation = data.rotation;

        CalculateDerivedStats();
        CheckLevelProgress();

        StatsChanged?.Invoke();
        HealthChanged?.Invoke();
        StaminaChanged?.Invoke();
        ManaChanged?.Invoke();
        SatietyChanged?.Invoke();

        UpdateUI();
    }


    // Update is called once per frame
    void Update()
    {
        float gameMinutes = TimeOfDayManager.Instance.GameMinutesDelta;

        // ---------------- STAMINA ----------------
        if (currentStamina < maxStamina)
        {
            if (staminaRegenDelayTimer > 0f)
            {
                staminaRegenDelayTimer -= gameMinutes;
            }
            else
            {
                float minutesPerTickStamina =
                    (60f * staminaRegenTick) / staminaRegenPerHour;

                staminaRegenMinuteAccumulator += gameMinutes;

                while (staminaRegenMinuteAccumulator >= minutesPerTickStamina)
                {
                    staminaRegenMinuteAccumulator -= minutesPerTickStamina;

                    if (currentStamina >= maxStamina)
                        break;

                    currentStamina += staminaRegenTick;
                    currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

                    StaminaChanged?.Invoke();
                }
            }
        }
        else
        {
            staminaRegenMinuteAccumulator = 0f;
        }

        // ---------------- MANA ----------------
        if (currentMana < maxMana)
        {
            if (manaRegenDelayTimer > 0f)
            {
                manaRegenDelayTimer -= gameMinutes;
            }
            else
            {
                float minutesPerTickMana = (60f * manaRegenTick) / manaRegenPerHour;
                manaRegenMinuteAccumulator += gameMinutes;

                while (manaRegenMinuteAccumulator >= minutesPerTickMana)
                {
                    manaRegenMinuteAccumulator -= minutesPerTickMana;

                    if (currentMana >= maxMana)
                        break;

                    float oldMana = currentMana;
                    currentMana += manaRegenTick;
                    currentMana = Mathf.Clamp(currentMana, 0f, maxMana);

                    if (Mathf.Abs(currentMana - oldMana) > 0.001f)
                        ManaChanged?.Invoke();
                }
            }
        }
        else
        {
            manaRegenMinuteAccumulator = 0f;
        }

        // ---------------- SATIETY ----------------
        satietyMinuteAccumulator += gameMinutes;
        float minutesPerSatiety = 60f / satietyLossPerHour;

        while (satietyMinuteAccumulator >= minutesPerSatiety)
        {
            satietyMinuteAccumulator -= minutesPerSatiety;

            if (currentSatiety <= 0f)
                break;

            currentSatiety -= 1f;
            currentSatiety = Mathf.Clamp(currentSatiety, 0f, maxSatiety);
            SatietyChanged?.Invoke();
        }

        // ---------------- HEALTH ----------------
        if (currentHealth < maxHealth && currentSatiety >= 30f)
        {
            float minutesPerTickHP = (60f * hpRegenTick) / hpRegenPerHour;
            hpRegenMinuteAccumulator += gameMinutes;

            while (hpRegenMinuteAccumulator >= minutesPerTickHP)
            {
                hpRegenMinuteAccumulator -= minutesPerTickHP;

                if (currentHealth >= maxHealth)
                    break;

                currentHealth += hpRegenTick;
                currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
                HealthChanged?.Invoke();
            }
        }
        else
        {
            hpRegenMinuteAccumulator = 0f;
        }

        // ---------------- Balance ----------------
        UpdateBalance(Time.deltaTime);

        // ---------------- UI ----------------
        UpdateUI();
    }


    public void TakeDamage(float incomingDamage, float incomingBalanceDamage)
    {
        if (controller != null && controller.IsInvulnerable)
        {
            Debug.Log("Damage ignored (INVUL)");
            return;
        }

        CombatStats combat = CalculateCombatStats();

        float armor = combat.totalArmor;

        // Блок (щит, парирування і т.п.)
        float blockedDamage = incomingDamage / blockMultiplier;

        if (playerCombat.isBlocking) playerCombat.OnBlockedHit();

        // Формула з урахуванням броні
        float finalDamage = blockedDamage * (blockedDamage / (blockedDamage + armor));

        finalDamage = Mathf.Max(finalDamage, 1f); // мінімальний урон

        currentHealth -= finalDamage;

        // Баланс
        float balanceArmorReduction = armor * 0.3f;
        float finalBalanceDamage =
            Mathf.Max(1f, incomingBalanceDamage - balanceArmorReduction);

        currentBalance -= finalBalanceDamage;
        balanceRegenTimer = balanceRegenDelay;

        if (currentBalance <= staggerThreshold)
        {
            TriggerStagger();
            currentBalance = maxBalance * 0.6f;
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        HealthChanged?.Invoke();
        UpdateUI();
    }

    private void TriggerStagger()
    {
        //playerCombat.InterruptAttack(true);
        //Debug.Log($"Trigger Stagger");
        if (playerCombat.IsInStag) return;

        playerCombat.EnterStagger();
    }

    private void UpdateBalance(float deltaTime)
    {
        if (currentBalance >= maxBalance) return;

        if (balanceRegenTimer > 0f)
        {
            balanceRegenTimer -= deltaTime;
        }
        else
        {
            currentBalance += balanceRegenRate * deltaTime;
            currentBalance = Mathf.Min(currentBalance, maxBalance);
        }
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

        if (currentStamina < adjustedAmount)
            return false;

        currentStamina -= adjustedAmount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        staminaRegenDelayTimer = regenDelay;         
        staminaRegenMinuteAccumulator = 0f;           

        StaminaChanged?.Invoke();
        UpdateUI();
        return true;
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        staminaRegenDelayTimer = regenDelay;       
        staminaRegenMinuteAccumulator = 0f;

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
        NotificationSystem.Instance.ShowNotification(NotificationSystem.Instance.expSprite, $"Досвід +{amount:F2} EXP");
    }

    public void AddGold(int amount)
    {
        gold += amount;
        StatsChanged?.Invoke();
    }

    public bool HasGold(int amount)
    {
        return gold >= amount;
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0)
            return true;

        if (gold < amount)
            return false;

        gold -= amount;
        StatsChanged?.Invoke();
        return true;
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

    private void CalculateBalance()
    {
        CombatStats combat = CalculateCombatStats();

        maxBalance =
            baseBalance +
            endurance * 2f +      
            vitality * 3f +
            combat.totalArmor * 0.5f;

        maxBalance = Mathf.Clamp(maxBalance, 50f, 500f);
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

        if (eq.equippedRightItem != null && eq.equippedRightItem.item.categories != ItemCategory.Shield && eq.isRightHandDrawn)
        {
            //durabilityMod = eq.equippedRightItem.currentDurability /
            //                eq.equippedRightItem.maxDurability;

            combat.totalDamage +=
                eq.equippedRightItem.item.baseDamage * durabilityMod +
                strength * 1.5f +
                agility * 0.5f;

            combat.totalBalanceDamage = eq.equippedRightItem.item.baseBalanceDamage;
        }

        if (eq.equippedLeftItem != null && eq.equippedLeftItem.item.categories != ItemCategory.Shield && eq.equippedLeftItem.item.weaponHandType != WeaponHandType.TwoHand && eq.isLeftHandDrawn)
        {
            //durabilityMod = eq.equippedRightItem.currentDurability /
            //                eq.equippedRightItem.maxDurability;

            combat.totalDamage +=
                eq.equippedLeftItem.item.baseDamage * durabilityMod +
                strength * 1.5f +
                agility * 0.5f;

            combat.totalBalanceDamage = eq.equippedLeftItem.item.baseBalanceDamage;
        }

        if (eq.equippedLeftItem != null && eq.equippedLeftItem.item.categories == ItemCategory.Shield && eq.isLeftHandDrawn && !eq.isRightHandDrawn)
        {
            //durabilityMod = eq.equippedRightItem.currentDurability /
            //                eq.equippedRightItem.maxDurability;

            combat.totalDamage +=
                eq.equippedLeftItem.item.baseDamage * durabilityMod +
                strength * 1.5f +
                agility * 0.5f;

            combat.totalBalanceDamage = eq.equippedLeftItem.item.baseBalanceDamage;
        }

        if ((eq.equippedRightItem == null && eq.equippedLeftItem == null) || (!eq.isLeftHandDrawn && !eq.isRightHandDrawn))
        {
            // Урон кулаками
            combat.totalDamage = strength * 0.5f;
            combat.totalBalanceDamage = 10f;
        }



        // ---------------------------
        // 2. ЗАХИСТ
        // ---------------------------
        float gearArmorSum = 0f;

        ItemInstance[] armourItems =
        {
        eq.equippedHeadArmourItem,
        eq.equippedChestArmourItem,
        eq.equippedLegArmourItem,
        eq.equippedBootsItem,
        eq.equippedGlovesItem,
        eq.equippedBeltItem,
    };

        foreach (var inst in armourItems)
        {
            if (inst == null) continue;

            //float itemDurability = item.currentDurability / item.maxDurability;
            gearArmorSum += inst.currentArmor;
        }

        if (eq.equippedLeftItem != null && eq.equippedLeftItem.item.categories == ItemCategory.Shield)
        {
            float shieldArmor = eq.equippedLeftItem.item.baseArmor;

            if (!EquipmentManager.Instance.isLeftHandDrawn) shieldArmor *= 0.5f;

            gearArmorSum += shieldArmor;
        }

        combat.totalArmor =
            gearArmorSum +
            vitality * 0.5f +
            endurance * 0.25f;

        maxBalance =
            baseBalance +
            endurance * 1.2f +
            vitality * 1.5f +
            combat.totalArmor * 0.5f;

        maxBalance = Mathf.Clamp(maxBalance, 50f, 500f);

        return combat;
    }

    public void InvokeCombatChanged()
    {
        CombatChanged?.Invoke();
    }

}
