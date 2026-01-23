using UnityEngine;
using UnityEngine.EventSystems;


public class TradeCenterDrop : MonoBehaviour, IDropHandler
{
    public int tradeSlotIndex;

    public void OnDrop(PointerEventData eventData)
    {
        var fromSlotUI = eventData.pointerDrag?
            .GetComponent<InventorySlotUI>();

        if (fromSlotUI == null)
            return;

        if (!TradeRules.CanDropIntoTrade(fromSlotUI))
            return;

        TradeManager.Instance.TryAddToTrade(fromSlotUI, tradeSlotIndex);
    }
}
