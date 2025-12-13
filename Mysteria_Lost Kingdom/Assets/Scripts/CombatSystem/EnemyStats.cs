using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHealth = 100f;
    public float armor = 5f;

    public Animator animator;

    private float currentHealth;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"Манекен отримав {amount} урону");

        animator.SetTrigger("Hit");

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

        Debug.Log("Манекен знищений");
    }
}
