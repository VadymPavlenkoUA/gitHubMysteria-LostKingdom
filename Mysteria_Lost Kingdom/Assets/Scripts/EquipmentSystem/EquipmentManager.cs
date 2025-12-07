using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;
using static InventorySlotUI;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Слоти для екіпіювання")]
    public Transform rightHand;
    public Transform leftHand;
    public Transform head;
    //public Animator animator;

    internal GameObject currentRightHandItem;
    internal GameObject currentLeftHandItem;
    internal GameObject currentHeadArmourItem;

    internal Item equippedRightItem;
    internal Item equippedLeftItem;
    internal Item equippedHeadArmourItem;

    internal bool isLeftHandDrawn;
    internal bool isRightHandDrawn;
    internal bool isBowDrawn;

    public bool isRTCharacter = false;
    internal bool twoHandEquipped = false;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        if (!isRTCharacter)
        {
            Instance = this;
            inputActions = new PlayerInputActions();
        }
    }

    private void Update()
    {
        if (inputActions != null && !isRTCharacter)
        {
            if (inputActions.HotBar.RightHand.WasPressedThisFrame()) ConfirmEquipRighHand();
            if (inputActions.HotBar.LeftHand.WasPressedThisFrame()) ConfirmEquipLeftHand();
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

    public void EquipItem(Item item, InventorySlotUI.SlotSpecification slotSpecification)
    {
        if (item == null) return;
        if ((item.categories & ItemCategory.Weapon) != 0 || (item.categories & ItemCategory.Bow) != 0 || (item.categories & ItemCategory.Shield) != 0)
        {
            EquipWeapon(item, slotSpecification);
            if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
            return;
        }
        else if ((item.categories & ItemCategory.ArmourHead) != 0)
        {
            EquipArmour(item, slotSpecification);
            if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
            return;
        }
    }

    private void EquipWeapon(Item item, InventorySlotUI.SlotSpecification slotSpecification)
    {
        if (item.weaponHandType == WeaponHandType.TwoHand)
        {

            equippedRightItem = item;
            equippedLeftItem = item;

            twoHandEquipped = true;

            return;
        }

        if (item.weaponHandType == WeaponHandType.OneHand && slotSpecification == InventorySlotUI.SlotSpecification.RightHand)
        {
            equippedRightItem = item;

            return;
        }

        if (item.weaponHandType == WeaponHandType.OneHand && slotSpecification == InventorySlotUI.SlotSpecification.LeftHand)
        {
            equippedLeftItem = item;

            return;
        }

        Debug.LogWarning("Weapon has NO WeaponHandType assigned!");
    }

    private void EquipArmour(Item item, InventorySlotUI.SlotSpecification slotSpecification)
    {
        if (slotSpecification == InventorySlotUI.SlotSpecification.HeadSlot)
        {
            currentHeadArmourItem = Instantiate(item.itemPrefabEquip, head);
            FixItemScale(currentHeadArmourItem.transform, head, item.itemPrefabEquip.transform.localScale);
            currentHeadArmourItem.transform.localPosition = item.armourPosition;
            currentHeadArmourItem.transform.localRotation = Quaternion.Euler(item.armourRotation);

            equippedHeadArmourItem = item;

            //animator.SetBool("WeaponEquipped", true);
            //animator.SetInteger("WeaponType", 1);

            return;
        }
    }

    private void ConfirmEquipRighHand()
    {
        if (equippedRightItem == null) return;
        if (isRightHandDrawn && equippedRightItem.weaponHandType == WeaponHandType.TwoHand)
        {
            HideLeftHand();
            HideRightHand();
            return;
        }
        else if (isRightHandDrawn)
        {
            HideRightHand();
            return;
        }
        if (equippedRightItem.weaponHandType == WeaponHandType.TwoHand)
        {
            DrawTwoHand();
            return;
        }

        DrawRightHand();
    }

    private void ConfirmEquipLeftHand()
    {
        if (equippedLeftItem == null) return;
        if (isLeftHandDrawn && equippedLeftItem.weaponHandType == WeaponHandType.TwoHand)
        {
            HideLeftHand();
            HideRightHand();
            return;
        }
        else if (isLeftHandDrawn)
        {
            HideLeftHand();
            return;
        }
        if (equippedLeftItem.weaponHandType == WeaponHandType.TwoHand)
        {
            DrawTwoHand();
            return;
        }

        DrawLeftHand();
    }

    internal void DrawTwoHand()
    {
        HideRightHand();
        HideLeftHand();

        currentRightHandItem = EquipWeaponInHand(equippedRightItem, rightHand, equippedRightItem.rightHandPosition, equippedRightItem.rightHandRotation);

        isRightHandDrawn = true;
        isLeftHandDrawn = true;

        //animator.SetBool("WeaponEquipped", true);
        //animator.SetInteger("WeaponType", 3); 

        if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
    }

    internal void DrawRightHand()
    {
        currentRightHandItem = EquipWeaponInHand(equippedRightItem, rightHand, equippedRightItem.rightHandPosition, equippedRightItem.rightHandRotation);

        isRightHandDrawn = true;

        //animator.SetBool("WeaponEquipped", true);
        //animator.SetInteger("WeaponType", 1);

        if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
    }

    internal void DrawLeftHand()
    {
        currentLeftHandItem = EquipWeaponInHand(equippedLeftItem, leftHand, equippedLeftItem.leftHandPosition, equippedLeftItem.leftHandRotation);

        isLeftHandDrawn = true;

        //animator.SetBool("WeaponEquipped", true);
        //animator.SetInteger("WeaponType", 2);

        if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
    }

    internal void HideRightHand()
    {
        if (!isRightHandDrawn) return;
        isRightHandDrawn = false;
        if (currentRightHandItem != null) Destroy(currentRightHandItem);
        currentRightHandItem = null;

        if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
    }

    internal void HideLeftHand()
    {
        if (!isLeftHandDrawn) return;
        isLeftHandDrawn = false;
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);
        currentLeftHandItem = null;

        if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
    }

    internal GameObject EquipWeaponInHand(Item item, Transform hand, Vector3 position, Vector3 rotation)
    {
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
                    if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
                    return;
                }
                UnequipRightHand();
                break;

            case InventorySlotUI.SlotSpecification.LeftHand:
                if (twoHandEquipped)
                {
                    UnequipTwoHand();
                    twoHandEquipped = false;
                    if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
                    return;
                }
                UnequipLeftHand();
                break;

            case InventorySlotUI.SlotSpecification.HeadSlot:
                UnequipHead();
                break;
        }

        if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
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

    public void UnequipHead()
    {
        if (currentHeadArmourItem != null) Destroy(currentHeadArmourItem);

        currentHeadArmourItem = null;
        equippedHeadArmourItem = null;

        if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();

        Debug.Log("Unequip");
    }

    private void CheckIfAnyWeaponLeft()
    {
        //bool hasWeapon = equippedLeftItem != null || equippedRightItem != null;

        //animator.SetBool("WeaponEquipped", hasWeapon);

        //if (!hasWeapon)
        //    animator.SetInteger("WeaponType", 0);
    }
}
