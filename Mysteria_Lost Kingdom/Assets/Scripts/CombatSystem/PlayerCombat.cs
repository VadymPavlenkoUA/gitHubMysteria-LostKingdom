using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public PlayerStats playerStats;
    public EquipmentManager equipment;
    public Animator animator;

    [Header("Durability")]
    public float durabilityDamagePerHit = 1.5f;

    [Header("Combo Settings")]
    public int maxCombo = 3;
    public float comboResetTime = 0.9f;

    [Header("Combo Buffer")]
    public float postAttackComboBuffer = 1f;

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
        if (IsAttacking)
        {
            return;
        }

        if (!playerStats.TryUseStamina(GetAttackStaminaCost()))
            return;

        ItemInstance weapon = GetActiveWeaponItem();

        float attackSpeed = weapon != null
        ? weapon.item.attackSpeedMultiplier
        : 1f;

        animator.SetFloat("AttackSpeed", attackSpeed);

        if (isBlocking) EndBlock();

        IsAttacking = true;

        SetAttackIndexByWeapon();

        animator.SetFloat("ComboStep", Mathf.Clamp(comboStep, 0, maxCombo - 1));
        animator.SetTrigger("Attack");
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
                 equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
            animator.SetFloat("AttackIndex", 4);
        else if (equipment.isRightHandDrawn && equipment.isLeftHandDrawn && equipment.equippedLeftItem.item.categories != ItemCategory.Shield &&
            equipment.equippedLeftItem.item.weaponHandType != WeaponHandType.TwoHand)
            animator.SetFloat("AttackIndex", 5);

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
                 equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
            return equipment.equippedRightItem.item.staminaCostPerAttack;

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
            return equipment.equippedRightItem.item.staminaShieldCostPerSecond;

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

        else if (equipment.equippedLeftItem != null &&
                 equipment.equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
            return equipment.equippedRightItem.item.baseDefenseMultiplier;

        else if (equipment.equippedLeftItem != null &&
                 equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
            return equipment.equippedRightItem.item.baseDefenseMultiplier;

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
                 equipment.equippedLeftItem.item.categories == ItemCategory.Shield)
            return equipment.equippedRightItem;

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

    public void AnimationAttackEnd()
    {
        IsAttacking = false;

        if (!equipment.isLeftHandDrawn && !equipment.isRightHandDrawn)
        {
            equipment.ForceCombatIdle(3f);
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

    internal void StartBlock()
    {
        if (isBlocking)
            return;

        if (IsAttacking)
            return;

        if (!equipment.isLeftHandDrawn && !equipment.isRightHandDrawn) return;
        if (equipment.equippedRightItem == null && equipment.equippedLeftItem.item.categories == ItemCategory.Shield) return;

        if (!playerStats.TryUseStamina(1f))
            return;

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

    public void OnBlockedHit(float staminaDamage)
    {
        playerStats.UseStamina(staminaDamage);

        if (playerStats.currentStamina <= 0)
        {
            EndBlock();
        }
    }

    public void OnWeaponHit(EnemyStats enemy)
    {
        CombatStats combatStats = playerStats.CalculateCombatStats();
        float damage = Mathf.Max(combatStats.totalDamage - enemy.armor, 1f);

        enemy.TakeDamage(damage, playerStats);
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
