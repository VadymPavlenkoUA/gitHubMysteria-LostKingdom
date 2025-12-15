using UnityEngine;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    private PlayerCombat combat;
    private Collider hitboxCollider;
    private HashSet<EnemyStats> hitEnemies = new();

    private void Awake()
    {
        combat = GetComponentInParent<PlayerCombat>();
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.enabled = false;
    }

    private void OnEnable()
    {
        hitEnemies.Clear();
    }

    public void EnableHitbox()
    {
        hitEnemies.Clear();
        hitboxCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!combat.IsAttacking) return;
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyStats>();
        if (enemy == null) return;

        if (hitEnemies.Contains(enemy)) return;

        hitEnemies.Add(enemy);

        combat.OnWeaponHit(enemy);
    }
}
