using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHealth = 100f;
    public float armor = 5f;

    public Animator animator;

    private float currentHealth;
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

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"Манекен отримав {amount} урону");

        animator.SetTrigger("Hit");

        if (healthBar != null) healthBar.UpdateHealth(CurrentHealthNormalized);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        currentHealth = 0;

        animator.SetTrigger("Die");
        if (healthBar != null) healthBar.OnDeath();

        Debug.Log("Манекен знищений");
    }
}
