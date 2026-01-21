using UnityEngine;


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

    public GameObject tradeUI;

    [Header("Trade Slots")]
    public int tradeSlotCount = 10;
    public TradeSlot[] tradeSlots;

    private void Awake()
    {
        Instance = this;
        tradeSlots = new TradeSlot[tradeSlotCount];
        for (int i = 0; i < tradeSlotCount; i++) tradeSlots[i] = new TradeSlot();
        SetBuyMode();
    }

    public void OpenTrade(Inventory traderInv)
    {
        traderInventory = traderInv;
        CurrentMode = TradeMode.Sell;

        InventoryUIManager.Instance.OpenTradeView(playerInventory, traderInventory);
    }

    public void SetBuyMode()
    {
        CurrentMode = TradeMode.Buy;
        ClearTradeSlots();
        InventoryUIManager.Instance.SetTradeMode(CurrentMode);
        InventoryUIManager.Instance.UpdateTradeButtons();
    }

    public void SetSellMode()
    {
        CurrentMode = TradeMode.Sell;
        ClearTradeSlots();
        InventoryUIManager.Instance.SetTradeMode(CurrentMode);
        InventoryUIManager.Instance.UpdateTradeButtons();
    }

    public void CloseTrade()
    {
        CurrentMode = TradeMode.None;
        tradeUI.SetActive(false);
        traderInventory = null;
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
}
