using Unity.VisualScripting;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Слоти для екіпіювання")]
    public Transform rightHand;
    public Transform leftHand;
    public Transform head;
    //public Animator animator;

    private GameObject currentRightHandItem;
    private GameObject currentLeftHandItem;
    private GameObject currentHeadArmourItem;

    internal Item equippedRightItem;
    internal Item equippedLeftItem;
    internal Item equippedHeadArmourItem;

    public bool isRTCharacter = false;
    internal bool twoHandEquipped = false;

    private void Awake()
    {
        if(!isRTCharacter) Instance = this;
    }

    public void EquipItem(Item item, InventorySlotUI.SlotType slotType, InventorySlotUI.SlotSpecification slotSpecification)
    {
        if (item == null) return;
        if ((item.categories & ItemCategory.Weapon) != 0 || (item.categories & ItemCategory.Bow) != 0 || (item.categories & ItemCategory.Shield) != 0)
        {
            EquipWeapon(item, slotSpecification);
            if (!isRTCharacter)
                RTCharacterManager.Instance?.SyncFromMain();
            return;
        }
        else if ((item.categories & ItemCategory.ArmourHead) != 0)
        {
            EquipArmour(item, slotSpecification);
            if (!isRTCharacter)
                RTCharacterManager.Instance?.SyncFromMain();
            return;
        }
    }

    private void EquipWeapon(Item item, InventorySlotUI.SlotSpecification slotSpecification)
    {
        if (item.weaponHandType == WeaponHandType.TwoHand)
        {
            UnequipTwoHand();

            currentRightHandItem = Instantiate(item.itemPrefabEquip, rightHand);
            FixItemScale(currentRightHandItem.transform, rightHand, item.itemPrefabEquip.transform.localScale);
            currentRightHandItem.transform.localPosition = item.rightHandPosition;
            currentRightHandItem.transform.localRotation = Quaternion.Euler(item.rightHandRotation);

            equippedRightItem = item;
            equippedLeftItem = item;

            twoHandEquipped = true;

            //animator.SetBool("WeaponEquipped", true);
            //animator.SetInteger("WeaponType", 3); 

            return;
        }

        if (item.weaponHandType == WeaponHandType.OneHand && slotSpecification == InventorySlotUI.SlotSpecification.RightHand)
        {
            UnequipRightHand();

            currentRightHandItem = Instantiate(item.itemPrefabEquip, rightHand);
            FixItemScale(currentRightHandItem.transform, rightHand, item.itemPrefabEquip.transform.localScale);
            currentRightHandItem.transform.localPosition = item.rightHandPosition;
            currentRightHandItem.transform.localRotation = Quaternion.Euler(item.rightHandRotation);

            equippedRightItem = item;

            //animator.SetBool("WeaponEquipped", true);
            //animator.SetInteger("WeaponType", 1);

            return;
        }

        if (item.weaponHandType == WeaponHandType.OneHand && slotSpecification == InventorySlotUI.SlotSpecification.LeftHand)
        {
            UnequipLeftHand();

            currentLeftHandItem = Instantiate(item.itemPrefabEquip, leftHand);
            FixItemScale(currentLeftHandItem.transform, leftHand, item.itemPrefabEquip.transform.localScale);
            currentLeftHandItem.transform.localPosition = item.leftHandPosition;
            currentLeftHandItem.transform.localRotation = Quaternion.Euler(item.leftHandRotation);

            equippedLeftItem = item;

            //animator.SetBool("WeaponEquipped", true);
            //animator.SetInteger("WeaponType", 1);

            return;
        }

        Debug.LogWarning("Weapon has NO WeaponHandType assigned!");
    }

    private void EquipArmour(Item item, InventorySlotUI.SlotSpecification slotSpecification)
    {
        if (slotSpecification == InventorySlotUI.SlotSpecification.HeadSlot)
        {
            UnequipHead();

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

        //CheckIfAnyWeaponLeft();
    }

    public void UnequipLeftHand()
    {
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);

        currentLeftHandItem = null;
        equippedLeftItem = null;

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
    }

    public void UnequipHead()
    {
        if (currentHeadArmourItem != null) Destroy(currentHeadArmourItem);

        currentHeadArmourItem = null;
        equippedHeadArmourItem = null;
    }

    private void CheckIfAnyWeaponLeft()
    {
        //bool hasWeapon = equippedLeftItem != null || equippedRightItem != null;

        //animator.SetBool("WeaponEquipped", hasWeapon);

        //if (!hasWeapon)
        //    animator.SetInteger("WeaponType", 0);
    }
}
