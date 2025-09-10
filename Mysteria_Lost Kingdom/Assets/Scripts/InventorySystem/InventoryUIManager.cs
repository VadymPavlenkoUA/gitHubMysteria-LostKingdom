using System;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public Inventory inventory;
    public GameObject slotPrefab;
    public Transform slotsParent;

    private InventorySlotUI[] slotUIs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slotUIs = new InventorySlotUI[inventory.slots.Count];

        for (int i = 0; i < inventory.slots.Count; i++)
        {
            GameObject obj = Instantiate(slotPrefab, slotsParent);
            slotUIs[i] = obj.GetComponent<InventorySlotUI>();
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        for (int i = 0; i < inventory.slots.Count; i++)
        {
            slotUIs[i].SetSlot(inventory.slots[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
