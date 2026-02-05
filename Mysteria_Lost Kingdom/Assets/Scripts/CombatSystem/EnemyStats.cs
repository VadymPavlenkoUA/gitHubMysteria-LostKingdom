using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public EnemyCombat combat;

    [Header("Identity")]
    public string enemyId; // goblin_warrior, boss_dragon_01
    public bool isUnique;

    [Header("Health")]
    public float maxHealth = 100f;
    public float armor = 5f;
    internal float currentHealth;

    [Header("Experience")]
    public float expBase = 50f;
    public float expVariance = 18f;

    [Header("Loot")]
    public LootTable lootTable;

    [Header("Balance / Poise")]
    public float maxBalance = 100f;
    public float balanceRegenRate = 15f;
    public float staggerThreshold = 0f;

    private float currentBalance;
    private float balanceRegenTimer;

    public Animator animator;

    private bool isDead = false;
    public GameObject healthBarPrefab;
    public GameObject barPosition;
    public string enemyName;
    internal EnemyHealthBar healthBar;

    public bool IsDead => isDead;

    public float CurrentHealthNormalized => currentHealth / maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentBalance = maxBalance;
        GameObject barGO = Instantiate(
            healthBarPrefab,
            barPosition.transform.position,
            Quaternion.identity,
            barPosition.transform
        );
        healthBar = barGO.GetComponent<EnemyHealthBar>();
        healthBar.Init(this, barPosition.transform);
        healthBar.UpdateHealth(CurrentHealthNormalized);
    }

    private void Update()
    {
        if (isDead) return;

        if (balanceRegenTimer > 0)
        {
            balanceRegenTimer -= Time.deltaTime;
        }
        else
        {
            currentBalance = Mathf.Min(
                maxBalance,
                currentBalance + balanceRegenRate * Time.deltaTime
            );
        }
    }

    public void TakeDamage(float damage, float balanceDamage, PlayerStats attacker = null)
    {
        if (isDead) return;

        float finalDamage = Mathf.Max(1f, damage - armor);
        currentHealth -= finalDamage;
        Debug.Log($"Манекен отримав {damage} урону");

        currentBalance -= balanceDamage;
        balanceRegenTimer = 2f;

        if (currentBalance <= staggerThreshold)
        {
            TriggerStagger();
            currentBalance = maxBalance;
        }

        if (healthBar != null) healthBar.UpdateHealth(CurrentHealthNormalized);

        if (currentHealth <= 0)
        {
            Die(attacker);
        }
    }

    void TriggerStagger()
    {
        combat.DisableHitbox();
        animator.SetTrigger("Hit");
    }

    void Die(PlayerStats killer)
    {
        isDead = true;
        currentHealth = 0;

        GetComponent<EnemyAIController>().enabled = false;

        if (killer != null)
        {
            PlayerStats playerStats = killer.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                float finalExp = Random.Range(expBase - expVariance, expBase + expVariance);
                playerStats.AddExperience(Mathf.Max(0f, finalExp));
            }
        }

        QuestManager.Instance.OnEnemyKilled(enemyId);
        PlayerKillStats.Instance.RegisterKill(enemyId);

        LootDropper.Drop(lootTable, transform.position);

        animator.SetTrigger("Die");
        if (healthBar != null) healthBar.OnDeath();

        Debug.Log("Манекен знищений");
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        isDead = currentHealth <= 0f;
        healthBar?.UpdateHealth(CurrentHealthNormalized);
    }

    public void ShowHealth()
    {
        healthBar?.ShowBar();
    }

    public void HideHealth()
    {
        healthBar?.OnDeath();
    }
}
