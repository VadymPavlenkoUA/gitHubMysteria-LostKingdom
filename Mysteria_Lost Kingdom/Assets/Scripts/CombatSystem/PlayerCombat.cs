using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public PlayerStats playerStats;
    public EquipmentManager equipment;
    public PlayerController playerController;
    public Animator animator;

    [Header("Durability")]
    public float durabilityDamagePerHit = 1.5f;

    [Header("Combo Settings")]
    public int maxCombo = 3;
    public float comboResetTime = 0.9f;

    [Header("Combo Buffer")]
    public float postAttackComboBuffer = 1f;

    [Header("Interrupt Penalty")]
    [SerializeField] private float interruptAttackSpeedMultiplier = 0.8f;
    [SerializeField] private int attacksWithPenaltyAfterInterrupt = 1;
    [SerializeField] private float interruptPenaltyTimeout = 1f;

    private int interruptPenaltyAttacksLeft = 0;
    private float interruptPenaltyTimer = 0f;

#pragma warning disable CS0414
    [SerializeField] private bool lastAttackWasInterrupted = false;
#pragma warning restore CS0414

    [SerializeField] private float ikBlendSpeed = 8f;
    private float leftHandIKWeight;

    private bool comboBufferActive;
    private float comboBufferTimer;

    private int comboStep = 0;

    private PlayerInputActions inputActions;
    internal bool isBlocking = false;
    private bool useLeftHandIK = false;

    public event Action OnBlockStarted;
    public event Action OnBlockEnded;
    private bool blockHeld;

    private float attackFailSafeTimer;
    private const float MAX_ATTACK_TIME = 5f;

    [SerializeField] private float attackCooldown = 0.5f; // пів секунди між атаками
    private float attackCooldownTimer = 0f;

    [SerializeField] private float attackStateGraceTime = 0.15f;
    private float attackStateCheckTimer = 0f;

    public bool IsAttacking { get; private set; }
    public bool CanCancelAttack { get; private set; } = false;
    public bool IsInStag { get; private set; } = false;

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
        if (IsAttacking)
        {
            attackFailSafeTimer -= Time.deltaTime;
            if (attackFailSafeTimer <= 0f)
            {
                Debug.LogWarning("Attack FAILSAFE triggered");
                AnimationDisableLeftHandIK();
                AttackEnd(true);
            }
        }

        if (blockHeld && !isBlocking)
        {
            TryStartBlockBuffered();
        }

        if (isBlocking)
        {
            if (!playerStats.TryUseStamina(GetBlockStaminaCost() * Time.deltaTime))
            {
                EndBlock();
            }
        }

        if (comboBufferActive)
        {
            comboBufferTimer -= Time.deltaTime;
            if (comboBufferTimer <= 0f)
            {
                comboBufferActive = false;
                comboStep = 0;
            }
        }

        if (interruptPenaltyAttacksLeft > 0)
        {
            interruptPenaltyTimer -= Time.deltaTime;

            if (interruptPenaltyTimer <= 0f)
            {
                interruptPenaltyAttacksLeft = 0;
                lastAttackWasInterrupted = false;
            }
        }

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        CheckAttackAnimatorState();
    }

    void CheckAttackAnimatorState()
    {
        if (!IsAttacking)
            return;

        if (attackStateCheckTimer > 0f)
        {
            attackStateCheckTimer -= Time.deltaTime;
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsTag("Attack"))
        {
            Debug.LogWarning("Attack desynced - InterruptAttack()");
            InterruptAttack();
        }
    }


    IEnumerator StartBlockNextFrame()
    {
        yield return null;
        StartBlock();
    }
    void TryStartBlockBuffered()
    {
        if ((IsAttacking && !CanCancelAttack) || IsInStag) return;

        if (IsAttacking && CanCancelAttack && !IsInStag)
        {
            InterruptAttack();
            StartCoroutine(StartBlockNextFrame());
            Debug.Log($"Trigger");
            return;
        }

        StartBlock();
    }

    public void InterruptAttack()
    {
        if (!IsAttacking) return;

        lastAttackWasInterrupted = true;
        interruptPenaltyAttacksLeft = attacksWithPenaltyAfterInterrupt;
        interruptPenaltyTimer = interruptPenaltyTimeout;

        animator.ResetTrigger("Attack");
        AnimationDisableLeftHandIK();
        AttackEnd(true);
        animator.CrossFade("Base Layer.Locomotion.Movement", 0.1f);
    }

    public void StagInterupt()
    {
        animator.ResetTrigger("Attack");
        IsInStag = true;
        animator.CrossFade("Stagger", 0.0f);
        IsAttacking = false;
        CanCancelAttack = false;
    }

    private void OnRightAttack(InputAction.CallbackContext ctx)
    {
        if (!IsAttacking && comboBufferActive)
        {
            comboBufferActive = false;
            TryAttack();
            return;
        }

        TryAttack();
    }

    private void OnBlockStart(InputAction.CallbackContext ctx)
    {
        blockHeld = true;
    }

    private void OnBlockEnd(InputAction.CallbackContext ctx)
    {
        blockHeld = false;
        EndBlock();
    }

    public void AnimationEnableAttackCancel()
    {
        CanCancelAttack = true;
    }

    public void AnimationDisableAttackCancel()
    {
        CanCancelAttack = false;
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
        if (attackCooldownTimer > 0f) return;
        //if ((IsAttacking && !CanCancelAttack) || IsInStag)
        //{
        //    return;
        //}

        if (IsAttacking || IsInStag || playerController.isRolling || playerController.isJumping)
        {
            return;
        }

        if (!playerStats.TryUseStamina(GetAttackStaminaCost()))
        {
            comboStep = 0;
            comboBufferActive = false;
            return;
        }

        attackCooldownTimer = attackCooldown;

        IsAttacking = true;
        attackStateCheckTimer = attackStateGraceTime;
        CanCancelAttack = false;

        ItemInstance weapon = GetActiveWeaponItem();

        float attackSpeed = weapon != null
        ? weapon.item.attackSpeedMultiplier
        : 1f;

        if (interruptPenaltyAttacksLeft > 0)
        {
            attackSpeed *= interruptAttackSpeedMultiplier;
        }

        animator.SetFloat("AttackSpeed", attackSpeed);

        if (isBlocking) EndBlock();

        attackFailSafeTimer = MAX_ATTACK_TIME;

        SetAttackIndexByWeapon();

        Debug.Log($"{IsAttacking}/{CanCancelAttack}");
        animator.SetFloat("ComboStep", Mathf.Clamp(comboStep, 0, maxCombo - 1));
        animator.Update(0f);

        if (interruptPenaltyAttacksLeft > 0)
        {
            interruptPenaltyAttacksLeft--;

            if (interruptPenaltyAttacksLeft <= 0)
            {
                lastAttackWasInterrupted = false;
            }
        }

        //animator.SetTrigger("Attack");
        animator.CrossFadeInFixedTime("Base Layer.AttackState", 0.1f, 0, 0f);
    }

    void SetAttackIndexByWeapon()
    {
        if (!equipment.isLeftHandDrawn && !equipment.isRightHandDrawn)
            animator.SetFloat("AttackIndex", 0);

        else if (equipment.isRightHandDrawn && !equipment.isLeftHandDrawn)
            animator.SetFloat("AttackIndex", 1);

        else if (equipment.isLeftHandDrawn && !equipment.isRightHandDrawn &&
                 equipment.equippedLeftItem.item.categories != ItemCategory.Shield)
            animator.SetFloat("AttackIndex", 2);

        else if (equipment.equippedLeftItem != null &&
                 equipment.equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
            animator.SetFloat("AttackIndex", 3);

        else if (equipment.equippedLeftItem != null &&
                 equipment.equippedLeftItem.item.categories == ItemCategory.Shield && equipment.isRightHandDrawn)
            animator.SetFloat("AttackIndex", 4);
        else if (equipment.isRightHandDrawn && equipment.isLeftHandDrawn && equipment.equippedLeftItem.item.categories != ItemCategory.Shield &&
            equipment.equippedLeftItem.item.weaponHandType != WeaponHandType.TwoHand)
            animator.SetFloat("AttackIndex", 5);
        else if (!equipment.isRightHandDrawn && equipment.isLeftHandDrawn && equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
            animator.SetFloat("AttackIndex", 6);
        else
            animator.SetFloat("AttackIndex", 0);
    }

    float GetAttackStaminaCost()
    {
        if (!equipment.isLeftHandDrawn && !equipment.isRightHandDrawn)
            return 5;

        else if (equipment.isRightHandDrawn && !equipment.isLeftHandDrawn)
            return equipment.equippedRightItem.item.staminaCostPerAttack;

        else if (equipment.isLeftHandDrawn && !equipment.isRightHandDrawn &&
                 equipment.equippedLeftItem.item.categories != ItemCategory.Shield)
            return equipment.equippedLeftItem.item.staminaCostPerAttack;

        else if (equipment.equippedLeftItem != null &&
                 equipment.equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
            return equipment.equippedRightItem.item.staminaCostPerAttack;

        else if (equipment.equippedLeftItem != null &&
                 equipment.equippedLeftItem.item.categories == ItemCategory.Shield && equipment.isRightHandDrawn)
            return equipment.equippedRightItem.item.staminaCostPerAttack;
        else if (!equipment.isRightHandDrawn && equipment.isLeftHandDrawn && equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
            return equipment.equippedLeftItem.item.staminaCostPerAttack;

        else
            return 5f;
    }

    float GetBlockStaminaCost()
    {
        if (!equipment.isLeftHandDrawn && !equipment.isRightHandDrawn)
            return 1;

        else if (equipment.isRightHandDrawn && !equipment.isLeftHandDrawn)
            return equipment.equippedRightItem.item.staminaShieldCostPerSecond;

        else if (equipment.isLeftHandDrawn && !equipment.isRightHandDrawn &&
                 equipment.equippedLeftItem.item.categories != ItemCategory.Shield)
            return equipment.equippedLeftItem.item.staminaShieldCostPerSecond;

        else if (equipment.equippedLeftItem != null &&
                 equipment.equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
            return equipment.equippedRightItem.item.staminaShieldCostPerSecond;

        else if (equipment.equippedLeftItem != null &&
                 equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
            return equipment.equippedLeftItem.item.staminaShieldCostPerSecond;

        else
            return 1f;
    }

    float GetBlockMultiplier()
    {
        if (!equipment.isLeftHandDrawn && !equipment.isRightHandDrawn)
            return 1.1f;

        else if (equipment.isRightHandDrawn && !equipment.isLeftHandDrawn)
            return equipment.equippedRightItem.item.baseDefenseMultiplier;

        else if (equipment.isLeftHandDrawn && !equipment.isRightHandDrawn &&
                 equipment.equippedLeftItem.item.categories != ItemCategory.Shield)
            return equipment.equippedLeftItem.item.baseDefenseMultiplier;

        else if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
            return equipment.equippedRightItem.item.baseDefenseMultiplier;

        else if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
            return equipment.equippedLeftItem.item.baseDefenseMultiplier;

        else if (equipment.isRightHandDrawn && equipment.isLeftHandDrawn && equipment.equippedLeftItem.item.categories != ItemCategory.Shield && equipment.equippedLeftItem.item.weaponHandType != WeaponHandType.TwoHand)
        {
            return equipment.equippedRightItem.item.baseDefenseMultiplier * equipment.equippedLeftItem.item.baseDefenseMultiplier;
        }

        else
            return 1.1f;
    }

    ItemInstance GetActiveWeaponItem()
    {
        if (equipment.isRightHandDrawn && !equipment.isLeftHandDrawn)
            return equipment.equippedRightItem;

        else if (equipment.isLeftHandDrawn && !equipment.isRightHandDrawn &&
                 equipment.equippedLeftItem.item.categories != ItemCategory.Shield)
            return equipment.equippedLeftItem;

        else if (equipment.equippedLeftItem != null &&
                 equipment.equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
            return equipment.equippedRightItem;

        else if (equipment.equippedLeftItem != null &&
                 equipment.equippedLeftItem.item.categories == ItemCategory.Shield && equipment.isRightHandDrawn)
            return equipment.equippedRightItem;

        else if (!equipment.isRightHandDrawn && equipment.isLeftHandDrawn && equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
            return equipment.equippedLeftItem;

        return null;
    }


    void OnAnimatorIK(int layerIndex)
    {
        bool shouldUseIK =
            useLeftHandIK &&
            equipment.twoHandEquipped &&
            equipment.currentRightHandItem != null &&
            equipment.isRightHandDrawn;

        float targetWeight = shouldUseIK ? 1f : 0f;
        leftHandIKWeight = Mathf.Lerp(
            leftHandIKWeight,
            targetWeight,
            Time.deltaTime * ikBlendSpeed
        );

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);

        if (!shouldUseIK || leftHandIKWeight < 0.01f)
            return;

        Transform leftGrip = equipment.currentRightHandItem.transform.Find("LeftHandGrip");
        if (leftGrip == null) return;

        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftGrip.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftGrip.rotation);
    }



    public void AnimationHit()
    {
        EnableHitbox();
    }

    public void AnimationHitWithHands()
    {
        EnableHitbox();
    }

    public void AttackEnd(bool interrupted = false)
    {
        IsAttacking = false;
        CanCancelAttack = false;

        if (interrupted)
        {
            comboStep = 0;
            comboBufferActive = false;
            return;
        }

        comboStep++;

        if (comboStep >= maxCombo)
        {
            comboStep = 0;
            comboBufferActive = false;
            return;
        }

        comboBufferActive = true;
        comboBufferTimer = postAttackComboBuffer;
    }

    public void AttackEndStagger()
    {
        IsInStag = false;
        //if (!equipment.isLeftHandDrawn && !equipment.isRightHandDrawn)
        //{
        //    equipment.ForceCombatIdle(3f);
        //}
        comboStep = 0;
        comboBufferActive = false;
        animator.CrossFade("Base Layer.Locomotion.Movement", 0.1f);
    }

    public void AnimationAttackEnd()
    {
        IsAttacking = false;
        CanCancelAttack = false;
        animator.SetTrigger("AttackInterupt");

        if (!equipment.isLeftHandDrawn && !equipment.isRightHandDrawn)
        {
            equipment.ForceCombatIdle(3f);
        }

        interruptPenaltyAttacksLeft = 0;
        lastAttackWasInterrupted = false;

        comboStep++;

        if (comboStep >= maxCombo)
        {
            comboStep = 0;
            comboBufferActive = false;
            return;
        }

        comboBufferActive = true;
        comboBufferTimer = postAttackComboBuffer;
    }

    public void AnimationDisableHitBox()
    {
        DisableHitbox();
    }

    void DamageWeaponDurability()
    {
        if (!equipment.isLeftHandDrawn && !equipment.isRightHandDrawn)
            return;

        else if (equipment.isRightHandDrawn && !equipment.isLeftHandDrawn)
            equipment.DamageDurability(equipment.equippedRightItem, durabilityDamagePerHit);

        else if (equipment.isLeftHandDrawn && !equipment.isRightHandDrawn && equipment.equippedLeftItem.item.categories != ItemCategory.Shield)
            equipment.DamageDurability(equipment.equippedLeftItem, durabilityDamagePerHit);

        else if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
        {
            equipment.DamageDurability(equipment.equippedRightItem, durabilityDamagePerHit);
        }

        else if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.categories == ItemCategory.Shield && equipment.isRightHandDrawn)
            equipment.DamageDurability(equipment.equippedRightItem, durabilityDamagePerHit);

        else if (!equipment.isRightHandDrawn && equipment.isLeftHandDrawn && equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
            equipment.DamageDurability(equipment.equippedLeftItem, durabilityDamagePerHit);

        else if (equipment.isRightHandDrawn && equipment.isLeftHandDrawn && equipment.equippedLeftItem.item.categories != ItemCategory.Shield && equipment.equippedLeftItem.item.weaponHandType != WeaponHandType.TwoHand)
        {
            equipment.DamageDurability(equipment.equippedRightItem, durabilityDamagePerHit);
            equipment.DamageDurability(equipment.equippedLeftItem, durabilityDamagePerHit);
        }

        else
            return;
    }

    internal void StartBlock()
    {
        if (isBlocking) return;

        if (!equipment.isLeftHandDrawn && !equipment.isRightHandDrawn) return;
        if (!playerStats.TryUseStamina(1f)) return;

        isBlocking = true;
        animator.ResetTrigger("BlockExit");
        animator.SetTrigger("BlockEnter");
        animator.SetBool("IsBlocking", true);

        playerStats.blockMultiplier = GetBlockMultiplier();
        OnBlockStarted?.Invoke();
    }

    internal void EndBlock()
    {
        if (!isBlocking) return;

        isBlocking = false;
        animator.SetBool("IsBlocking", false);
        animator.ResetTrigger("BlockEnter");
        animator.SetTrigger("BlockExit");

        playerStats.blockMultiplier = 1f;

        OnBlockEnded?.Invoke();
    }

    public void OnBlockedHit()
    {
        playerStats.UseStamina(30f);

        if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.categories == ItemCategory.Shield && equipment.isLeftHandDrawn)
        {
            equipment.DamageDurability(equipment.equippedLeftItem, durabilityDamagePerHit);
        }
        else if (equipment.isLeftHandDrawn && !equipment.isRightHandDrawn && equipment.equippedLeftItem.item.categories != ItemCategory.Shield)
        {
            equipment.DamageDurability(equipment.equippedLeftItem, durabilityDamagePerHit);
        }
        else if (equipment.isRightHandDrawn && !equipment.isLeftHandDrawn)
        {
            equipment.DamageDurability(equipment.equippedRightItem, durabilityDamagePerHit);
        }
        else if (equipment.equippedLeftItem != null && equipment.equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
        {
            equipment.DamageDurability(equipment.equippedRightItem, durabilityDamagePerHit);
        }
        else if (equipment.isRightHandDrawn && equipment.isLeftHandDrawn && equipment.equippedLeftItem.item.categories != ItemCategory.Shield && equipment.equippedLeftItem.item.weaponHandType != WeaponHandType.TwoHand)
        {
            equipment.DamageDurability(equipment.equippedRightItem, durabilityDamagePerHit);
            equipment.DamageDurability(equipment.equippedLeftItem, durabilityDamagePerHit);
        }

        if (playerStats.currentStamina <= 0)
        {
            EndBlock();
        }
    }

    public void OnWeaponHit(EnemyStats enemy)
    {
        CombatStats combatStats = playerStats.CalculateCombatStats();
        float damage = Mathf.Max(combatStats.totalDamage, 1f);
        float balanceDamage = Mathf.Max(combatStats.totalBalanceDamage, 1f);

        enemy.TakeDamage(damage, balanceDamage, playerStats);
        DamageWeaponDurability();
    }

    public bool CanMove()
    {
        if (IsInStag) return false;
        if (IsAttacking && !CanCancelAttack) return false;
        return true;
    }

    public void TryInterruptByMovement()
    {
        if (!IsAttacking) return;
        if (!CanCancelAttack) return;
        InterruptAttack();
    }

    public bool TryInterruptForAction()
    {
        if (!IsAttacking) return true;
        if (!CanCancelAttack) return false;

        InterruptAttack();
        return true;
    }

    public void EnterStagger()
    {
        AnimationDisableHitBox();
        StagInterupt();
    }

    public void EnableHitbox()
    {
        equipment.EnableWeaponHitboxes();
    }

    public void DisableHitbox()
    {
        equipment.DisableWeaponHitboxes();
    }

    public void ResetCombo()
    {
        comboStep = 0;
        comboBufferActive = false;
    }
}
