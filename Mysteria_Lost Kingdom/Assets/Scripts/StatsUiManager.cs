using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUiManager : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("UI Elements")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelCircleText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI vitalityText;
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI enduranceText;
    public TextMeshProUGUI agilityText;
    public TextMeshProUGUI intellectText;
    public TextMeshProUGUI faithText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI weightText;

    [Header("UI Elements Stats")]
    public TextMeshProUGUI levelStatsText;
    public TextMeshProUGUI availableStatsPointsText;
    public TextMeshProUGUI vitalityStatsText;
    public TextMeshProUGUI strengthStatsText;
    public TextMeshProUGUI enduranceStatsText;
    public TextMeshProUGUI agilityStatsText;
    public TextMeshProUGUI intellectStatsText;
    public TextMeshProUGUI faithStatsText;

    [Header("Buttons Stats")]
    public Button plusVitalityBtn;
    public Button minusVitalityBtn;
    public Button plusStrengthBtn;
    public Button minusStrengthBtn;
    public Button plusEnduranceBtn;
    public Button minusEnduranceBtn;
    public Button plusAgilityBtn;
    public Button minusAgilityBtn;
    public Button plusIntellectBtn;
    public Button minusIntellectBtn;
    public Button plusFaithBtn;
    public Button minusFaithBtn;
    public Button confirmBTN;
    public Button cancelBTN;

    public TextMeshProUGUI healthEquipText;
    public TextMeshProUGUI staminaEquipText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStats.StatsChanged += UpdateStatsDisplay;
        playerStats.HealthChanged += UpdatesHealthOnly;
        playerStats.StaminaChanged += UpdateStaminaOnly;
        playerStats.LevelUpEvent += OnLevelUp;
        UpdateStatsDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateStatsDisplay()
    {
        if (playerStats == null) return;
        levelText.text = $"Рівень: {playerStats.level}";
        levelCircleText.text = $"{playerStats.level}";
        expText.text = $"Досвід: {playerStats.currentExp.ToString("0.0", CultureInfo.InvariantCulture)} / " +
            $"{playerStats.expToNextLevel.ToString("0.0", CultureInfo.InvariantCulture)}";
        vitalityText.text = $"Здоров’я: {playerStats.vitality}";
        strengthText.text = $"Сила: {playerStats.strength}";
        enduranceText.text = $"Стійкість: {playerStats.endurance}";
        agilityText.text = $"Гнучкість: {playerStats.agility}";
        intellectText.text = $"Вченість: {playerStats.intellect}";
        faithText.text = $"Віра: {playerStats.faith}";
        healthText.text = $"Здоров'я: {playerStats.maxHealth}";
        staminaText.text = $"Витривалість: {playerStats.maxStamina}";
        weightText.text = $"Вантажність: {playerStats.maxWeight.ToString("0.0", CultureInfo.InvariantCulture)}";
        healthEquipText.text = $"{playerStats.currentHealth.ToString("0.0", CultureInfo.InvariantCulture)} / " +
            $"{playerStats.maxHealth.ToString("0.0", CultureInfo.InvariantCulture)}";
        staminaEquipText.text = $"{playerStats.currentStamina.ToString("0.0", CultureInfo.InvariantCulture)} / " +
            $"{playerStats.maxStamina.ToString("0.0", CultureInfo.InvariantCulture)}";

        levelStatsText.text = $"Рівень: {playerStats.level}";
        availableStatsPointsText.text = $"Доступно очків: {playerStats.availableStatPoints}";
        vitalityStatsText.text = playerStats.pendingVitality > 0
            ? $"Здоров’я: {playerStats.vitality}\n(+{playerStats.pendingVitality})"
            : $"Здоров’я: {playerStats.vitality}";
        enduranceStatsText.text = playerStats.pendingEndurance > 0
            ? $"Стійкість: {playerStats.endurance}\n(+{playerStats.pendingEndurance})"
            : $"Стійкість {playerStats.endurance}";
        strengthStatsText.text = playerStats.pendingStrength > 0
            ? $"Сила: {playerStats.strength}\n(+{playerStats.pendingStrength})"
            : $"Сила: {playerStats.strength}";
        agilityStatsText.text = playerStats.pendingAgility > 0
            ? $"Гнучкість: {playerStats.agility}\n(+{playerStats.pendingAgility})"
            : $"Гнучкість: {playerStats.agility}";
        intellectStatsText.text = playerStats.pendingIntellect > 0
            ? $"Вченість: {playerStats.intellect}\n(+{playerStats.pendingIntellect})"
            : $"Вченість: {playerStats.intellect}";
        faithStatsText.text = playerStats.pendingFaith > 0
            ? $"Віра: {playerStats.faith}\n(+{playerStats.pendingFaith})"
            : $"Віра: {playerStats.faith}";

        plusVitalityBtn.gameObject.SetActive(playerStats.availableStatPoints > 0);
        minusVitalityBtn.gameObject.SetActive(playerStats.pendingVitality > 0);
        plusStrengthBtn.gameObject.SetActive(playerStats.availableStatPoints > 0);
        minusStrengthBtn.gameObject.SetActive(playerStats.pendingStrength > 0);
        plusEnduranceBtn.gameObject.SetActive(playerStats.availableStatPoints > 0);
        minusEnduranceBtn.gameObject.SetActive(playerStats.pendingEndurance > 0);
        plusAgilityBtn.gameObject.SetActive(playerStats.availableStatPoints > 0);
        minusAgilityBtn.gameObject.SetActive(playerStats.pendingAgility > 0);
        plusIntellectBtn.gameObject.SetActive(playerStats.availableStatPoints > 0);
        minusIntellectBtn.gameObject.SetActive(playerStats.pendingIntellect > 0);
        plusFaithBtn.gameObject.SetActive(playerStats.availableStatPoints > 0);
        minusFaithBtn.gameObject.SetActive(playerStats.pendingFaith > 0);

        bool hasPending = playerStats.pendingVitality + playerStats.pendingStrength + playerStats.pendingEndurance +
            playerStats.pendingAgility + playerStats.pendingIntellect + playerStats.pendingFaith > 0;
        confirmBTN.gameObject.SetActive(hasPending);
        cancelBTN.gameObject.SetActive(hasPending);
    }

    private void UpdatesHealthOnly()
    {
        healthEquipText.text = $"{playerStats.currentHealth.ToString("0.0", CultureInfo.InvariantCulture)} / " +
            $"{playerStats.maxHealth.ToString("0.0", CultureInfo.InvariantCulture)}";
    }

    private void UpdateStaminaOnly()
    {
        staminaEquipText.text = $"{playerStats.currentStamina.ToString("0.0", CultureInfo.InvariantCulture)} / " +
            $"{playerStats.maxStamina.ToString("0.0", CultureInfo.InvariantCulture)}";
    }

    private void OnLevelUp()
    {
        UpdateStatsDisplay();
    }
}
