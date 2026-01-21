using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TradeSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text amountText;

    private int index;

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
        amountText.text = slot.amount.ToString();
    }
}
