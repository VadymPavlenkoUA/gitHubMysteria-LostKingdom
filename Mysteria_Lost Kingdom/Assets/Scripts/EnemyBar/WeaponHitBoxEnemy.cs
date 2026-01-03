using UnityEngine;

public class WeaponHitBoxEnemy : MonoBehaviour
{
    private Collider col;
    private float damage;

    void Awake()
    {
        col = GetComponent<Collider>();
        col.enabled = false;
    }

    public void EnableHitbox(float dmg)
    {
        damage = dmg;
        col.enabled = true;
    }

    public void DisableHitbox()
    {
        col.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var playerStats = other.GetComponent<PlayerStats>();
        if (playerStats == null) return;

        playerStats.TakeDamage(damage);
    }
}
