using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public TMP_Text countText;
    public SplitStackUI splitStackUI;

    private InventorySlot slot;
    private Transform originalParent;
    private GameObject draggingIcon;

    public void SetSlot(InventorySlot slot)
    {
        this.slot = slot;
        if (slot.IsEmpty)
        {
            icon.enabled = false;
            countText.text = "";
        }
        else
        {
            icon.enabled = true;
            icon.sprite = slot.item.icon;
            countText.text = slot.count > 1 ? slot.count.ToString() : "";
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slot == null || slot.IsEmpty) return;

        draggingIcon = new GameObject("DraggingIcon");
        draggingIcon.transform.SetParent(transform.root);
        draggingIcon.transform.SetAsLastSibling();

        Image img = draggingIcon.AddComponent<Image>();
        img.sprite = slot.item.icon;
        img.color = new Color(1f, 1f, 1f, 0.7f);
        img.raycastTarget = false;

        RectTransform rt = draggingIcon.GetComponent<RectTransform>();
        rt.sizeDelta = icon.rectTransform.sizeDelta;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingIcon != null)
        {
            draggingIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingIcon != null) Destroy(draggingIcon);
        if (slot == null || slot.IsEmpty) return;
        if (eventData.pointerEnter != null)
        {
            InventorySlotUI otherSlotUI = eventData.pointerEnter.GetComponentInParent<InventorySlotUI>();
            if (otherSlotUI != null && otherSlotUI != this)
            {
                bool ctrl = Keyboard.current.leftCtrlKey.isPressed;
                if (ctrl && slot.count > 1)
                {
                    if (!otherSlotUI.slot.IsEmpty && otherSlotUI.slot.item != slot.item)
                    {
                        TryStackOrSwap(otherSlotUI);
                        return;
                    }
                    splitStackUI.Show(slot.count - 1, (amountChosen) =>
                    {
                        int spaceLeft = otherSlotUI.slot.IsEmpty ? amountChosen : otherSlotUI.slot.item.maxStack - otherSlotUI.slot.count;
                        int toMove = Mathf.Min(amountChosen, spaceLeft);

                        if (toMove > 0)
                        {
                            slot.count -= toMove;
                            otherSlotUI.slot.AddItem(slot.item, toMove);

                            SetSlot(slot);
                            otherSlotUI.SetSlot(otherSlotUI.slot);
                        }
                    });
                }
                else
                {
                    TryStackOrSwap(otherSlotUI);
                }
            }
        }
    }

    private void TryStackOrSwap(InventorySlotUI other)
    {
        if (slot.item == other.slot.item && slot.item != null)
        {
            int remaining = other.slot.AddItem(slot.item, slot.count);
            slot.count = remaining;

            SetSlot(slot);
            other.SetSlot(other.slot);

            if (slot.count <= 0)
            {
                slot.Clear();
                SetSlot(slot);
            }
            return;
        }
        SwapSlots(other);
    }

    private void SwapSlots(InventorySlotUI other)
    {
        InventorySlot temp = new InventorySlot();
        temp.item = slot.item;
        temp.count = slot.count;

        slot.item = other.slot.item;
        slot.count = other.slot.count;

        other.slot.item = temp.item;
        other.slot.count = temp.count;

        SetSlot(slot);
        other.SetSlot(other.slot);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
