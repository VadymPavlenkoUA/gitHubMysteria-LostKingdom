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
        //if (UseActionManager.Instance.isUsing) return;
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
        //if (UseActionManager.Instance.isUsing) return;
        if (draggingIcon != null)
        {
            draggingIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //if (UseActionManager.Instance.isUsing) return;
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
                            slot.instance.count -= toMove;
                            otherSlotUI.slot.AddItem(slot.item, toMove);

                            SetSlot(slot);
                            otherSlotUI.SetSlot(otherSlotUI.slot);

                            HandleEquipmentSwapWithTwoHand(otherSlotUI.slot, otherSlotUI);
                        }
                    });
                }
                else
                {
                    TryStartEquipTimer(this, otherSlotUI);
                }
            }
        }
    }

    private void TryStartEquipTimer(InventorySlotUI a, InventorySlotUI b)
    {
        if (UseActionManager.Instance.isUsing)
            return;
        bool aIsEquip = a.slotType == SlotType.Equipment;
        bool bIsEquip = b.slotType == SlotType.Equipment;

        // Якщо обидва — інвентар → ТАЙМЕР НЕ ПОТРІБНИЙ
        if (!aIsEquip && !bIsEquip)
        {
            TryStackOrSwap(b);
            return;
        }

        // Якщо обидва — екіпіровка → ТАЙМЕР НЕ ПОТРІБНИЙ
        if (aIsEquip && bIsEquip)
        {
            TryStackOrSwap(b);
            return;
        }
        if (slotType == SlotType.Equipment && !b.slot.IsEmpty)
        {
            if ((b.slot.item.categories & allowedCategory) == 0)
            {
                Debug.Log("Not that category!");
                UseActionManager.Instance.StartUse(a.slot.item.useDuration, () => UnequipFromThisSlot(), () => Debug.Log("Скасовано!"));
                return;
            }
        }
        UseActionManager.Instance.StartUse(
            a.slot.item.useDuration,
            () => TryStackOrSwap(b),
            () => Debug.Log("Скасовано екіпіровку")
        );
    }


    private void TryStackOrSwap(InventorySlotUI other)
    {
        if (slotType == SlotType.Inventory && other.slotType == SlotType.Inventory)
        {
            if (slot.item == other.slot.item && slot.item != null)
            {
                int remaining = other.slot.AddItem(slot.item, slot.count);
                slot.instance.count = remaining;

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

        // Зберігаємо тимчасово item та count
        var tempItem = slot.item;
        var tempCount = slot.count;

        // Копіюємо значення від іншого слоту
        if (other.slot.IsEmpty)
            slot.Clear();
        else
            slot.SetItem(other.slot.item, other.slot.count);

        // Копіюємо тимчасові значення в інший слот
        if (tempItem == null)
            other.slot.Clear();
        else
            other.slot.SetItem(tempItem, tempCount);

        // Оновлюємо UI
        SetSlot(slot);
        other.SetSlot(other.slot);

        // Обробка дворучної зброї
        HandleEquipmentSwapWithTwoHand(slot, this);
        HandleEquipmentSwapWithTwoHand(other.slot, other);
    }


    private void HandleEquipmentSwapWithTwoHand(InventorySlot slotToEquip, InventorySlotUI slotUI)
    {
        var eq = EquipmentManager.Instance;
        bool isRight = eq.isRightHandDrawn;
        bool isLeft = eq.isLeftHandDrawn;

        // Якщо це не слот екіпірування — нічого не робимо
        if (slotUI.slotType != SlotType.Equipment) return;

        // Якщо слот порожній, потрібно просто роззняти його
        if (slotToEquip.IsEmpty)
        {
            bool isHand =
                slotUI.slotSpecification == SlotSpecification.RightHand ||
                slotUI.slotSpecification == SlotSpecification.LeftHand;
            if (isHand)
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
            }

            eq.Unequip(slotUI.slotSpecification);
            HotbarUI.Instance.RefreshHotbar();
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
                eq.EquipItem(slotToEquip.instance, rightHandSlot.slotSpecification);
                rightHandSlot.slot.SetItem(slotToEquip.item, 1);
                rightHandSlot.SetSlot(rightHandSlot.slot);
            }

            if (leftHandSlot != null)
            {
                eq.EquipItem(slotToEquip.instance, leftHandSlot.slotSpecification);
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
            eq.EquipItem(slotToEquip.instance, slotUI.slotSpecification);
            slotUI.slot.SetItem(slotToEquip.item, 1);
            slotUI.SetSlot(slotUI.slot);
        }

        InventoryUIManager.Instance.RefreshUI();
        HotbarUI.Instance.RefreshHotbar();
        UpdateDrawnWeapons(isRight, isLeft);
    }

    private void UpdateDrawnWeapons(bool isRightHandDrawn, bool isLeftHandDrawn)
    {
        var eq = EquipmentManager.Instance;

        if (isRightHandDrawn)
        {
            if (eq.equippedRightItem != null)
            {
                if (eq.equippedRightItem.item.weaponHandType == WeaponHandType.TwoHand)
                {
                    eq.HideTwoHanded();
                    eq.DrawTwoHand();
                    return;
                }
                eq.HideRightHand();
                eq.DrawRightHand();
            }
        }
        else
        {
            if (eq.equippedRightItem != null)
            {
                if (eq.equippedRightItem.item.weaponHandType == WeaponHandType.TwoHand)
                {
                    eq.isRightHandDrawn = true;
                    eq.isLeftHandDrawn = true;
                    eq.HideTwoHanded();
                    return;
                }
                eq.isRightHandDrawn = true;
                eq.HideRightHand();
            }
        }
        if (isLeftHandDrawn)
        {
            eq.HideLeftHand();
            if (eq.equippedLeftItem != null && eq.equippedLeftItem.item.weaponHandType == WeaponHandType.OneHand)
            {
                eq.DrawLeftHand();
            }
        }
        else
        {
            if (eq.equippedLeftItem != null && eq.equippedLeftItem.item.weaponHandType == WeaponHandType.OneHand)
            {
                eq.isLeftHandDrawn = true;
                eq.HideLeftHand();
            }
        }
    }


    private void UseFood(Item item)
    {
        var stats = InventoryUIManager.Instance.inventory.playerStats;

        if (item.satietyRestore != 0) stats.IncreaseSatiety(item.satietyRestore);
        if (item.healthRestore != 0) stats.Heal(item.healthRestore);

        slot.instance.count--;
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
        HotbarUI.Instance.RefreshHotbar();
    }

    private bool HasShieldEquipped(out InventorySlotUI shieldSlot)
    {
        shieldSlot = InventoryUIManager.Instance.equipmentSlots
            .FirstOrDefault(s => !s.slot.IsEmpty && s.slot.item.categories == ItemCategory.Shield);
        return shieldSlot != null;
    }

    private void TryEquipItem(ItemInstance inst)
    {
        var item = inst.item;
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

        EquipToSlot(inst, freeSlot);
        InventoryUIManager.Instance.NotifyInventoryChanged();
    }

    private void EquipToSlot(ItemInstance inst, InventorySlotUI equipSlot)
    {
        var item = inst.item;
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

            UseActionManager.Instance.StartUse(item.useDuration, () => EquipTwoHandedProgress(eq, inst, equipSlot, rightHandSlot, leftHandSlot), () => Debug.Log("Скасовано!"));
        }
        else
        {
            UseActionManager.Instance.StartUse(item.useDuration, () => EquipOneHandedProgress(eq, inst, equipSlot), () => Debug.Log("Скасовано!"));
        }

        InventoryUIManager.Instance.RefreshUI();
    }

    private void EquipOneHandedProgress(EquipmentManager eq, ItemInstance inst, InventorySlotUI equipSlot)
    {
        eq.EquipItem(inst, equipSlot.slotSpecification);
        equipSlot.slot.SetItem(inst.item, 1);
        equipSlot.SetSlot(equipSlot.slot);

        slot.instance.count--;
        if (slot.count <= 0) slot.Clear();
        SetSlot(slot);
        HotbarUI.Instance.RefreshHotbar();
        UpdateDrawnWeapons(eq.isRightHandDrawn, eq.isLeftHandDrawn);
    }

    private void EquipTwoHandedProgress(EquipmentManager eq, ItemInstance inst, InventorySlotUI equipSlot, InventorySlotUI rightHandSlot, InventorySlotUI leftHandSlot)
    {
        if (rightHandSlot != null)
        {
            eq.EquipItem(inst, rightHandSlot.slotSpecification);
            rightHandSlot.slot.SetItem(inst.item, 1);
            rightHandSlot.SetSlot(rightHandSlot.slot);
        }

        if (leftHandSlot != null)
        {
            eq.EquipItem(inst, leftHandSlot.slotSpecification);
            leftHandSlot.slot.SetItem(inst.item, 1);
            leftHandSlot.SetSlot(leftHandSlot.slot);
        }

        slot.instance.count--;
        if (slot.count <= 0) slot.Clear();
        SetSlot(slot);
        HotbarUI.Instance.RefreshHotbar();
        UpdateDrawnWeapons(eq.isRightHandDrawn, eq.isLeftHandDrawn);
    }


    internal void UseItem()
    {
        if (slot == null || slot.IsEmpty) return;
        ItemInstance inst = slot.instance;
        var item = inst.item;
        if ((item.categories & ItemCategory.Food) != 0)
        {
            UseActionManager.Instance.StartUse(item.useDuration, () => UseFood(item), () => Debug.Log("Скасовано!"));
            return;
        }

        if (slotType == SlotType.Equipment)
        {
            UseActionManager.Instance.StartUse(item.useDuration, () => UnequipFromThisSlot(), () => Debug.Log("Скасовано!"));
            return;
        }
        TryEquipItem(inst);
    }

    internal void SplitItem()
    {
        if (slot == null || slot.IsEmpty || slot.count <= 1) return;

        splitStackUI.Show(slot.count - 1, (amountChosen) =>
        {
            if (amountChosen <= 0) return;

            slot.instance.count -= amountChosen;
            SetSlot(slot);

            InventorySlotUI freeSlot = InventoryUIManager.Instance.FindFirstEmptySlot();
            if (freeSlot != null)
            {
                freeSlot.slot.AddItem(slot.item, amountChosen);
                freeSlot.SetSlot(freeSlot.slot);
            }
            else
            {
                slot.instance.count += amountChosen;
                SetSlot(slot);
            }
        });
        InventoryUIManager.Instance.RefreshUI();
    }

    internal void DropItem()
    {
        if (UseActionManager.Instance.isUsing)
        {
            UseActionManager.Instance.CancelUse();
        }
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
        if (UseActionManager.Instance.isUsing)
        {
            UseActionManager.Instance.CancelUse();
        }
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
