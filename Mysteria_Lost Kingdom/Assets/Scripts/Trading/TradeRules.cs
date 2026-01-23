public static class TradeRules
{
    public static bool CanDrag(InventorySlotUI slot)
    {
        var tm = TradeManager.Instance;

        if (!tm.isTradeOpen || tm.CurrentMode == TradeMode.None)
            return true;

        return tm.CurrentMode switch
        {
            TradeMode.Buy => slot.ownerInventory == tm.traderInventory,
            TradeMode.Sell => slot.ownerInventory == tm.playerInventory,
            _ => false
        };
    }

    public static bool CanDropIntoTrade(InventorySlotUI slot)
    {
        var tm = TradeManager.Instance;

        if (!tm.isTradeOpen) return false;

        return tm.CurrentMode switch
        {
            TradeMode.Buy => slot.ownerInventory == tm.traderInventory,
            TradeMode.Sell => slot.ownerInventory == tm.playerInventory,
            _ => false
        };
    }

    public static bool CanDropToWorld()
    {
        if (!TradeManager.Instance.isTradeOpen)
            return true;

        return TradeManager.Instance.CurrentMode == TradeMode.None;
    }
}
