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

        // œ–¿¬¿ –” ¿
        if (main.equippedRightItem != null)
            rtEquipment.EquipItem(main.equippedRightItem, InventorySlotUI.SlotType.Equipment, InventorySlotUI.SlotSpecification.RightHand);
        else
            rtEquipment.Unequip(InventorySlotUI.SlotSpecification.RightHand);

        // À≤¬¿ –” ¿
        if (main.equippedLeftItem != null)
            rtEquipment.EquipItem(main.equippedLeftItem, InventorySlotUI.SlotType.Equipment, InventorySlotUI.SlotSpecification.LeftHand);
        else
            rtEquipment.Unequip(InventorySlotUI.SlotSpecification.LeftHand);

        // √ŒÀŒ¬¿
        if (main.equippedHeadArmourItem != null)
            rtEquipment.EquipItem(main.equippedHeadArmourItem, InventorySlotUI.SlotType.Equipment, InventorySlotUI.SlotSpecification.HeadSlot);
        else
            rtEquipment.Unequip(InventorySlotUI.SlotSpecification.HeadSlot);
    }
}
