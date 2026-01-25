using UnityEngine;
using UnityEngine.InputSystem;


public enum TradeMode
{
    None,
    Buy,
    Sell
}

public class TradeManager : MonoBehaviour
{
    public static TradeManager Instance;

    public TradeMode CurrentMode { get; private set; }

    public Inventory playerInventory;
    public Inventory traderInventory;
    public SplitStackUI splitStackUI;

    public GameObject arrowSell;
    public GameObject arrowBuy;

    public GameObject tradeUI;

    [Header("Trade Slots")]
    public int tradeSlotCount = 10;
    public TradeSlot[] tradeSlots;
    internal bool isTradeOpen;
    private PlayerInputActions inputActions;

    public NPC currentTrader;

    private void Awake()
    {
        Instance = this;
        InventoryUIManager.Instance.CloseTradeView();
        inputActions = MenuController.Instance.inputActions;
        tradeSlots = new TradeSlot[tradeSlotCount];
        for (int i = 0; i < tradeSlotCount; i++) tradeSlots[i] = new TradeSlot();
        SetBuyMode();
    }

    public void OpenTrade(NPC npcData)
    {
        isTradeOpen = true;
        currentTrader = npcData;
        traderInventory = npcData.traderInventory;

        SetBuyMode();

        InteractionBlocker.Block(InteractionBlockReason.Trade);
        inputActions.Combat.Disable();
        inputActions.Player.Disable();
        inputActions.HotBar.Disable();
        MenuController.Instance.cinemachineInput.enabled = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        InventoryUIManager.Instance.OpenTradeView(playerInventory, traderInventory);
    }

    public void SetBuyMode()
    {
        CancelTrade();
        CurrentMode = TradeMode.Buy;
        ClearTradeSlots();
        InventoryUIManager.Instance.SetTradeMode(CurrentMode);
        InventoryUIManager.Instance.UpdateTradeButtons();
        arrowBuy.SetActive(true);
        arrowSell.SetActive(false);
    }

    public void SetSellMode()
    {
        CancelTrade();
        CurrentMode = TradeMode.Sell;
        ClearTradeSlots();
        InventoryUIManager.Instance.SetTradeMode(CurrentMode);
        InventoryUIManager.Instance.UpdateTradeButtons();
        arrowSell.SetActive(true);
        arrowBuy.SetActive(false);
    }

    public void CloseTrade()
    {
        CancelTrade();
        isTradeOpen = false;
        CurrentMode = TradeMode.None;
        InventoryUIManager.Instance.CloseTradeView();
        ItemDescriptionUI.Instance.ClearDescription(true);
        traderInventory = null;
        currentTrader = null;

        InteractionBlocker.Unblock(InteractionBlockReason.Trade);
        inputActions.Player.Enable();
        inputActions.HotBar.Enable();
        inputActions.Combat.Enable();
        MenuController.Instance.cinemachineInput.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ClearTradeSlots()
    {
        foreach (var slot in tradeSlots) slot.Clear();
        InventoryUIManager.Instance.RefreshTradeCenter();
    }

    public void OnBuyClicked()
    {
        SetBuyMode();
    }

    public void OnSellClicked()
    {
        SetSellMode();
    }

    public void TryAddToTrade(InventorySlotUI from, int index)
    {
        var tradeSlot = tradeSlots[index];
        var invSlot = from.slot;

        if (invSlot.IsEmpty)
            return;

        // ===== UNIQUE =====
        if (invSlot.IsUnique)
        {
            tradeSlot.Set(invSlot.instance);

            invSlot.Clear();
            from.SetSlot(invSlot);

            InventoryUIManager.Instance.RefreshTradeUI();
            return;
        }

        // ===== STACKABLE =====
        bool ctrl = Keyboard.current.leftCtrlKey.isPressed;

        if (ctrl && invSlot.count > 1)
        {
            splitStackUI.Show(
                invSlot.count,
                (chosen) =>
                {
                    if (chosen <= 0) return;

                    int moved = tradeSlot.Add(invSlot.item, chosen);
                    invSlot.instance.count -= moved;

                    if (invSlot.count <= 0)
                        invSlot.Clear();

                    from.SetSlot(invSlot);
                    InventoryUIManager.Instance.RefreshTradeUI();
                });
        }
        else
        {
            int moved = tradeSlot.Add(invSlot.item, invSlot.count);
            invSlot.instance.count -= moved;

            if (invSlot.count <= 0)
                invSlot.Clear();

            from.SetSlot(invSlot);
            InventoryUIManager.Instance.RefreshTradeUI();
        }
    }

    public void TryReturnFromTrade(int tradeIndex, InventorySlotUI target)
    {
        var tradeSlot = tradeSlots[tradeIndex];

        if (tradeSlot.IsEmpty)
            return;

        // ===== UNIQUE =====
        if (tradeSlot.IsUnique)
        {
            if (!target.slot.IsEmpty) return;

            target.slot.SetItem(tradeSlot.itemInstance);
            target.SetSlot(target.slot);
            tradeSlot.Clear();
        }
        else
        {
            // ===== STACKABLE =====
            int remaining = target.slot.AddItem(tradeSlot.item, tradeSlot.amount);
            int added = tradeSlot.amount - remaining;

            if (added <= 0) return;

            tradeSlot.amount = remaining;

            if (tradeSlot.amount <= 0)
                tradeSlot.Clear();

            target.SetSlot(target.slot);
        }

        InventoryUIManager.Instance.RefreshTradeUI();
        InventoryUIManager.Instance.RefreshUI();
    }

    public void CancelTrade()
    {
        Inventory targetInventory = CurrentMode == TradeMode.Buy ? traderInventory : playerInventory;

        foreach (var slot in tradeSlots)
        {
            if (slot.IsEmpty) continue;

            // ===== UNIQUE =====
            if (slot.IsUnique && slot.itemInstance != null)
            {
                bool added = targetInventory.AddInstance(slot.itemInstance);
                if (!added)
                {
                    Debug.LogWarning($"Не вдалося повернути {slot.itemInstance.item.name} у інвентар!");
                }
            }
            // ===== STACKABLE =====
            else if (!slot.IsEmpty && slot.item != null)
            {
                bool added = targetInventory.AddItem(slot.item, slot.amount);
                if (!added)
                {
                    Debug.LogWarning($"Не вдалося повернути {slot.amount}x {slot.item.name} у інвентар!");
                }
            }

            slot.Clear();
        }

        InventoryUIManager.Instance.RefreshTradeUI();
    }

    public int GetItemTradePrice(Item item)
    {
        if (currentTrader == null) return item.basePrice;

        return TradeSlot.GetUnitPrice(item, currentTrader.traderData, CurrentMode);
    }

    public int CalculateTradeTotalPrice()
    {
        if (currentTrader == null) return 0;

        int total = 0;
        foreach (var slot in tradeSlots)
        {
            if (slot.IsEmpty) continue;
            total += slot.GetTotalPrice(currentTrader.traderData, CurrentMode);
        }
        return total;
    }

    public void ConfirmTrade()
    {
        int totalPrice = CalculateTradeTotalPrice();

        if (CurrentMode == TradeMode.Buy)
        {
            TryBuy(totalPrice);
        }
        else if (CurrentMode == TradeMode.Sell)
        {
            TrySell(totalPrice);
        }
        InventoryUIManager.Instance.RefreshUI();
    }

    private void TryBuy(int totalPrice)
    {
        if (!playerInventory.playerStats.TrySpendGold(totalPrice))
        {
            Debug.Log("Недостатньо золота");
            return;
        }

        if (!currentTrader.TrySpendGold(-totalPrice))
        {
            Debug.Log("У торговця щось пішло не так");
            playerInventory.playerStats.AddGold(totalPrice);
            return;
        }

        foreach (var slot in tradeSlots)
        {
            if (slot.IsEmpty) continue;

            if (slot.IsUnique)
                playerInventory.AddInstance(slot.itemInstance);
            else
                playerInventory.AddItem(slot.item, slot.amount);

            slot.Clear();
        }

        InventoryUIManager.Instance.RefreshTradeUI();
    }

    private void TrySell(int totalPrice)
    {
        if (!currentTrader.TrySpendGold(totalPrice))
        {
            Debug.Log("У торговця недостатньо золота");
            return;
        }

        playerInventory.playerStats.AddGold(totalPrice);

        foreach (var slot in tradeSlots)
        {
            if (slot.IsEmpty) continue;

            if (slot.IsUnique)
                traderInventory.AddInstance(slot.itemInstance);
            else
                traderInventory.AddItem(slot.item, slot.amount);

            slot.Clear();
        }

        InventoryUIManager.Instance.RefreshTradeUI();
    }




}
