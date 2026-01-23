using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TradeSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public TMP_Text amountText;

    private int index;
    private GameObject draggingIcon;

    public void Init(int slotIndex)
    {
        index = slotIndex;
        Refresh();
    }

    public void Refresh()
    {
        var slot = TradeManager.Instance.tradeSlots[index];

        if (slot.IsEmpty)
        {
            icon.enabled = false;
            amountText.text = "";
            return;
        }

        icon.enabled = true;
        icon.sprite = slot.item.icon;
        bool check = (slot.amount > 1);
        amountText.text = check ? slot.amount.ToString() : "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var slot = TradeManager.Instance.tradeSlots[index];
        if (slot.IsEmpty) return;

        draggingIcon = new GameObject("TradeDragIcon");
        draggingIcon.transform.SetParent(transform.root);
        draggingIcon.transform.SetAsLastSibling();

        var img = draggingIcon.AddComponent<Image>();
        img.sprite = slot.item.icon;
        img.color = new Color(1, 1, 1, 0.7f);
        img.raycastTarget = false;

        var rt = draggingIcon.GetComponent<RectTransform>();
        rt.sizeDelta = GetComponent<RectTransform>().sizeDelta;
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (draggingIcon != null)
            draggingIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingIcon != null)
            Destroy(draggingIcon);

        var targetSlot =
            eventData.pointerEnter?
            .GetComponentInParent<InventorySlotUI>();

        if (targetSlot == null)
            return;

        TradeManager.Instance.TryReturnFromTrade(index, targetSlot);
    }
}
