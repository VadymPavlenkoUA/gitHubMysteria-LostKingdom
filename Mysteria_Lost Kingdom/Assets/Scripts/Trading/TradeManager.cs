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

    public GameObject tradeUI;

    [Header("Trade Slots")]
    public int tradeSlotCount = 10;
    public TradeSlot[] tradeSlots;
    internal bool isTradeOpen;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        Instance = this;
        InventoryUIManager.Instance.CloseTradeView();
        inputActions = MenuController.Instance.inputActions;
        tradeSlots = new TradeSlot[tradeSlotCount];
        for (int i = 0; i < tradeSlotCount; i++) tradeSlots[i] = new TradeSlot();
        SetBuyMode();
    }

    public void OpenTrade(Inventory traderInv)
    {
        isTradeOpen = true;
        traderInventory = traderInv;
        CurrentMode = TradeMode.Sell;

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
    }

    public void SetSellMode()
    {
        CancelTrade();
        CurrentMode = TradeMode.Sell;
        ClearTradeSlots();
        InventoryUIManager.Instance.SetTradeMode(CurrentMode);
        InventoryUIManager.Instance.UpdateTradeButtons();
    }

    public void CloseTrade()
    {
        CancelTrade();
        isTradeOpen = false;
        CurrentMode = TradeMode.None;
        InventoryUIManager.Instance.CloseTradeView();
        traderInventory = null;

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

            InventoryUIManager.Instance.RefreshTradeCenter();
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
        foreach (var slot in tradeSlots)
        {
            if (slot.IsEmpty)
                continue;

            // ===== UNIQUE =====
            if (slot.IsUnique && slot.itemInstance != null)
            {
                bool added = playerInventory.AddInstance(slot.itemInstance);
                if (!added)
                {
                    Debug.LogWarning($"Не вдалося повернути {slot.itemInstance.item.name} у інвентар!");
                }
            }

            // ===== STACKABLE =====
            else if (!slot.IsEmpty && slot.item != null)
            {
                bool added = playerInventory.AddItem(slot.item, slot.amount);
                if (!added)
                {
                    Debug.LogWarning($"Не вдалося повернути {slot.amount}x {slot.item.name} у інвентар!");
                }
            }

            slot.Clear();
        }

        InventoryUIManager.Instance.RefreshTradeUI();
    }



}
