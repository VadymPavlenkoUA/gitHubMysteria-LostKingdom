using JetBrains.Annotations;
using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    public static HotbarUI Instance;

    [Header("Weapon Slots")]
    public HotbarSlot rightHandSlot;
    public HotbarSlot leftHandSlot;
    public HotbarSlot rangeSlot;
    public HotbarSlot throwableSlot;

    [Header("Skill Slots")]
    public HotbarSlot[] skillSlots;

    [Header("Pouch Slots")]
    public HotbarSlot[] pouchSlots;

    void Start()
    {
        ClearAllSlots();
        RefreshHotbar();
    }

    private void Awake()
    {
        Instance = this;
    }

    public void ClearAllSlots()
    {
        leftHandSlot.SetItem(null);
        rightHandSlot.SetItem(null);
        rangeSlot.SetItem(null);
        throwableSlot.SetItem(null);

        foreach (var s in skillSlots) s.SetItem(null);

        foreach (var p in pouchSlots) p.SetItem(null);
    }

    public void RefreshHotbar()
    {
        rightHandSlot.SetItem(EquipmentManager.Instance.equippedRightItem);
        leftHandSlot.SetItem(EquipmentManager.Instance.equippedLeftItem);
    }
}
