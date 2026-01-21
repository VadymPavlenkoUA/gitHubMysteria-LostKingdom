public static class TradeRules
{
    public static bool CanDrag(InventorySlotUI slot)
    {
        if (TradeManager.Instance == null)
            return true;

        var mode = TradeManager.Instance.CurrentMode;
        if (mode == TradeMode.None)
            return true;

        bool isPlayerInventory = slot.ownerInventory ==
            TradeManager.Instance.playerInventory;

        bool isTraderInventory = slot.ownerInventory ==
            TradeManager.Instance.traderInventory;

        if (mode == TradeMode.Buy)
            return isTraderInventory;

        if (mode == TradeMode.Sell)
            return isPlayerInventory;

        return false;
    }
}
