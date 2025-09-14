using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("HealthSettings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("StaminaSettings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrain = 10f;
    public float staminaRegen = 5f;
    public float regenDelay = 2f;

    [Header("UI")]
    public Slider healthBar;
    public Slider staminaBar;

    public float regenTimer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        healthBar.maxValue = maxHealth;
        staminaBar.maxValue = maxStamina;

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
                currentStamina += staminaRegen * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
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

        UpdateUI();
    }

    public bool TryUseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            regenTimer = regenDelay;
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
        UpdateUI();
    }

    private void Die()
    {
        Debug.Log("Player Died!");
    }

    private void UpdateUI()
    {
        healthBar.value = currentHealth;
        staminaBar.value = currentStamina;
    }
}
