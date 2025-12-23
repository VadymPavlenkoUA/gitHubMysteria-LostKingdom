using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    public Image icon;
    public Image backIcon;
    public TextMeshProUGUI btnText;

    [Header("Highlight")]
    public Image background;             
    public Color normalColor = Color.white;
    public Color activeColor = new Color(1f, 1f, 1f, 1.25f); 

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

    public void SetActive(bool active)
    {
        if (background == null) return;
        background.color = active ? activeColor : normalColor;
    }

    //public Item GetItem()
    //{
    //    return assignedItem;
    //}
}
