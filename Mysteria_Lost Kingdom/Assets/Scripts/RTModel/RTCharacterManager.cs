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
            anim.Rebind();        
            anim.Update(0f);

            anim.SetFloat("Speed", 0f);
            anim.SetBool("IsAttacking", false);
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


    // ============================================================
    //  —»Õ’–ŒÕ≤«¿÷≤ﬂ ≈ ≤œ”¬¿ÕÕﬂ («¡–Œﬂ + ¬—ﬂ ¡–ŒÕﬂ)
    // ============================================================
    private void SyncEquipState(EquipmentManager main)
    {
        // --- —»Õ’–Œ Ã≈ÿ≤¬ ¡–ŒÕ≤ ---
        SyncArmour(main);

        // --- —»Õ’–Œ œ–≈ƒÃ≈“≤¬ ” —ÀŒ“¿’ ---
        rtEquipment.equippedRightItem = main.equippedRightItem;
        rtEquipment.equippedLeftItem = main.equippedLeftItem;

        rtEquipment.equippedHeadArmourItem = main.equippedHeadArmourItem;
        rtEquipment.equippedChestArmourItem = main.equippedChestArmourItem;
        rtEquipment.equippedLegArmourItem = main.equippedLegArmourItem;
        rtEquipment.equippedBootsItem = main.equippedBootsItem;
        rtEquipment.equippedGlovesItem = main.equippedGlovesItem;
        rtEquipment.equippedBeltItem = main.equippedBeltItem;

        rtEquipment.twoHandEquipped = main.twoHandEquipped;
    }

    private void SyncArmour(EquipmentManager main)
    {
        // √ŒÀŒ¬¿
        rtEquipment.SetArmour(rtEquipment.armourMeshes.headArmourMeshes,
            main.equippedHeadArmourItem != null ? main.equippedHeadArmourItem.item.meshIndex : -1);

        // √–”ƒ»
        rtEquipment.SetArmour(rtEquipment.armourMeshes.chestArmourMeshes,
            main.equippedChestArmourItem != null ? main.equippedChestArmourItem.item.meshIndex : -1);

        // ÕŒ√»
        rtEquipment.SetArmour(rtEquipment.armourMeshes.legsArmourMeshes,
            main.equippedLegArmourItem != null ? main.equippedLegArmourItem.item.meshIndex : -1);

        // œ¿—Œ 
        rtEquipment.SetArmour(rtEquipment.armourMeshes.beltArmourMeshes,
            main.equippedBeltItem != null ? main.equippedBeltItem.item.meshIndex : -1);

        // –” ¿¬»◊ »
        rtEquipment.SetArmour(rtEquipment.armourMeshes.glovesArmourMeshes,
            main.equippedGlovesItem != null ? main.equippedGlovesItem.item.meshIndex : -1);

        // ◊Œ¡≤“»
        rtEquipment.SetArmour(rtEquipment.armourMeshes.bootsArmourMeshes,
            main.equippedBootsItem != null ? main.equippedBootsItem.item.meshIndex : -1);
    }

    // ============================================================
    //  —»Õ’–ŒÕ≤«¿÷≤ﬂ "¬»“ﬂ√Õ”“Œ / —’Œ¬¿ÕŒ"
    // ============================================================
    private void SyncDrawState(EquipmentManager main)
    {
        var eq = EquipmentManager.Instance;
        bool isRightHandDrawn = eq.isRightHandDrawn;
        bool isLeftHandDrawn = eq.isLeftHandDrawn;
        
        if (eq.equippedRightItem == null && eq.equippedLeftItem == null)
        {
            if (rtEquipment.currentRightHandItem != null) Destroy(rtEquipment.currentRightHandItem);
            if (rtEquipment.currentLeftHandItem != null) Destroy(rtEquipment.currentLeftHandItem);
        }

        rtEquipment.HideRightHand();
        rtEquipment.HideLeftHand();
        rtEquipment.HideTwoHanded();

        if (isRightHandDrawn)
        {
            if (eq.equippedRightItem != null)
            {
                if (eq.equippedRightItem.item.weaponHandType == WeaponHandType.TwoHand)
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
            if (eq.equippedRightItem != null)
            {
                if (eq.equippedRightItem.item.weaponHandType == WeaponHandType.TwoHand)
                {
                    rtEquipment.isRightHandDrawn = true;
                    rtEquipment.isLeftHandDrawn = true;
                    rtEquipment.HideTwoHanded();
                    return;
                }
                rtEquipment.isRightHandDrawn = true;
                rtEquipment.HideRightHand();
            }
        }
        if (isLeftHandDrawn)
        {
            rtEquipment.HideLeftHand();
            if (eq.equippedLeftItem != null && eq.equippedLeftItem.item.weaponHandType == WeaponHandType.OneHand)
            {
                rtEquipment.DrawLeftHand();
            }
        }
        else
        {
            if (eq.equippedLeftItem != null && eq.equippedLeftItem.item.weaponHandType == WeaponHandType.OneHand)
            {
                rtEquipment.isLeftHandDrawn = true;
                rtEquipment.HideLeftHand();
            }
        }
    }

    public void ApplyCustomizationFromMain(CharacterCustomizer mainCustomizer)
    {
        if (clone == null || mainCustomizer == null) return;

        CharacterCustomizationData data = mainCustomizer.GetCustomizationData();

        var rtCustomizer = clone.GetComponent<CharacterCustomizer>();
        if (rtCustomizer != null)
        {
            rtCustomizer.ApplyCustomization(data);
        }
    }
}
