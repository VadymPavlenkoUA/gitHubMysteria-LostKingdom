using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Identity")]
    public string enemyId; // goblin_warrior, boss_dragon_01
    public bool isUnique;

    [Header("Health")]
    public float maxHealth = 100f;
    public float armor = 5f;
    private float currentHealth;

    [Header("Experience")]
    public float expBase = 50f;
    public float expVariance = 18f;

    public Animator animator;

    private bool isDead = false;
    public GameObject healthBarPrefab;
    public GameObject barPosition;
    public string enemyName;
    private EnemyHealthBar healthBar;

    public float CurrentHealthNormalized => currentHealth / maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
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

    public void TakeDamage(float amount, PlayerStats attacker = null)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"Манекен отримав {amount} урону");

        animator.SetTrigger("Hit");

        if (healthBar != null) healthBar.UpdateHealth(CurrentHealthNormalized);

        if (currentHealth <= 0)
        {
            Die(attacker);
        }
    }

    void Die(PlayerStats killer)
    {
        isDead = true;
        currentHealth = 0;

        if (killer != null)
        {
            PlayerStats playerStats = killer.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                float finalExp = Random.Range(expBase - expVariance, expBase + expVariance);
                playerStats.AddExperience(Mathf.Max(0f, finalExp));
            }
        }

        PlayerKillStats.Instance.RegisterKill(enemyId);

        animator.SetTrigger("Die");
        if (healthBar != null) healthBar.OnDeath();

        Debug.Log("Манекен знищений");
    }
}
