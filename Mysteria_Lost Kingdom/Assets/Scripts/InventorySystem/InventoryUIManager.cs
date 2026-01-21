using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Trade UI")]
    public GameObject tradePanel;
    public Transform tradePlayerSlotsParent;
    public Transform tradeTraderSlotsParent;

    private InventorySlotUI[] tradePlayerSlotUIs;
    private InventorySlotUI[] tradeTraderSlotUIs;

    [Header("Trade Panels")]
    public CanvasGroup tradePlayerPanel;
    public CanvasGroup tradeTraderPanel;

    [Header("Trade Center")]
    public Transform tradeCenterParent;
    public GameObject tradeSlotPrefab;
    private TradeSlotUI[] tradeCenterSlots;

    [Header("Trade Buttons")]
    public Button buyButton;
    public Button sellButton;
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(1, 1, 1, 0.5f);

    [Header("Tab UI")]
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
    }

    public void BuildTradeCenter()
    {
        foreach (Transform c in tradeCenterParent)
            Destroy(c.gameObject);

        tradeCenterSlots = new TradeSlotUI[TradeManager.Instance.tradeSlots.Length];

        for (int i = 0; i < tradeCenterSlots.Length; i++)
        {
            var obj = Instantiate(tradeSlotPrefab, tradeCenterParent);
            var ui = obj.GetComponent<TradeSlotUI>();
            ui.Init(i);
            tradeCenterSlots[i] = ui;
        }
    }

    public void RefreshTradeCenter()
    {
        if (tradeCenterSlots == null) return;
        foreach (var slot in tradeCenterSlots) slot.Refresh();
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

    public void OpenTradeView(Inventory playerInv, Inventory traderInv)
    {
        tradePanel.SetActive(true);

        tradePlayerSlotUIs = BuildInventoryView(
            playerInv,
            tradePlayerSlotsParent,
            InventorySlotUI.SlotType.TradePlayer
        );

        tradeTraderSlotUIs = BuildInventoryView(
            traderInv,
            tradeTraderSlotsParent,
            InventorySlotUI.SlotType.TradeTrader
        );

        BuildTradeCenter();

        //if (!MenuController.Instance.isGMOpen) MenuController.Instance.OpenGameMenu();
    }

    public void CloseTradeView()
    {
        tradePanel.SetActive(false);
        tradePlayerSlotUIs = null;
        tradeTraderSlotUIs = null;
    }

    private InventorySlotUI[] BuildInventoryView(
    Inventory inv,
    Transform parent,
    InventorySlotUI.SlotType slotType)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        var result = new InventorySlotUI[inv.slots.Count];

        for (int i = 0; i < inv.slots.Count; i++)
        {
            var obj = Instantiate(slotPrefab, parent);
            var slotUI = obj.GetComponent<InventorySlotUI>();

            slotUI.ownerInventory = inv;
            slotUI.slotType = slotType;
            slotUI.splitStackUI = splitStackUI;

            slotUI.SetSlot(inv.slots[i]);
            result[i] = slotUI;
        }

        return result;
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

    public void SetTradeMode(TradeMode mode)
    {
        switch (mode)
        {
            case TradeMode.Buy:
                SetPanelState(tradeTraderPanel, true);
                SetPanelState(tradePlayerPanel, false);
                break;

            case TradeMode.Sell:
                SetPanelState(tradePlayerPanel, true);
                SetPanelState(tradeTraderPanel, false);
                break;
        }
    }

    private void SetPanelState(CanvasGroup cg, bool active)
    {
        cg.alpha = active ? 1f : 0.2f;
        cg.interactable = active;
        cg.blocksRaycasts = active;
    }

    public void UpdateTradeButtons()
    {
        bool isBuy = TradeManager.Instance.CurrentMode == TradeMode.Buy;

        buyButton.image.color = isBuy ? activeColor : inactiveColor;
        sellButton.image.color = !isBuy ? activeColor : inactiveColor;
    }
}
