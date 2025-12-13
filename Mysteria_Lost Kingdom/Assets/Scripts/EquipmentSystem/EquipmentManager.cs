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

    internal Item equippedRightItem;
    internal Item equippedLeftItem;
    internal Item equippedHeadArmourItem;
    internal Item equippedChestArmourItem;
    internal Item equippedLegArmourItem;
    internal Item equippedBootsItem;
    internal Item equippedGlovesItem;
    internal Item equippedBeltItem;

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
            inputActions = new PlayerInputActions();
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

    public void EquipItem(Item item, InventorySlotUI.SlotSpecification slotSpecification)
    {
        if (item == null) return;
        if ((item.categories & ItemCategory.Weapon) != 0 || (item.categories & ItemCategory.Bow) != 0 || (item.categories & ItemCategory.Shield) != 0)
        {
            EquipWeapon(item, slotSpecification);
            if (!isRTCharacter)
            {
                playerStats.InvokeCombatChanged();
                RTCharacterManager.Instance?.SyncFromMain();
            }
            return;
        }
        else if (item.categories == ItemCategory.ArmourHead || item.categories == ItemCategory.ArmourChest || item.categories == ItemCategory.ArmourLegs || 
            item.categories == ItemCategory.ArmourBelt || item.categories == ItemCategory.ArmourGloves || item.categories == ItemCategory.ArmourBoots)
        {
            EquipArmour(item);
            if (!isRTCharacter)
            {
                playerStats.InvokeCombatChanged();
                RTCharacterManager.Instance?.SyncFromMain();
            }
            return;
        }
    }

    internal void SetArmour(GameObject[] meshArray, int index)
    {
        for (int i = 0; i < meshArray.Length; i++)
            meshArray[i].SetActive(i == index);
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

    private void EquipArmour(Item item)
    {
        switch (item.categories)
        {
            case ItemCategory.ArmourHead:
                SetArmour(armourMeshes.headArmourMeshes, item.meshIndex);
                equippedHeadArmourItem = item;
                break;

            case ItemCategory.ArmourChest:
                SetArmour(armourMeshes.chestArmourMeshes, item.meshIndex);
                equippedChestArmourItem = item;
                break;

            case ItemCategory.ArmourLegs:
                SetArmour(armourMeshes.legsArmourMeshes, item.meshIndex);
                equippedLegArmourItem = item;
                break;

            case ItemCategory.ArmourBelt:
                SetArmour(armourMeshes.beltArmourMeshes, item.meshIndex);
                equippedBeltItem = item;
                break;

            case ItemCategory.ArmourGloves:
                SetArmour(armourMeshes.glovesArmourMeshes, item.meshIndex);
                equippedGlovesItem = item;
                break;

            case ItemCategory.ArmourBoots:
                SetArmour(armourMeshes.bootsArmourMeshes, item.meshIndex);
                equippedBootsItem = item;
                break;
        }
    }


    private void ConfirmEquipRighHand()
    {
        if (isRightHandDrawn && equippedRightItem.weaponHandType == WeaponHandType.TwoHand)
        {
            HideTwoHanded();
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
        if (isLeftHandDrawn && equippedLeftItem.weaponHandType == WeaponHandType.TwoHand)
        {
            HideTwoHanded();
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
        if (currentRightHandItem != null) Destroy(currentRightHandItem);
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);
        currentRightHandItem = EquipWeaponInHand(equippedRightItem, rightHand, equippedRightItem.rightHandPosition, equippedRightItem.rightHandRotation);

        isRightHandDrawn = true;
        isLeftHandDrawn = true;

        //animator.SetBool("WeaponEquipped", true);
        //animator.SetInteger("WeaponType", 3); 

        if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
    }

    internal void DrawRightHand()
    {
        if (currentRightHandItem != null) Destroy(currentRightHandItem);
        currentRightHandItem = EquipWeaponInHand(equippedRightItem, rightHand, equippedRightItem.rightHandPosition, equippedRightItem.rightHandRotation);

        isRightHandDrawn = true;

        //animator.SetBool("WeaponEquipped", true);
        //animator.SetInteger("WeaponType", 1);

        if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
    }

    internal void DrawLeftHand()
    {
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);
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
        if (equippedRightItem != null)
        {
            if (equippedRightItem.categories == ItemCategory.Shield) currentRightHandItem = EquipWeaponInactive(equippedRightItem, backSocket, equippedRightItem.inactivePosition, equippedRightItem.inactiveRotation);
            if (equippedRightItem.categories == ItemCategory.Weapon && equippedRightItem.weaponHandType == WeaponHandType.OneHand) currentRightHandItem = EquipWeaponInactive(equippedRightItem, rightBeltSocket, equippedRightItem.inactivePosition, equippedRightItem.inactiveRotation);
        }

        if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
    }

    internal void HideLeftHand()
    {
        if (!isLeftHandDrawn) return;
        isLeftHandDrawn = false;
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);
        if (equippedLeftItem != null)
        {
            if (equippedLeftItem.categories == ItemCategory.Shield) currentLeftHandItem = EquipWeaponInactive(equippedLeftItem, backSocket, equippedLeftItem.inactivePosition, equippedLeftItem.inactiveRotation);
            if (equippedLeftItem.categories == ItemCategory.Weapon && equippedLeftItem.weaponHandType == WeaponHandType.OneHand) currentLeftHandItem = EquipWeaponInactive(equippedLeftItem, leftBeltSocket, equippedLeftItem.inactiveAltPosition, equippedLeftItem.inactiveAltRotation);
        }

        if (!isRTCharacter) RTCharacterManager.Instance?.SyncFromMain();
    }

    internal void HideTwoHanded()
    {
        if (!isRightHandDrawn && !isLeftHandDrawn) return;
        isRightHandDrawn = false;
        isLeftHandDrawn = false;
        if (currentRightHandItem != null) Destroy(currentRightHandItem);
        if (currentLeftHandItem != null) Destroy(currentLeftHandItem);
        if (equippedRightItem != null) currentRightHandItem = EquipWeaponInactive(equippedRightItem, backSocket, equippedRightItem.inactivePosition, equippedRightItem.inactiveRotation);

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

    internal GameObject EquipWeaponInactive(Item item, Transform hand, Vector3 position, Vector3 rotation)
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

    public void DamageDurability(Item item, float amount)
    {
        if (item == null) return;

        item.currentDurability -= amount;
        if (item.currentDurability < 0) item.currentDurability = 0;
    }
    public float GetDurabilityPercent(Item item)
    {
        if (item == null) return 1f;
        return item.currentDurability / item.maxDurability;
    }
}
