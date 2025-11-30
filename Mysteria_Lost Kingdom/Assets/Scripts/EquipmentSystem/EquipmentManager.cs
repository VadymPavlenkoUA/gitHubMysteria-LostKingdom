using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Слоти для екіпіювання")]
    public Transform rightHand;
    public Transform leftHand;
    //public Animator animator;

    private GameObject currentRightHandItem;
    private GameObject currentLeftHandItem;

    private Item equippedRightItem;
    private Item equippedLeftItem;

    private void Awake()
    {
        Instance = this;
    }

    public void EquipItem(Item item, InventorySlotUI.SlotType slotType, InventorySlotUI.SlotSpecification slotSpecification)
    {
        if (item == null) return;
        if ((item.categories & ItemCategory.Weapon) != 0 || (item.categories & ItemCategory.Bow) != 0 || (item.categories & ItemCategory.Shield) != 0)
        {
            EquipWeapon(item, slotSpecification);
            return;
        } 
    }

    private void EquipWeapon(Item item, InventorySlotUI.SlotSpecification slotSpecification)
    {
        if (item.weaponHandType == WeaponHandType.TwoHand)
        {
            UnequipRightHand();
            UnequipLeftHand();

            currentRightHandItem = Instantiate(item.itemPrefabEquip, rightHand);
            currentRightHandItem.transform.localScale = Vector3.one;
            currentRightHandItem.transform.localPosition = item.rightHandPosition;
            currentRightHandItem.transform.localRotation = Quaternion.Euler(item.rightHandRotation);

            equippedRightItem = item;
            equippedLeftItem = item; 

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

    private void FixItemScale(Transform itemTransform, Transform hand, Vector3 localScale)
    {
        Vector3 s = hand.lossyScale;
        itemTransform.localScale = new Vector3(
            localScale.x / s.x,
            localScale.y / s.y,
            localScale.z / s.z
        );
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

    private void CheckIfAnyWeaponLeft()
    {
        //bool hasWeapon = equippedLeftItem != null || equippedRightItem != null;

        //animator.SetBool("WeaponEquipped", hasWeapon);

        //if (!hasWeapon)
        //    animator.SetInteger("WeaponType", 0);
    }
}
