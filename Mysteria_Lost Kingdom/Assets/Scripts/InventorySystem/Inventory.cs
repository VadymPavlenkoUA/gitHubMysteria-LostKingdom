using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Inventory : MonoBehaviour, ISaveable
{
    [Header("Save")]
    [SerializeField] private string saveID;
    public string GetSaveID() => saveID;

    public int slotCount = 20;
    public bool ignoreWeight = false;
    public PlayerStats playerStats;

    public event Action OnInventoryChanged;
    public List<InventorySlot> slots = new List<InventorySlot>();
    public List<InventorySlot> equipSlots = new List<InventorySlot>();

    public EquipmentManager eq;

    public void Awake()
    {
        //for (int i = 0; i < slotCount; i++) slots.Add(new InventorySlot());
        //for (int i = 0; i < 12; i++) equipSlots.Add(new InventorySlot());

        slots.Clear();
        for (int i = 0; i < slotCount; i++) slots.Add(new InventorySlot());

        // ≈к≥п≥ровка Ч я¬Ќќ
        equipSlots.Clear();
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.RightHand });
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.LeftHand });
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.RangeSlot });
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.ThrowSlot });
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.NecklaceSlot });
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.RingSlot });
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.BeltSlot });
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.HeadSlot });
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.ChestSlot });
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.HandsSlot });
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.LegsSlot });
        equipSlots.Add(new InventorySlot { slotSpec = InventorySlotUI.SlotSpecification.BootsSlot });
    }

    public void InitTrade(int slotsCount, bool ignoreWeight = true)
    {
        slotCount = slotsCount;
        this.ignoreWeight = ignoreWeight;

        slots.Clear();
        equipSlots.Clear();

        for (int i = 0; i < slotCount; i++) slots.Add(new InventorySlot());

        for (int i = 0; i < 12; i++) equipSlots.Add(new InventorySlot());
    }

    public void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    public float CurrentWeight
    {
        get
        {
            float weight = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty)
                    weight += slot.item.weight * slot.count;
            }

            foreach (var eqSlot in equipSlots)
            {
                if (!eqSlot.IsEmpty)
                    weight += eqSlot.item.weight * eqSlot.count;
            }

            return weight;
        }
    }

    public bool AddItem(Item item, int amount = 1)
    {
        if (item.isUnique)
        {
            Debug.LogError("AddItem called for UNIQUE item!");
            return false;
        }
        if (!ignoreWeight && CurrentWeight + item.weight * amount > playerStats.maxWeight)
        {
            Debug.Log("ѕеревищено вагу!");
            //return false;
        }

        int remaining = amount;

        foreach (var slot in slots)
        {
            if (slot.item == item && slot.count < item.maxStack)
            {
                remaining = slot.AddItem(item, remaining);
                if (remaining <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }
        }

        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                remaining = slot.AddItem(item, remaining);
                if (remaining <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }
        }

        Debug.Log("ЌемаЇ м≥сц€ в ≥нвентар≥!");
        return remaining <= 0;
    }

    public bool AddInstance(ItemInstance inst)
    {
        if (inst == null || inst.item == null)
            return false;

        if (!inst.item.isUnique)
        {
            Debug.LogError("AddInstance called for non-unique item!");
            return false;
        }

        // вага
        if (!ignoreWeight && CurrentWeight + inst.item.weight > playerStats.maxWeight)
        {
            Debug.Log("ѕеревищено вагу!");
            //return false;
        }

        // шукаЇмо порожн≥й слот
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.SetItem(inst);
                NotifyInventoryChanged();
                return true;
            }
        }

        Debug.Log("ЌемаЇ м≥сц€ в ≥нвентар≥!");
        return false;
    }


    public bool HasItem(Item item, int amount = 1)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.item == item)
                total += slot.count;
            if (total >= amount)
                return true;
        }
        return false;
    }

    public int GetItemCount(Item item)
    {
        if (item == null) return 0;
        int total = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.item == item)
                total += slot.count;
        }
        return total;
    }


    public bool RemoveItem(Item item, int amount = 1, bool notify = true)
    {
        if (item == null || amount <= 0) return false;

        int remaining = amount;
        bool removedAny = false;

        foreach (var slot in slots)
        {
            if (slot.IsEmpty) continue;
            if (slot.item != item) continue;

            var inst = slot.instance;

            if (inst.count >= remaining)
            {
                inst.count -= remaining;
                if (inst.count <= 0)
                    slot.Clear();

                removedAny = true;
                remaining = 0;
                break;
            }
            else
            {
                remaining -= inst.count;
                slot.Clear();
                removedAny = true;
            }
        }

        if (removedAny && notify)
        {
            InventoryUIManager.Instance.RefreshUI();
            NotifyInventoryChanged();
        }

        return removedAny;
    }

    public Item GetLockpickForLevel(int requiredLevel)
    {
        List<Item> lockpicks = new List<Item>();

        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && (slot.item.categories & ItemCategory.Lockpick) != 0)
                lockpicks.Add(slot.item);
        }

        if (lockpicks.Count == 0)
            return null; 

        lockpicks.Sort((a, b) => a.lockpickLevel.CompareTo(b.lockpickLevel));

        foreach (var lp in lockpicks)
        {
            if (lp.lockpickLevel >= requiredLevel)
                return lp;
        }

        return lockpicks[lockpicks.Count - 1];
    }

    public object CaptureState()
    {
        InventorySaveData data = new();

        foreach (var slot in slots) data.slots.Add(SaveSlot(slot));

        foreach (var slot in equipSlots) data.equipSlots.Add(SaveSlot(slot));

        return data;
    }

    private ItemInstanceSaveData SaveSlot(InventorySlot slot)
    {
        if (slot.IsEmpty) return null;

        var inst = slot.instance;

        if (slot.item.isUnique)
        {
            return new ItemInstanceSaveData
            {
                itemID = inst.item.itemID,
                count = inst.count,
                durability = inst.currentDurability,
                damage = inst.currentDamage,
                balanceDamage = inst.currentBalanceDamage,
                defenseMultiplier = inst.currentDefenseMultiplier,
                armor = inst.currentArmor,
                slotSpec = slot.slotSpec
            };
        }
        else
        {
            return new ItemInstanceSaveData
            {
                itemID = slot.item.itemID,
                count = slot.count,
                slotSpec = slot.slotSpec
            };
        }
    }

    //public void RestoreState(object state)
    //{
    //    var data = (InventorySaveData)state;

    //    slots.Clear();
    //    equipSlots.Clear();

    //    for (int i = 0; i < data.slots.Count; i++) slots.Add(LoadSlot(data.slots[i]));

    //    for (int i = 0; i < data.equipSlots.Count; i++) equipSlots.Add(LoadSlot(data.equipSlots[i]));

    //    NotifyInventoryChanged();
    //    InventoryUIManager.Instance?.RefreshUI();

    //    if (eq != null) eq.RestoreFromInventory(equipSlots);
    //}

    //public void RestoreState(object state)
    //{
    //    var data = (InventorySaveData)state;

    //    slots.Clear();
    //    //equipSlots.Clear();

    //    for (int i = 0; i < data.slots.Count; i++) slots.Add(LoadSlot(data.slots[i]));

    //    foreach (var slot in equipSlots) slot.Clear();

    //    foreach (var slotData in data.equipSlots)
    //    {
    //        if (slotData == null) continue;
    //        var slot = LoadSlot(slotData);

    //        var targetSlot = equipSlots.FirstOrDefault(s => s.slotSpec == slot.slotSpec);
    //        if (targetSlot != null)
    //        {
    //            if (slot.instance != null)
    //            {
    //                targetSlot.SetItem(slot.instance);
    //            }
    //            else if (slot.item != null)
    //            {
    //                targetSlot.SetItem(slot.item, slot.count);
    //            }
    //        }
    //        else
    //        {
    //            equipSlots.Add(slot);
    //        }
    //    }

    //    //for (int i = 0; i < data.equipSlots.Count; i++) equipSlots.Add(LoadSlot(data.equipSlots[i]));

    //    NotifyInventoryChanged();
    //    InventoryUIManager.Instance?.RefreshUI();

    //    if (eq != null) eq.RestoreFromInventory(equipSlots);
    //}

    public void RestoreState(object state)
    {
        var data = (InventorySaveData)state;

        slots.Clear();
        for (int i = 0; i < data.slots.Count; i++) slots.Add(LoadSlot(data.slots[i]));

        foreach (var slot in equipSlots) slot.Clear();

        foreach (var slotData in data.equipSlots)
        {
            if (slotData == null) continue;

            var loadedSlot = LoadSlot(slotData);

            var targetSlot = equipSlots.FirstOrDefault(s => s.slotSpec == loadedSlot.slotSpec);

            if (targetSlot != null)
            {
                if (loadedSlot.instance != null)
                {
                    targetSlot.SetItem(loadedSlot.instance);
                }
                else if (loadedSlot.item != null)
                {
                    targetSlot.SetItem(loadedSlot.item, loadedSlot.count);
                }
            }
            else
            {
                equipSlots.Add(loadedSlot);
            }
        }

        NotifyInventoryChanged();
        InventoryUIManager.Instance?.RefreshUI();
    }


    private InventorySlot LoadSlot(ItemInstanceSaveData data)
    {
        InventorySlot slot = new InventorySlot();

        if (data == null || string.IsNullOrEmpty(data.itemID))
            return slot;

        Item item = ItemDatabaseHolder.Instance.GetItem(data.itemID);
        if (item == null)
        {
            Debug.LogWarning($"[Inventory] Missing item ID in database: {data.itemID}");
            return slot;
        }

        if (item.isUnique)
        {
            ItemInstance inst = new ItemInstance(item, data.count)
            {
                currentDurability = data.durability,
                currentDamage = data.damage,
                currentBalanceDamage = data.balanceDamage,
                currentDefenseMultiplier = data.defenseMultiplier,
                currentArmor = data.armor
            };

            slot.SetItem(inst);
        }

        else
        {
            slot.SetItem(item, data.count);
        }

        slot.slotSpec = data.slotSpec;
        return slot;
    }


    //private InventorySlot LoadSlot(ItemInstanceSaveData data)
    //{
    //    InventorySlot slot = new InventorySlot();

    //    if (data == null)
    //        return slot;

    //    Item item = ItemDatabaseHolder.Instance.GetItem(data.itemID);
    //    if (item == null)
    //    {
    //        Debug.LogWarning($"[Inventory] Missing item ID in database: {data.itemID}");
    //        return slot;
    //    }

    //    ItemInstance inst = new ItemInstance(item, data.count)
    //    {
    //        currentDurability = data.durability,
    //        currentDamage = data.damage,
    //        currentBalanceDamage = data.balanceDamage,
    //        currentDefenseMultiplier = data.defenseMultiplier,
    //        currentArmor = data.armor
    //    };

    //    slot.SetItem(inst);
    //    return slot;
    //}

}
