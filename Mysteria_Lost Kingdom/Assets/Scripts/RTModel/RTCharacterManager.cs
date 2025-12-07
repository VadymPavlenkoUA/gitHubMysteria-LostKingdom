using UnityEngine;

public class RTCharacterManager : MonoBehaviour
{
    public static RTCharacterManager Instance;

    [Header("Êóäè ñòàâèòè RT ìîäåëü")]
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

        // --- ÑÈÍÕĞÎÍ²ÇÀÖ²ß ÅÊ²ÏÓÂÀÍÍß ---
        SyncEquipState(main);

        // --- ÑÈÍÕĞÎÍ²ÇÀÖ²ß Â²ÇÓÀËÓ (ùî çàğàç ó ğóêàõ) ---
        SyncDrawState(main);
    }


    private void SyncEquipState(EquipmentManager main)
    {
        // ÏĞÀÂÀ ĞÓÊÀ
        if (main.equippedRightItem != null)
            rtEquipment.equippedRightItem = main.equippedRightItem;
        else
            rtEquipment.equippedRightItem = null;

        // Ë²ÂÀ ĞÓÊÀ
        if (main.equippedLeftItem != null)
            rtEquipment.equippedLeftItem = main.equippedLeftItem;
        else
            rtEquipment.equippedLeftItem = null;

        // ÃÎËÎÂÀ
        rtEquipment.Unequip(InventorySlotUI.SlotSpecification.HeadSlot);

        if (main.equippedHeadArmourItem != null) rtEquipment.EquipItem(main.equippedHeadArmourItem, InventorySlotUI.SlotSpecification.HeadSlot);

        rtEquipment.twoHandEquipped = main.twoHandEquipped;
    }

    private void SyncDrawState(EquipmentManager main)
    {
        rtEquipment.HideRightHand();
        rtEquipment.HideLeftHand();

        if (!main.isRightHandDrawn && !main.isLeftHandDrawn) return;

        if (main.twoHandEquipped && main.isRightHandDrawn)
        {
            rtEquipment.DrawTwoHand();
            return;
        }

        if (main.isRightHandDrawn && main.equippedRightItem != null) rtEquipment.DrawRightHand();

        if (main.isLeftHandDrawn && main.equippedLeftItem != null) rtEquipment.DrawLeftHand();
    }


}
