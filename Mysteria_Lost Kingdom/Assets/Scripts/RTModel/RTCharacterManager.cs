using UnityEngine;

public class RTCharacterManager : MonoBehaviour
{
    public static RTCharacterManager Instance;

    [Header(" Û‰Ë ÒÚ‡‚ËÚË RT ÏÓ‰ÂÎ¸")]
    public Transform rtRoot;

    public GameObject clone;

    private EquipmentManager rtEquipment;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateClone();
    }

    void CreateClone()
    {
        Animator anim = clone.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.Play("Idle");
        }

        rtEquipment = clone.GetComponent<EquipmentManager>();
    }

    public void SyncFromMain()
    {
        if (rtEquipment == null) return;

        var main = EquipmentManager.Instance;

        // --- —»Õ’–ŒÕ≤«¿÷≤ﬂ ≈ ≤œ”¬¿ÕÕﬂ ---
        SyncEquipState(main);

        // --- —»Õ’–ŒÕ≤«¿÷≤ﬂ ¬≤«”¿À” (˘Ó Á‡‡Á Û ÛÍ‡ı) ---
        SyncDrawState(main);
    }


    private void SyncEquipState(EquipmentManager main)
    {
        // œ–¿¬¿ –” ¿
        if (main.equippedRightItem != null)
            rtEquipment.equippedRightItem = main.equippedRightItem;
        else
            rtEquipment.equippedRightItem = null;

        // À≤¬¿ –” ¿
        if (main.equippedLeftItem != null)
            rtEquipment.equippedLeftItem = main.equippedLeftItem;
        else
            rtEquipment.equippedLeftItem = null;

        // √ŒÀŒ¬¿
        rtEquipment.Unequip(InventorySlotUI.SlotSpecification.HeadSlot);
        if (main.equippedHeadArmourItem != null) rtEquipment.EquipItem(main.equippedHeadArmourItem, InventorySlotUI.SlotSpecification.HeadSlot);

        rtEquipment.twoHandEquipped = main.twoHandEquipped;
    }

    private void SyncDrawState(EquipmentManager main)
    {
        //rtEquipment.HideRightHand();
        //rtEquipment.HideLeftHand();

        //if (!main.isRightHandDrawn && !main.isLeftHandDrawn) return;

        //if (main.twoHandEquipped && main.isRightHandDrawn)
        //{
        //    rtEquipment.DrawTwoHand();
        //    return;
        //}

        //if (main.isRightHandDrawn && main.equippedRightItem != null) rtEquipment.DrawRightHand();

        //if (main.isLeftHandDrawn && main.equippedLeftItem != null) rtEquipment.DrawLeftHand();

        if (main.isRightHandDrawn)
        {
            if (main.equippedRightItem != null)
            {
                if (main.equippedRightItem.weaponHandType == WeaponHandType.TwoHand)
                {
                    rtEquipment.HideTwoHanded();
                    rtEquipment.DrawTwoHand();
                    return;
                }
                rtEquipment.HideRightHand();
                rtEquipment.DrawRightHand();
            }
        }
        else
        {
            if (main.equippedRightItem != null)
            {
                if (main.equippedRightItem.weaponHandType == WeaponHandType.TwoHand)
                {
                    rtEquipment.isRightHandDrawn = true;
                    rtEquipment.HideTwoHanded();
                    return;
                }
                rtEquipment.isRightHandDrawn = true;
                rtEquipment.HideRightHand();
            }
        }
        if (main.isLeftHandDrawn)
        {
            rtEquipment.HideLeftHand();
            if (main.equippedLeftItem != null && main.equippedLeftItem.weaponHandType == WeaponHandType.OneHand)
            {
                rtEquipment.DrawLeftHand();
            }
        }
        else
        {
            if (main.equippedLeftItem != null && main.equippedLeftItem.weaponHandType == WeaponHandType.OneHand)
            {
                rtEquipment.isLeftHandDrawn = true;
                rtEquipment.HideLeftHand();
            }
        }
    }


}
