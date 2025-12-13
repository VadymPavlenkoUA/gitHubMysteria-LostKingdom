using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public PlayerStats playerStats;
    public EquipmentManager equipment;
    public Animator animator;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackStaminaCost = 15f;
    public LayerMask enemyLayer;

    [Header("Block Settings")]
    public float blockDefenseMultiplier = 2f;
    public float blockStaminaCostPerSecond = 5f;

    [Header("Durability")]
    public float durabilityDamagePerHit = 1.5f;

    private PlayerInputActions inputActions;
    private bool isBlocking = false;

    private void Awake()
    {
        inputActions = MenuController.Instance.inputActions;
    }

    private void OnEnable()
    {
        inputActions.Combat.Enable();
        inputActions.Combat.RightHandAction.performed += ctx => OnRightAttack();
        inputActions.Combat.LeftHandAction.performed += ctx => OnBlockStart();
        inputActions.Combat.LeftHandAction.canceled += ctx => OnBlockEnd();
    }

    private void OnDisable()
    {
        inputActions.Combat.RightHandAction.performed -= ctx => OnRightAttack();
        inputActions.Combat.LeftHandAction.performed -= ctx => OnBlockStart();
        inputActions.Combat.LeftHandAction.canceled -= ctx => OnBlockEnd();
        inputActions.Combat.Disable();
    }

    private void Update()
    {
        if (isBlocking)
        {
            if (!playerStats.TryUseStamina(blockStaminaCostPerSecond * Time.deltaTime))
            {
                EndBlock();
            }
        }
    }

    private void OnRightAttack()
    {
        TryAttack();
    }

    private void OnBlockStart()
    {
        StartBlock();
    }

    private void OnBlockEnd()
    {
        EndBlock();
    }

    void TryAttack()
    {
        if (!playerStats.TryUseStamina(attackStaminaCost)) return;

        if (!equipment.isLeftHandDrawn && !equipment.isLeftHandDrawn)
        {
            animator.SetTrigger("Attack");
        }
        else if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
        {
            //animator.SetTrigger("AttackWithShield");
        }
        else if (equipment.equippedRightItem != null && equipment.equippedRightItem.item.categories == ItemCategory.Shield)
        {
            //animator.SetTrigger("AttackWithShieldRight");
        }
        else if (equipment.equippedRightItem != null && equipment.equippedRightItem.item.weaponHandType != WeaponHandType.TwoHand && equipment.equippedRightItem.item.categories == ItemCategory.Weapon && 
            equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.categories == ItemCategory.Weapon)
        {
            //animator.SetTrigger("AttackWithTwoWeapons");
        }
        else if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
        {
            //animator.SetTrigger("AttackTwoHanded");
        }
        else
        {
            animator.SetTrigger("Attack");
        }

        PerformAttack();
    }

    void PerformAttack()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 1.4f, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, enemyLayer))
        {
            var enemy = hit.collider.GetComponent<EnemyStats>();
            if (enemy == null) return;

            CombatStats combat = playerStats.CalculateCombatStats();
            float damage = Mathf.Max(combat.totalDamage - enemy.armor, 1f);

            enemy.TakeDamage(damage);
            DamageWeaponDurability();
        }
    }

    void DamageWeaponDurability()
    {
        if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
        {
            equipment.DamageDurability(equipment.equippedRightItem, durabilityDamagePerHit);
        }
        else if (equipment.equippedRightItem != null && equipment.equippedRightItem.item.categories == ItemCategory.Shield)
        {
            equipment.DamageDurability(equipment.equippedLeftItem, durabilityDamagePerHit);
        }
        else if (equipment.equippedRightItem != null && equipment.equippedRightItem.item.weaponHandType != WeaponHandType.TwoHand && equipment.equippedRightItem.item.categories == ItemCategory.Weapon &&
            equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.categories == ItemCategory.Weapon)
        {
            equipment.DamageDurability(equipment.equippedRightItem, durabilityDamagePerHit);
        }
        else if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
        {
            equipment.DamageDurability(equipment.equippedRightItem, durabilityDamagePerHit);
        }
        else
        {
            equipment.DamageDurability(equipment.equippedRightItem, durabilityDamagePerHit);
        }
    }

    void StartBlock()
    {
        isBlocking = true;
        //animator.SetBool("Block", true);
        playerStats.blockMultiplier = blockDefenseMultiplier; 
    }

    void EndBlock()
    {
        isBlocking = false;
        //animator.SetBool("Block", false);
        playerStats.blockMultiplier = 1f;
    }
}
