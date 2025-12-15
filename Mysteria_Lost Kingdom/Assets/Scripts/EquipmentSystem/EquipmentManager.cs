using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;
using static InventorySlotUI;

[System.Serializable]
public class ArmourMeshes
{
    public GameObject[] headArmourMeshes;
    public GameObject[] chestArmourMeshes;
    public GameObject[] legsArmourMeshes;
    public GameObject[] bootsArmourMeshes;
    public GameObject[] glovesArmourMeshes;
    public GameObject[] beltArmourMeshes;
}

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    public PlayerStats playerStats;

    [Header("Слоти для екіпіювання")]
    public Transform rightHand;
    public Transform leftHand;
    public Transform backSocket;
    public Transform rightBeltSocket;
    public Transform leftBeltSocket;
    public ArmourMeshes armourMeshes;
    //public Animator animator;

    internal GameObject currentRightHandItem;
    internal GameObject currentLeftHandItem;

    internal ItemInstance equippedRightItem;
    internal ItemInstance equippedLeftItem;
    internal ItemInstance equippedHeadArmourItem;
    internal ItemInstance equippedChestArmourItem;
    internal ItemInstance equippedLegArmourItem;
    internal ItemInstance equippedBootsItem;
    internal ItemInstance equippedGlovesItem;
    internal ItemInstance equippedBeltItem;

    public bool isLeftHandDrawn;
    public bool isRightHandDrawn;
    internal bool isBowDrawn;

    public bool isRTCharacter = false;
    internal bool twoHandEquipped = false;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        if (!isRTCharacter)
        {
            Instance = this;
            inputActions = MenuController.Instance.inputActions;
        }
    }

    private void Update()
    {
        if (inputActions != null && !isRTCharacter)
        {
            if (inputActions.HotBar.RightHand.WasPressedThisFrame())
            {
                if (equippedRightItem == null) return;
                UseActionManager.Instance.StartUse(0.5f, () => ConfirmEquipRighHand(), () => Debug.Log("Маніпуляції з хотбаром права рука!"));
            }
            if (inputActions.HotBar.LeftHand.WasPressedThisFrame())
            {
                if (equippedLeftItem == null) return;
                UseActionManager.Instance.StartUse(0.5f, () => ConfirmEquipLeftHand(), () => Debug.Log("Маніпуляції з хотбаром ліва рука!"));
            }
            if (inputActions.HotBar.RangeWeapon.WasPressedThisFrame()) return;
            if (inputActions.HotBar.ThrowableWeapon.WasPressedThisFrame()) return;
        }
    }

    private void OnEnable()
    {
        if (inputActions != null && !isRTCharacter)
        {
            inputActions.HotBar.Enable();
        }
    }

    private void OnDisable()
    {
        if (inputActions != null && !isRTCharacter)
        {
            inputActions?.HotBar.Disable();
        }
    }

    public void EquipItem(ItemInstance inst, SlotSpecification slotSpecification)
    {
        if (inst == null || inst.item == null) return;

        var item = inst.item;

        if ((item.categories & ItemCategory.Weapon) != 0 ||
            (item.categories & ItemCategory.Bow) != 0 ||
            (item.categories & ItemCategory.Shield) != 0)
        {
            EquipWeapon(inst, slotSpecification);
        }
        else
        {
            EquipArmour(inst);
        }

        if (!isRTCharacter)
        {
            playerStats.InvokeCombatChanged();
            RTCharacterManager.Instance?.SyncFromMain();
        }
    }

    internal void SetArmour(GameObject[] meshArray, int index)
    {
        for (int i = 0; i < meshArray.Length; i++)
            meshArray[i].SetActive(i == index);
    }


    private void EquipWeapon(ItemInstance inst, InventorySlotUI.SlotSpecification slotSpecification)
    {
        var item = inst.item;
        if (item.weaponHandType == WeaponHandType.TwoHand)
        {
            equippedRightItem = inst;
            equippedLeftItem = inst;
            twoHandEquipped = true;

            return;
        }

        if (item.weaponHandType == WeaponHandType.OneHand && slotSpecification == InventorySlotUI.SlotSpecification.RightHand)
        {
            equippedRightItem = inst;
            return;
        }

        if (item.weaponHandType == WeaponHandType.OneHand && slotSpecification == InventorySlotUI.SlotSpecification.LeftHand)
        {
            equippedLeftItem = inst;

            return;
        }

        Debug.LogWarning("Weapon has NO WeaponHandType assigned!");
    }

    private void EquipArmour(ItemInstance inst)
    {
        var item = inst.item;
        switch (item.categories)
        {
            case ItemCategory.ArmourHead:
                SetArmour(armourMeshes.headArmourMeshes, item.meshIndex);
                equippedHeadArmourItem = inst;
                break;

            case ItemCategory.ArmourChest:
                SetArmour(armourMeshes.chestArmourMeshes, item.meshIndex);
                equippedChestArmourItem = inst;
                break;

            case ItemCategory.ArmourLegs:
                SetArmour(armourMeshes.legsArmourMeshes, item.meshIndex);
                equippedLegArmourItem = inst;
                break;

            case ItemCategory.ArmourBelt:
                SetArmour(armourMeshes.beltArmourMeshes, item.meshIndex);
                equippedBeltItem = inst;
                break;

            case ItemCategory.ArmourGloves:
                SetArmour(armourMeshes.glovesArmourMeshes, item.meshIndex);
                equippedGlovesItem = inst;
                break;

            case ItemCategory.ArmourBoots:
                SetArmour(armourMeshes.bootsArmourMeshes, item.meshIndex);
                equippedBootsItem = inst;
                break;
        }
    }


    private void ConfirmEquipRighHand()
    {
        if (isRightHandDrawn && equippedRightItem.item.weaponHandType == WeaponHandType.TwoHand)
        {
            HideTwoHanded();
            return;
        }
        else if (isRightHandDrawn)
        {
            HideRightHand();
            return;
        }
        if (equippedRightItem.item.weaponHandType == WeaponHandType.TwoHand)
        {
            DrawTwoHand();
            return;
        }

        DrawRightHand();
    }

    private void ConfirmEquipLeftHand()
    {
        if (isLeftHandDrawn && equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
        {
            HideTwoHanded();
            return;
        }
        else if (isLeftHandDrawn)
        {
            HideLeftHand();
            return;
        }
        if (equippedLeftItem.item.weaponHandType == WeaponHandType.TwoHand)
        {
            DrawTwoHand();
            return;
        }

        DrawLeftHand();
    }

    internal void DrawTwoHand()
    {
        if (currentRightHandItem != null) Destroy(currentRightHandItem);
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);
        currentRightHandItem = EquipWeaponInHand(equippedRightItem, rightHand, equippedRightItem.item.rightHandPosition, equippedRightItem.item.rightHandRotation);

        isRightHandDrawn = true;
        isLeftHandDrawn = true;

        //animator.SetBool("WeaponEquipped", true);
        //animator.SetInteger("WeaponType", 3); 

        if (!isRTCharacter)
        {
            playerStats.InvokeCombatChanged();
            RTCharacterManager.Instance?.SyncFromMain();
        }
    }

    internal void DrawRightHand()
    {
        if (currentRightHandItem != null) Destroy(currentRightHandItem);
        currentRightHandItem = EquipWeaponInHand(equippedRightItem, rightHand, equippedRightItem.item.rightHandPosition, equippedRightItem.item.rightHandRotation);

        isRightHandDrawn = true;

        //animator.SetBool("WeaponEquipped", true);
        //animator.SetInteger("WeaponType", 1);

        if (!isRTCharacter)
        {
            playerStats.InvokeCombatChanged();
            RTCharacterManager.Instance?.SyncFromMain();
        }
    }

    internal void DrawLeftHand()
    {
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);
        currentLeftHandItem = EquipWeaponInHand(equippedLeftItem, leftHand, equippedLeftItem.item.leftHandPosition, equippedLeftItem.item.leftHandRotation);

        isLeftHandDrawn = true;

        //animator.SetBool("WeaponEquipped", true);
        //animator.SetInteger("WeaponType", 2);

        if (!isRTCharacter)
        {
            playerStats.InvokeCombatChanged();
            RTCharacterManager.Instance?.SyncFromMain();
        }
    }

    internal void HideRightHand()
    {
        if (!isRightHandDrawn) return;
        isRightHandDrawn = false;
        if (currentRightHandItem != null) Destroy(currentRightHandItem);
        if (equippedRightItem != null)
        {
            if (equippedRightItem.item.categories == ItemCategory.Shield) currentRightHandItem = EquipWeaponInactive(equippedRightItem, backSocket, equippedRightItem.item.inactivePosition, equippedRightItem.item.inactiveRotation);
            if (equippedRightItem.item.categories == ItemCategory.Weapon && equippedRightItem.item.weaponHandType == WeaponHandType.OneHand) currentRightHandItem = EquipWeaponInactive(equippedRightItem, rightBeltSocket, equippedRightItem.item.inactivePosition, equippedRightItem.item.inactiveRotation);
        }

        if (!isRTCharacter)
        {
            playerStats.InvokeCombatChanged();
            RTCharacterManager.Instance?.SyncFromMain();
        }
    }

    internal void HideLeftHand()
    {
        if (!isLeftHandDrawn) return;
        isLeftHandDrawn = false;
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);
        if (equippedLeftItem != null)
        {
            if (equippedLeftItem.item.categories == ItemCategory.Shield) currentLeftHandItem = EquipWeaponInactive(equippedLeftItem, backSocket, equippedLeftItem.item.inactivePosition, equippedLeftItem.item.inactiveRotation);
            if (equippedLeftItem.item.categories == ItemCategory.Weapon && equippedLeftItem.item.weaponHandType == WeaponHandType.OneHand) currentLeftHandItem = EquipWeaponInactive(equippedLeftItem, leftBeltSocket, equippedLeftItem.item.inactiveAltPosition, equippedLeftItem.item.inactiveAltRotation);
        }

        if (!isRTCharacter)
        {
            playerStats.InvokeCombatChanged();
            RTCharacterManager.Instance?.SyncFromMain();
        }
    }

    internal void HideTwoHanded()
    {
        if (!isRightHandDrawn && !isLeftHandDrawn) return;
        isRightHandDrawn = false;
        isLeftHandDrawn = false;
        if (currentRightHandItem != null) Destroy(currentRightHandItem);
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);
        if (equippedRightItem != null) currentRightHandItem = EquipWeaponInactive(equippedRightItem, backSocket, equippedRightItem.item.inactivePosition, equippedRightItem.item.inactiveRotation);

        if (!isRTCharacter)
        {
            playerStats.InvokeCombatChanged();
            RTCharacterManager.Instance?.SyncFromMain();
        }
    }

    internal GameObject EquipWeaponInHand(ItemInstance inst, Transform hand, Vector3 position, Vector3 rotation)
    {
        var item = inst.item;
        GameObject handItem = Instantiate(item.itemPrefabEquip, hand);
        FixItemScale(handItem.transform, hand, item.itemPrefabEquip.transform.localScale);
        handItem.transform.localPosition = position;
        handItem.transform.localRotation = Quaternion.Euler(rotation);
        
        return handItem;
    }

    internal GameObject EquipWeaponInactive(ItemInstance inst, Transform hand, Vector3 position, Vector3 rotation)
    {
        var item = inst.item;
        GameObject handItem = Instantiate(item.itemPrefabEquip, hand);
        FixItemScale(handItem.transform, hand, item.itemPrefabEquip.transform.localScale);
        handItem.transform.localPosition = position;
        handItem.transform.localRotation = Quaternion.Euler(rotation);

        return handItem;
    }

    private void FixItemScale(Transform itemTransform, Transform hand, Vector3 localScale)
    {
        Vector3 s = hand.lossyScale;
        itemTransform.localScale = new Vector3(
            localScale.x / s.x,
            localScale.y / s.y,
            localScale.z / s.z
        );
    }

    public void Unequip(InventorySlotUI.SlotSpecification spec)
    {
        switch (spec)
        {
            case InventorySlotUI.SlotSpecification.RightHand:
                if (twoHandEquipped)
                {
                    UnequipTwoHand();
                    twoHandEquipped = false;
                    if (!isRTCharacter)
                    {
                        playerStats.InvokeCombatChanged();
                        RTCharacterManager.Instance?.SyncFromMain();
                    }
                    return;
                }
                UnequipRightHand();
                break;

            case InventorySlotUI.SlotSpecification.LeftHand:
                if (twoHandEquipped)
                {
                    UnequipTwoHand();
                    twoHandEquipped = false;
                    if (!isRTCharacter)
                    {
                        playerStats.InvokeCombatChanged();
                        RTCharacterManager.Instance?.SyncFromMain();
                    }
                    return;
                }
                UnequipLeftHand();
                break;

            case SlotSpecification.HeadSlot:
                SetArmour(armourMeshes.headArmourMeshes, -1);
                equippedHeadArmourItem = null;
                break;

            case SlotSpecification.ChestSlot:
                SetArmour(armourMeshes.chestArmourMeshes, -1);
                equippedChestArmourItem = null;
                break;

            case SlotSpecification.LegsSlot:
                SetArmour(armourMeshes.legsArmourMeshes, -1);
                equippedLegArmourItem = null;
                break;

            case SlotSpecification.BeltSlot:
                SetArmour(armourMeshes.beltArmourMeshes, -1);
                equippedBeltItem = null;
                break;

            case SlotSpecification.HandsSlot:
                SetArmour(armourMeshes.glovesArmourMeshes, -1);
                equippedGlovesItem = null;
                break;

            case SlotSpecification.BootsSlot:
                SetArmour(armourMeshes.bootsArmourMeshes, -1);
                equippedBootsItem = null;
                break;

        }

        if (!isRTCharacter)
        {
            playerStats.InvokeCombatChanged();
            RTCharacterManager.Instance?.SyncFromMain();
        }
    }

    public void UnequipRightHand()
    {
        if (currentRightHandItem != null) Destroy(currentRightHandItem);

        currentRightHandItem = null;
        equippedRightItem = null;
        isRightHandDrawn = false;

        //CheckIfAnyWeaponLeft();
    }

    public void UnequipLeftHand()
    {
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);

        currentLeftHandItem = null;
        equippedLeftItem = null;
        isLeftHandDrawn = false;

        //CheckIfAnyWeaponLeft();
    }

    public void UnequipTwoHand()
    {
        if (currentRightHandItem != null) Destroy(currentRightHandItem);
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);
        currentRightHandItem = null;
        equippedRightItem = null;
        currentLeftHandItem = null;
        equippedLeftItem = null;
        isLeftHandDrawn = false;
        isRightHandDrawn = false;
    }

    public void DamageDurability(ItemInstance inst, float amount)
    {
        if (inst == null || !inst.HasDurability) return;

        inst.currentDurability -= amount;
        if (inst.currentDurability <= 0)
        {
            inst.currentDurability = 0;
            Debug.Log($"{inst.item.itemName} зламалась");
        }
    }

    public void EnableWeaponHitboxes()
    {
        EnableHitbox(currentRightHandItem);
        EnableHitbox(currentLeftHandItem);
    }

    public void DisableWeaponHitboxes()
    {
        DisableHitbox(currentRightHandItem);
        DisableHitbox(currentLeftHandItem);
    }

    void EnableHitbox(GameObject item)
    {
        if (item == null) return;

        var hitbox = item.GetComponentInChildren<WeaponHitbox>();
        if (hitbox != null)
            hitbox.EnableHitbox();
    }

    void DisableHitbox(GameObject item)
    {
        if (item == null) return;

        var hitbox = item.GetComponentInChildren<WeaponHitbox>();
        if (hitbox != null)
            hitbox.DisableHitbox();
    }
}
