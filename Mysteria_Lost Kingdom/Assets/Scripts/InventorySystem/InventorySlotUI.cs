using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public enum SlotType { Inventory, Equipment};
    public enum SlotSpecification { RightHand, LeftHand, RangeSlot, ThrowSlot, NecklaceSlot, RingSlot, BeltSlot, HeadSlot, ChestSlot, HandsSlot, LegsSlot, BootsSlot};
    [SerializeField] private SlotType slotType = SlotType.Inventory;
    [SerializeField] private SlotSpecification slotSpecification = SlotSpecification.RightHand;
    [SerializeField] private ItemCategory allowedCategory;

    public Image icon;
    public Image emptyIcon;
    public TMP_Text countText;
    public SplitStackUI splitStackUI;
    public GameObject contextMenuPrefab;

    public Vector2 offset = new Vector2(100f, 30f);

    internal InventorySlot slot;
    private Transform originalParent;
    private GameObject draggingIcon;

    public void SetSlot(InventorySlot slot)
    {
        this.slot = slot;
        if (slot.IsEmpty)
        {
            icon.enabled = false;
            if (slotType == SlotType.Equipment) emptyIcon.enabled = true;
            countText.text = "";
        }
        else
        {
            if (slotType == SlotType.Equipment) emptyIcon.enabled = false;
            icon.enabled = true;
            icon.sprite = slot.item.icon;
            countText.text = slot.count > 1 ? slot.count.ToString() : "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slot != null && !slot.IsEmpty)
        {
            ItemDescriptionUI.Instance.ShowDescription(slot.item.description);
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ContextMenuUI.Instance.Show(this, transform.position + (Vector3)offset);
            }
        }
        else
        {
            ItemDescriptionUI.Instance.ClearDescription();
            ContextMenuUI.Instance.Hide();
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
                ItemDescriptionUI.Instance.ShowDescription(slot.item.description);
                if (otherSlotUI.slotType == SlotType.Equipment)
                {
                    if ((slot.item.categories & otherSlotUI.allowedCategory) == 0)
                    {
                        Debug.Log("Not that category!");
                        return;
                    }
                }

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

                            HandleEquipmentSwap(otherSlotUI);
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
        if (slotType == SlotType.Inventory && other.slotType == SlotType.Inventory) 
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
        }
        SwapSlots(other);
    }

    private void SwapSlots(InventorySlotUI other)
    {
        if (slotType == SlotType.Equipment && !other.slot.IsEmpty)
        {
            if ((other.slot.item.categories & allowedCategory) == 0)
            {
                Debug.Log("Not that category!");
                return;
            }
        }
        InventorySlot temp = new InventorySlot();
        temp.item = slot.item;
        temp.count = slot.count;

        slot.item = other.slot.item;
        slot.count = other.slot.count;

        other.slot.item = temp.item;
        other.slot.count = temp.count;

        SetSlot(slot);
        other.SetSlot(other.slot);

        HandleEquipmentSwap(other);
    }

    private void HandleEquipmentSwap(InventorySlotUI other)
    {
        if (slot == null || other.slot == null)
        {
            Debug.LogWarning("One of the slots is null!");
            return;
        }

        if (other.slot.item == null && other.slotType == SlotType.Equipment)
        {
            Debug.LogWarning("Other slot item is null!");
            return;
        }

        if (EquipmentManager.Instance == null)
        {
            Debug.LogWarning("EquipManager not assigned!");
            return;
        }

        if (other.slotType == SlotType.Equipment && !other.slot.IsEmpty)
        {
            EquipmentManager.Instance.EquipItem(other.slot.item, other.slotType, other.slotSpecification);
        }

        if (slotType == SlotType.Equipment && slot.IsEmpty)
        {
            if (slotSpecification == SlotSpecification.RightHand) EquipmentManager.Instance.UnequipRightHand();
            if (slotSpecification == SlotSpecification.LeftHand) EquipmentManager.Instance.UnequipLeftHand();
        }
    }

    internal void UseItem()
    {
        Debug.Log("Use: " + slot.item.name);
        InventoryUIManager.Instance.RefreshUI(); 
        InventoryUIManager.Instance.NotifyInventoryChanged();
    }

    internal void SplitItem()
    {
        if (slot == null || slot.IsEmpty || slot.count <= 1) return;

        splitStackUI.Show(slot.count - 1, (amountChosen) =>
        {
            if (amountChosen <= 0) return;

            slot.count -= amountChosen;
            SetSlot(slot);

            InventorySlotUI freeSlot = InventoryUIManager.Instance.FindFirstEmptySlot();
            if (freeSlot != null)
            {
                freeSlot.slot.AddItem(slot.item, amountChosen);
                freeSlot.SetSlot(freeSlot.slot);
            }
            else
            {
                slot.count += amountChosen;
                SetSlot(slot);
            }
        });
        InventoryUIManager.Instance.RefreshUI();
    }

    internal void DropItem()
    {
        if (slot == null || slot.IsEmpty) return;
        if (slot.item.itemPrefab == null)
        {
            Debug.Log("Prefab missing!");
            return;
        }

        Inventory inventory = InventoryUIManager.Instance.inventory;
        if (inventory == null)
        {
            Debug.LogWarning("Inventory not found!");
            return;
        }

        int amountToDrop = slot.count;

        // 1) Інстанціюємо фізичні префаби (можна інстанціювати після видалення, але краще перед щоб відчути момент)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        for (int i = 0; i < amountToDrop; i++)
        {
            Vector3 dropPos = player.transform.position + transform.forward * 1f;
            dropPos += new Vector3(Random.Range(-0.2f, 0.2f), 1f, Random.Range(-0.2f, 0.2f));
            Instantiate(slot.item.itemPrefab, dropPos, Quaternion.identity);
        }

        // 2) Тепер делегуємо видалення інвентарю
        inventory.RemoveItem(slot.item, amountToDrop);

        // 3) Оновлюємо локальний UI-слот (inventory.NotifyInventoryChanged() вже сповістить RefreshUI через підписку)
        SetSlot(slot); // тут slot вже змінений методами Inventory.RemoveItem (slot.Clear() викликано у Inventory)
        InventoryUIManager.Instance.RefreshUI();
    }

    internal void DropItem(int amount)
    {
        if (slot == null || slot.IsEmpty) return;
        if (slot.item.itemPrefab == null)
        {
            Debug.Log("Prefab missing!");
            return;
        }

        Inventory inventory = InventoryUIManager.Instance.inventory;
        if (inventory == null) return;

        int toDrop = Mathf.Min(amount, slot.count);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        for (int i = 0; i < toDrop; i++)
        {
            Vector3 dropPos = player.transform.position + transform.forward * 1f;
            dropPos += new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, Random.Range(-0.2f, 0.2f));
            Instantiate(slot.item.itemPrefab, dropPos, Quaternion.identity);
        }

        inventory.RemoveItem(slot.item, toDrop);
        SetSlot(slot);
        InventoryUIManager.Instance.RefreshUI();
    }


    //internal void DropItem()
    //{
    //    if (slot == null || slot.IsEmpty) return;
    //    if (slot.item.itemPrefab == null)
    //    {
    //        Debug.Log("Prefab missing!");
    //        return;
    //    }
    //    GameObject player = GameObject.FindGameObjectWithTag("Player");
    //    if (player == null) return;
    //    for (int i = 0; i < slot.count; i++)
    //    {
    //        Vector3 dropPos = player.transform.position + transform.forward * 1f;
    //        dropPos += new Vector3(Random.Range(-0.2f, 0.2f), 1f, Random.Range(-0.2f, 0.2f));

    //        Instantiate(slot.item.itemPrefab, dropPos, Quaternion.identity);
    //    }

    //    slot.Clear();
    //    SetSlot(slot);
    //    InventoryUIManager.Instance.RefreshUI();
    //    InventoryUIManager.Instance.NotifyInventoryChanged();
    //}
    //internal void DropItem(int amount)
    //{
    //    if (slot == null || slot.IsEmpty) return;
    //    if (slot.item.itemPrefab == null)
    //    {
    //        Debug.Log("Prefab missing!");
    //        return;
    //    }

    //    GameObject player = GameObject.FindGameObjectWithTag("Player");
    //    if (player == null) return;
    //    int toDrop = Mathf.Min(amount, slot.count);

    //    for (int i = 0; i < toDrop; i++)
    //    {
    //        Vector3 dropPos = player.transform.position + transform.forward * 1f;
    //        dropPos += new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, Random.Range(-0.2f, 0.2f));
    //        Instantiate(slot.item.itemPrefab, dropPos, Quaternion.identity);
    //    }
    //    slot.count -= toDrop;
    //    if (slot.count <= 0) slot.Clear();
    //    SetSlot(slot);
    //    InventoryUIManager.Instance.RefreshUI();
    //    InventoryUIManager.Instance.NotifyInventoryChanged();
    //}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
