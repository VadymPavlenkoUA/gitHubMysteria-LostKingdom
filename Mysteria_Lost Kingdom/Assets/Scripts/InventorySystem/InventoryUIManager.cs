using System;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    public Inventory inventory;
    public GameObject slotPrefab;
    public Transform slotsParent;
    public SplitStackUI splitStackUI;

    public InventorySlotUI[] equipmentSlots;
    private InventorySlotUI[] slotUIs;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        slotUIs = new InventorySlotUI[inventory.slots.Count];

        for (int i = 0; i < inventory.slots.Count; i++)
        {
            GameObject obj = Instantiate(slotPrefab, slotsParent);
            slotUIs[i] = obj.GetComponent<InventorySlotUI>();
        }

        foreach (var slot in slotUIs)
        {
            slot.splitStackUI = splitStackUI;
        }

        foreach (var eqSlot in equipmentSlots)
        {
            if (eqSlot.slot == null) eqSlot.slot = new InventorySlot(); 
        }

        RefreshUI();
    }

    public InventorySlotUI FindFirstEmptySlot()
    {
        foreach (var slotUI in slotUIs)
        {
            if (slotUI.slot.IsEmpty) return slotUI;
        }
        return null;
    }

    public void RefreshUI()
    {
        for (int i = 0; i < inventory.slots.Count; i++)
        {
            slotUIs[i].SetSlot(inventory.slots[i]);
        }

        foreach (var eqSlot in equipmentSlots)
        {
            eqSlot.SetSlot(eqSlot.slot);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
