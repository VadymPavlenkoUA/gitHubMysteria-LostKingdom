using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    public Inventory inventory;
    public GameObject slotPrefab;
    public Transform slotsParent;
    public SplitStackUI splitStackUI;
    public TextMeshProUGUI weightText;

    public InventorySlotUI[] equipmentSlots;
    private InventorySlotUI[] slotUIs;

    [Header("Chest UI")]
    public GameObject chestPanel;
    public TextMeshProUGUI chestPanelName;
    public Transform chestSlotsParent;
    public GameObject chestSlotPrefab;
    private ChestVisual currentChestVisual;

    public TabSwitcher tabSwitcher;
    public int inventoryTabIndex;

    private Inventory chestInventory;
    private InventorySlotUI[] chestSlotUIs;

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
            var slotUI = obj.GetComponent<InventorySlotUI>();

            slotUI.ownerInventory = inventory; 
            slotUI.slotType = InventorySlotUI.SlotType.Inventory;
            slotUI.splitStackUI = splitStackUI;

            slotUIs[i] = slotUI;
        }

        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            equipmentSlots[i].slot = inventory.equipSlots[i];
            equipmentSlots[i].ownerInventory = inventory;
            equipmentSlots[i].slotType = InventorySlotUI.SlotType.Equipment;
        }

        foreach (var slot in slotUIs)
        {
            slot.splitStackUI = splitStackUI;
        }

        inventory.playerStats.CalculateDerivedStats();
        inventory.playerStats.StatsChanged += RefreshWeightEquipText;
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

        RefreshWeightEquipText();

        //if (ItemDescriptionUI.Instance != null) ItemDescriptionUI.Instance.ClearDescription();
    }

    public void NotifyInventoryChanged()
    {
        if (inventory != null) inventory.NotifyInventoryChanged();
    }

    private void RefreshWeightEquipText()
    {
        weightText.text = $"{inventory.CurrentWeight.ToString("0.0", CultureInfo.InvariantCulture)} / " +
            $"{inventory.playerStats.maxWeight.ToString("0.0", CultureInfo.InvariantCulture)}";
    }

    public void OpenChest(Inventory chestInv, ChestVisual chestVisual, string chestName)
    {
        chestInventory = chestInv;
        currentChestVisual = chestVisual;

        currentChestVisual?.Open();

        chestPanelName.text = chestName;
        chestPanel.SetActive(true);
        BuildChestSlots();
        RefreshChestUI();

        if (!MenuController.Instance.isGMOpen) MenuController.Instance.OpenGameMenu();
        if (tabSwitcher != null) tabSwitcher.OpenTab(inventoryTabIndex);
    }

    private void BuildChestSlots()
    {
        // очищаємо старі
        foreach (Transform child in chestSlotsParent)
            Destroy(child.gameObject);

        chestSlotUIs = new InventorySlotUI[chestInventory.slots.Count];

        for (int i = 0; i < chestInventory.slots.Count; i++)
        {
            GameObject obj = Instantiate(slotPrefab, chestSlotsParent);
            var slotUI = obj.GetComponent<InventorySlotUI>();

            slotUI.ownerInventory = chestInventory;
            slotUI.slotType = InventorySlotUI.SlotType.Chest;
            slotUI.splitStackUI = splitStackUI;

            chestSlotUIs[i] = slotUI;
        }

    }

    private void RefreshChestUI()
    {
        for (int i = 0; i < chestInventory.slots.Count; i++)
        {
            chestSlotUIs[i].SetSlot(chestInventory.slots[i]);
        }
    }

    public void CloseChest()
    {
        chestPanel.SetActive(false);
        chestInventory = null;

        currentChestVisual?.Close();
        currentChestVisual = null;
    }

    public bool IsChestOpen(Inventory chest)
    {
        if (chestPanel.activeSelf && chestInventory == chest) return true;
        return false;
    }
}
