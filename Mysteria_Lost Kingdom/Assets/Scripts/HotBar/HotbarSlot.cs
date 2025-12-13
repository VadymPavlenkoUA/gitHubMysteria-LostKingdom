using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    public Image icon;
    public Image backIcon;
    public TextMeshProUGUI btnText;

    private Item assignedItem;

    public void SetItem(ItemInstance item)
    {
        //assignedItem = item.item;
        if (item == null)
        {
            icon.enabled = false;
            backIcon.enabled = true;
        }
        else
        {
            backIcon.enabled = false;
            icon.enabled = true;
            icon.sprite = item.item.icon;
        }
    }

    //public Item GetItem()
    //{
    //    return assignedItem;
    //}
}
