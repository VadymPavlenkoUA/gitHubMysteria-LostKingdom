using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public int slotCount = 20;
    public float maxWeight = 50f;

    public List<InventorySlot> slots = new List<InventorySlot>();

    public void Awake()
    {
        for (int i = 0; i < slotCount; i++) slots.Add(new InventorySlot());
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
            return weight;
        }
    }

    public bool AddItem(Item item, int amount = 1)
    {
        if (CurrentWeight + item.weight * amount > maxWeight)
        {
            Debug.Log("Перевищено вагу!");
            return false;
        }

        int remaining = amount;

        foreach(var slot in slots)
        {
            if (slot.item == item && slot.count < item.maxStack)
            {
                remaining = slot.AddItem(item, remaining);
                if (remaining <= 0) return true;
            }
        }

        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                remaining = slot.AddItem(item, remaining);
                if (remaining <= 0) return true;
            }
        }

        Debug.Log("Немає місця в інвентарі!");
        return remaining <= 0;
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
