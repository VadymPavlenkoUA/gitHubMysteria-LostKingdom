using UnityEngine;

public class WeaponHitBoxEnemy : MonoBehaviour
{
    private Collider col;
    private float damage;
    private bool isActive;
    private bool hasHit;


    void Awake()
    {
        col = GetComponent<Collider>();
        col.enabled = false;
        isActive = false;
    }

    public void EnableHitbox(float dmg)
    {
        damage = dmg;
        hasHit = false;
        isActive = true;
        col.enabled = true;
    }

    public void DisableHitbox()
    {
        isActive = false;
        col.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if(!isActive || hasHit) return;
        if (!other.CompareTag("Player")) return;

        hasHit = true;
        var playerStats = other.GetComponent<PlayerStats>();
        if (playerStats == null) return;

        playerStats.TakeDamage(damage);
    }
}
