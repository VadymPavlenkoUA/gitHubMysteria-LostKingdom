using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public enum SlotType { Inventory, Equipment };
    public enum SlotSpecification { RightHand, LeftHand, RangeSlot, ThrowSlot, NecklaceSlot, RingSlot, BeltSlot, HeadSlot, ChestSlot, HandsSlot, LegsSlot, BootsSlot, None };
    [SerializeField] private SlotType slotType = SlotType.Inventory;
    [SerializeField] internal SlotSpecification slotSpecification = SlotSpecification.None;
    [SerializeField] private ItemCategory allowedCategory;

    public Image icon;
    public Image emptyIcon;
    public TMP_Text countText;
    public SplitStackUI splitStackUI;
    public GameObject contextMenuPrefab;

    public Vector2 offset = new Vector2(100f, 30f);

    internal InventorySlot slot;
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

                            HandleEquipmentSwapWithTwoHand(otherSlotUI.slot, otherSlotUI);
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
        // Перевірка категорій для слотів екіпірування
        if (slotType == SlotType.Equipment && !other.slot.IsEmpty)
        {
            if ((other.slot.item.categories & allowedCategory) == 0)
            {
                Debug.Log("Not that category!");
                return;
            }
        }

        InventorySlot temp = new InventorySlot
        {
            item = slot.item,
            count = slot.count
        };

        slot.item = other.slot.item;
        slot.count = other.slot.count;

        other.slot.item = temp.item;
        other.slot.count = temp.count;

        SetSlot(slot);
        other.SetSlot(other.slot);

        HandleEquipmentSwapWithTwoHand(slot, this);
        HandleEquipmentSwapWithTwoHand(other.slot, other);
    }

    private void HandleEquipmentSwapWithTwoHand(InventorySlot slotToEquip, InventorySlotUI slotUI)
    {
        var eq = EquipmentManager.Instance;

        // Якщо це не слот екіпірування — нічого не робимо
        if (slotUI.slotType != SlotType.Equipment) return;

        // Якщо слот порожній, потрібно просто роззняти його
        if (slotToEquip.IsEmpty)
        {
            // Якщо цей слот є RightHand або LeftHand і там дворучна зброя — очистити обидва слоти
            var handSlots = InventoryUIManager.Instance.equipmentSlots
                .Where(s => s.slotSpecification == SlotSpecification.RightHand || s.slotSpecification == SlotSpecification.LeftHand);

            foreach (var hand in handSlots)
            {
                if (!hand.slot.IsEmpty && hand.slot.item.weaponHandType == WeaponHandType.TwoHand)
                {
                    eq.Unequip(hand.slotSpecification);
                    hand.slot.Clear();
                    hand.SetSlot(hand.slot);
                }
            }

            eq.Unequip(slotUI.slotSpecification);
            return;
        }

        // Якщо предмет дворучний — одягаємо в обидва слоти
        if (slotToEquip.item.weaponHandType == WeaponHandType.TwoHand)
        {
            var rightHandSlot = InventoryUIManager.Instance.equipmentSlots
                .FirstOrDefault(s => s.slotSpecification == SlotSpecification.RightHand);
            var leftHandSlot = InventoryUIManager.Instance.equipmentSlots
                .FirstOrDefault(s => s.slotSpecification == SlotSpecification.LeftHand);

            // Спершу очищаємо обидва слоти, якщо там щось стоїть
            foreach (var hand in new[] { rightHandSlot, leftHandSlot })
            {
                if (hand != null && !hand.slot.IsEmpty && hand.slot.item != slotToEquip.item)
                {
                    InventoryUIManager.Instance.inventory.AddItem(hand.slot.item, hand.slot.count);
                    hand.slot.Clear();
                    hand.SetSlot(hand.slot);
                    eq.Unequip(hand.slotSpecification);
                }
            }

            // Екіпіруємо нову дворучну
            if (rightHandSlot != null)
            {
                eq.EquipItem(slotToEquip.item, rightHandSlot.slotType, rightHandSlot.slotSpecification);
                rightHandSlot.slot.SetItem(slotToEquip.item, 1);
                rightHandSlot.SetSlot(rightHandSlot.slot);
            }

            if (leftHandSlot != null)
            {
                eq.EquipItem(slotToEquip.item, leftHandSlot.slotType, leftHandSlot.slotSpecification);
                leftHandSlot.slot.SetItem(slotToEquip.item, 1);
                leftHandSlot.SetSlot(leftHandSlot.slot);
            }
        }
        else
        {
            // Одноручна зброя
            // Перевіряємо, чи в парному слоті стоїть дворучка
            InventorySlotUI pairedSlot = null;

            if (slotUI.slotSpecification == SlotSpecification.RightHand)
                pairedSlot = InventoryUIManager.Instance.equipmentSlots
                    .FirstOrDefault(s => s.slotSpecification == SlotSpecification.LeftHand);
            else if (slotUI.slotSpecification == SlotSpecification.LeftHand)
                pairedSlot = InventoryUIManager.Instance.equipmentSlots
                    .FirstOrDefault(s => s.slotSpecification == SlotSpecification.RightHand);

            if (pairedSlot != null && !pairedSlot.slot.IsEmpty && pairedSlot.slot.item.weaponHandType == WeaponHandType.TwoHand)
            {
                // Роззняти дворучку з обох слотів
                eq.Unequip(pairedSlot.slotSpecification);
                pairedSlot.slot.Clear();
                pairedSlot.SetSlot(pairedSlot.slot);

                // Також очистити слот, з якого ми знімали дворучку (якщо він не поточний)
                if (pairedSlot.slotSpecification != slotUI.slotSpecification)
                {
                    eq.Unequip(slotUI.slotSpecification);
                }
            }

            eq.EquipItem(slotToEquip.item, slotUI.slotType, slotUI.slotSpecification);
            slotUI.slot.SetItem(slotToEquip.item, 1);
            slotUI.SetSlot(slotUI.slot);
        }

        InventoryUIManager.Instance.RefreshUI();
    }



    //private void HandleEquipmentSwap(InventorySlotUI other)
    //{
    //    bool thisIsEquip = this.slotType == SlotType.Equipment;
    //    bool otherIsEquip = other.slotType == SlotType.Equipment;

    //    if (!thisIsEquip && !otherIsEquip)
    //        return;


    //    if (thisIsEquip)
    //    {
    //        if (slot.IsEmpty)
    //        {
    //            EquipmentManager.Instance.Unequip(slotSpecification);
    //        }
    //        else
    //        {
    //            EquipmentManager.Instance.EquipItem(
    //                slot.item,
    //                slotType,
    //                slotSpecification
    //            );
    //        }
    //    }

    //    if (otherIsEquip)
    //    {
    //        if (other.slot.IsEmpty)
    //        {
    //            EquipmentManager.Instance.Unequip(other.slotSpecification);
    //        }
    //        else
    //        {
    //            EquipmentManager.Instance.EquipItem(
    //                other.slot.item,
    //                other.slotType,
    //                other.slotSpecification
    //            );
    //        }
    //    }
    //}


    //private void HandleEquipmentSwap(InventorySlotUI other)
    //{
    //    if (slot == null || other.slot == null)
    //    {
    //        Debug.LogWarning("One of the slots is null!");
    //        return;
    //    }

    //    if (other.slot.item == null && other.slotType == SlotType.Equipment)
    //    {
    //        Debug.LogWarning("Other slot item is null!");
    //        return;
    //    }

    //    if (EquipmentManager.Instance == null)
    //    {
    //        Debug.LogWarning("EquipManager not assigned!");
    //        return;
    //    }

    //    if (other.slotType == SlotType.Equipment && !other.slot.IsEmpty)
    //    {
    //        EquipmentManager.Instance.EquipItem(other.slot.item, other.slotType, other.slotSpecification);
    //    }

    //    if (slotType == SlotType.Equipment && slot.IsEmpty)
    //    {
    //        if (slotSpecification == SlotSpecification.RightHand) EquipmentManager.Instance.UnequipRightHand();
    //        if (slotSpecification == SlotSpecification.LeftHand) EquipmentManager.Instance.UnequipLeftHand();
    //    }
    //}

    private void UseFood(Item item)
    {
        var stats = InventoryUIManager.Instance.inventory.playerStats;

        stats.currentSatiety += item.satietyRestore;
        stats.currentSatiety = Mathf.Clamp(stats.currentSatiety, 0, stats.maxSatiety);

        if (item.healthRestore != 0) stats.Heal(item.healthRestore);

        slot.count--;
        if (slot.count <= 0) slot.Clear();

        SetSlot(slot);
        InventoryUIManager.Instance.RefreshUI();
        InventoryUIManager.Instance.NotifyInventoryChanged();

        Debug.Log($"Використано їжу '{item.itemName}': {item.satietyRestore} ситості / {item.healthRestore} здоров'я");
    }

    private void UnequipFromThisSlot()
    {
        var eq = EquipmentManager.Instance;

        if (slot.IsEmpty) return;

        // Якщо предмет дворучний, очистити обидва слоти
        if (slot.item.weaponHandType == WeaponHandType.TwoHand)
        {
            var rightHandSlot = InventoryUIManager.Instance.equipmentSlots
                .FirstOrDefault(s => s.slotSpecification == SlotSpecification.RightHand);
            var leftHandSlot = InventoryUIManager.Instance.equipmentSlots
                .FirstOrDefault(s => s.slotSpecification == SlotSpecification.LeftHand);

            InventoryUIManager.Instance.inventory.AddItem(slot.item, 1);

            if (rightHandSlot != null && !rightHandSlot.slot.IsEmpty)
            {
                eq.Unequip(SlotSpecification.RightHand);
                rightHandSlot.slot.Clear();
                rightHandSlot.SetSlot(rightHandSlot.slot);
            }

            if (leftHandSlot != null && !leftHandSlot.slot.IsEmpty)
            {
                eq.Unequip(SlotSpecification.LeftHand);
                leftHandSlot.slot.Clear();
                leftHandSlot.SetSlot(leftHandSlot.slot);
            }
        }
        else
        {
            // Одноручна зброя або інший предмет
            eq.Unequip(slotSpecification);
            InventoryUIManager.Instance.inventory.AddItem(slot.item, 1);
            slot.Clear();
            SetSlot(slot);
        }

        InventoryUIManager.Instance.RefreshUI();
        InventoryUIManager.Instance.NotifyInventoryChanged();
    }


    private void TryEquipItem(Item item)
    {
        var equipSlots = InventoryUIManager.Instance.equipmentSlots;
        InventorySlotUI freeSlot = null;

        foreach (var eSlot in equipSlots)
        {
            if ((item.categories & eSlot.allowedCategory) == 0)
                continue;

            if (eSlot.slot.IsEmpty)
            {
                freeSlot = eSlot;
                break;
            }
        }

        if (freeSlot == null)
        {
            Debug.Log("Немає вільних екіпіровочних слотів!");
            return;
        }

        EquipToSlot(item, freeSlot);
        InventoryUIManager.Instance.NotifyInventoryChanged();
    }

    private void EquipToSlot(Item item, InventorySlotUI equipSlot)
    {
        var eq = EquipmentManager.Instance;
        if (item.weaponHandType == WeaponHandType.TwoHand)
        {
            InventorySlotUI rightHandSlot = null;
            InventorySlotUI leftHandSlot = null;

            foreach (var slotUI in InventoryUIManager.Instance.equipmentSlots)
            {
                if (slotUI.slotSpecification == SlotSpecification.RightHand) rightHandSlot = slotUI;
                if (slotUI.slotSpecification == SlotSpecification.LeftHand) leftHandSlot = slotUI;
            }

            if ((rightHandSlot == null || !rightHandSlot.slot.IsEmpty) || 
            (leftHandSlot == null || !leftHandSlot.slot.IsEmpty))
        {
            Debug.Log("Не вистачає вільних слотів для дворучної зброї!");
            return;
        }

            if (rightHandSlot != null)
            {
                eq.EquipItem(item, rightHandSlot.slotType, rightHandSlot.slotSpecification);
                rightHandSlot.slot.SetItem(item, 1);
                rightHandSlot.SetSlot(rightHandSlot.slot);
            }

            if (leftHandSlot != null)
            {
                eq.EquipItem(item, leftHandSlot.slotType, leftHandSlot.slotSpecification);
                leftHandSlot.slot.SetItem(item, 1);
                leftHandSlot.SetSlot(leftHandSlot.slot);
            }

            slot.count--;
            if (slot.count <= 0) slot.Clear();
            SetSlot(slot);
        }
        else
        {
            eq.EquipItem(item, equipSlot.slotType, equipSlot.slotSpecification);
            equipSlot.slot.SetItem(item, 1);
            equipSlot.SetSlot(equipSlot.slot);

            slot.count--;
            if (slot.count <= 0) slot.Clear();
            SetSlot(slot);
        }

        InventoryUIManager.Instance.RefreshUI();
    }


    internal void UseItem()
    {
        if (slot == null || slot.IsEmpty) return;
        Item item = slot.item;
        if ((item.categories & ItemCategory.Food) != 0)
        {
            UseFood(item);
            return;
        }

        if (slotType == SlotType.Equipment)
        {
            UnequipFromThisSlot();
            return;
        }
        TryEquipItem(item);

        Debug.Log("Цей предмет не можливо використати!");
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

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        int amountToDrop = slot.count;

        for (int i = 0; i < amountToDrop; i++)
        {
            Vector3 dropPos = player.transform.position + transform.forward * 1f;
            dropPos += new Vector3(Random.Range(-0.2f, 0.2f), 1f, Random.Range(-0.2f, 0.2f));
            Instantiate(slot.item.itemPrefab, dropPos, Quaternion.identity);
        }

        if (slotType == SlotType.Equipment)
        {
            var eq = EquipmentManager.Instance;

            if (slot.item.weaponHandType == WeaponHandType.TwoHand)
            {
                foreach (var eqSlotUI in InventoryUIManager.Instance.equipmentSlots)
                {
                    if (eqSlotUI.slotSpecification == SlotSpecification.RightHand ||
                        eqSlotUI.slotSpecification == SlotSpecification.LeftHand)
                    {
                        eq?.Unequip(eqSlotUI.slotSpecification);
                        eqSlotUI.slot.Clear();
                        eqSlotUI.SetSlot(eqSlotUI.slot);
                    }
                }
            }
            else
            {
                eq?.Unequip(slotSpecification);
                slot.Clear();
                SetSlot(slot);
            }

            InventoryUIManager.Instance.RefreshUI();
            return;
        }

        Inventory inventory = InventoryUIManager.Instance.inventory;
        if (inventory != null)
        {
            inventory.RemoveItem(slot.item, amountToDrop);
        }

        SetSlot(slot);
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

        int toDrop = Mathf.Min(amount, slot.count);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        for (int i = 0; i < toDrop; i++)
        {
            Vector3 dropPos = player.transform.position + transform.forward * 1f;
            dropPos += new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, Random.Range(-0.2f, 0.2f));
            Instantiate(slot.item.itemPrefab, dropPos, Quaternion.identity);
        }

        if (slotType == SlotType.Equipment)
        {
            EquipmentManager.Instance?.Unequip(slotSpecification);

            slot.Clear();
            SetSlot(slot);
            InventoryUIManager.Instance.RefreshUI();
            return;
        }

        Inventory inventory = InventoryUIManager.Instance.inventory;
        if (inventory != null)
        {
            inventory.RemoveItem(slot.item, toDrop);
        }

        SetSlot(slot);
        InventoryUIManager.Instance.RefreshUI();
    }
}
