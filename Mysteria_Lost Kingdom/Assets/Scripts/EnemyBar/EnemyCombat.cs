using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public float attackCooldown = 1.5f;
    public float damage = 10f;
    public WeaponHitBoxEnemy hitbox;
    private float lastAttackTime;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<EnemyStats>().animator;
    }

    public void TryAttack(Transform target)
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;

        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        animator.SetTrigger("Attack");
    }

    // викликається з Animation Event
    public void EnableHitbox()
    {
        hitbox.EnableHitbox(damage);
    }

    public void DisableHitbox()
    {
        hitbox.DisableHitbox();
    }
}
