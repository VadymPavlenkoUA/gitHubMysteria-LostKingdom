using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public PlayerStats playerStats;
    public EquipmentManager equipment;
    public Animator animator;

    [Header("Attack Settings")]
    public float attackStaminaCost = 15f;

    [Header("Block Settings")]
    public float blockDefenseMultiplier = 2f;
    public float blockStaminaCostPerSecond = 5f;

    [Header("Durability")]
    public float durabilityDamagePerHit = 1.5f;

    private PlayerInputActions inputActions;
    private bool isBlocking = false;
    private bool useLeftHandIK = false;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        inputActions = MenuController.Instance.inputActions;
    }

    private void OnEnable()
    {
        inputActions.Combat.Enable();
        inputActions.Combat.RightHandAction.performed += OnRightAttack;
        inputActions.Combat.LeftHandAction.performed += OnBlockStart;
        inputActions.Combat.LeftHandAction.canceled += OnBlockEnd;
    }

    private void OnDisable()
    {
        inputActions.Combat.RightHandAction.performed -= OnRightAttack;
        inputActions.Combat.LeftHandAction.performed -= OnBlockStart;
        inputActions.Combat.LeftHandAction.canceled -= OnBlockEnd;
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

    private void OnRightAttack(InputAction.CallbackContext ctx)
    {
        TryAttack();
    }

    private void OnBlockStart(InputAction.CallbackContext ctx)
    {
        StartBlock();
    }

    private void OnBlockEnd(InputAction.CallbackContext ctx)
    {
        EndBlock();
    }

    public void AnimationEnableLeftHandIK()
    {
        useLeftHandIK = true;
    }

    public void AnimationDisableLeftHandIK()
    {
        useLeftHandIK = false;
    }


    void TryAttack()
    {
        if (IsAttacking) return;
        if (!playerStats.TryUseStamina(attackStaminaCost)) return;

        if (!equipment.isLeftHandDrawn && !equipment.isRightHandDrawn)
        {
            IsAttacking = true;
            animator.SetFloat("AttackIndex", 0);
            animator.SetTrigger("Attack");
        }
        else if (equipment.isRightHandDrawn && !equipment.isLeftHandDrawn)
        {
            IsAttacking = true;
            animator.SetFloat("AttackIndex", 1);
            animator.SetTrigger("Attack");
        }
        else if (equipment.isLeftHandDrawn && !equipment.isRightHandDrawn)
        {
            IsAttacking = true;
            animator.SetFloat("AttackIndex", 2);
            animator.SetTrigger("Attack");
        }
        else if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand && equipment.isRightHandDrawn)
        {
            IsAttacking = true;
            animator.SetFloat("AttackIndex", 3);
            animator.SetTrigger("Attack");
        }
        else if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.categories == ItemCategory.Shield && equipment.isLeftHandDrawn)
        {
            IsAttacking = true;
            animator.SetFloat("AttackIndex", 4);
            animator.SetTrigger("Attack");
        }
        else if (equipment.equippedRightItem != null && equipment.equippedRightItem.item.weaponHandType != WeaponHandType.TwoHand && equipment.equippedRightItem.item.categories == ItemCategory.Weapon &&
            equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.categories == ItemCategory.Weapon && equipment.isRightHandDrawn && equipment.isLeftHandDrawn)
        {
            IsAttacking = true;
            //animator.SetTrigger("AttackWithTwoWeapons");
        }
        else
        {
            IsAttacking = true;
            animator.SetFloat("AttackIndex", 0);
            animator.SetTrigger("Attack");
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!useLeftHandIK)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
            return;
        }

        if (equipment.twoHandEquipped &&
            equipment.currentRightHandItem != null &&
            equipment.isRightHandDrawn)
        {
            Transform leftGrip = equipment.currentRightHandItem.transform.Find("LeftHandGrip");
            if (leftGrip == null) return;

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);

            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftGrip.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftGrip.rotation);
        }
    }


    public void AnimationHit()
    {
        EnableHitbox();
        DamageWeaponDurability();
    }

    public void AnimationHitWithHands()
    {
        EnableHitbox();
    }

    public void AnimationAttackEnd()
    {
        IsAttacking = false;
    }

    public void AnimationDisableHitBox()
    {
        DisableHitbox();
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

    public void OnWeaponHit(EnemyStats enemy)
    {
        CombatStats combatStats = playerStats.CalculateCombatStats();
        float damage = Mathf.Max(combatStats.totalDamage - enemy.armor, 1f);

        enemy.TakeDamage(damage);
        DamageWeaponDurability();
    }

    public void EnableHitbox()
    {
        equipment.EnableWeaponHitboxes();
    }

    public void DisableHitbox()
    {
        equipment.DisableWeaponHitboxes();
    }

}
