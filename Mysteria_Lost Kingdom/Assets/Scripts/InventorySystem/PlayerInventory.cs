using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Inventory inventory;
    public InventoryUIManager uiManager;

    public void PickUpItem(Item item, int amount)
    {
        if (inventory.AddItem(item, amount))
        {
            uiManager.RefreshUI();
        }
    }

    public void PickUpInstance(ItemInstance inst)
    {
        if (inventory.AddInstance(inst))
        {
            uiManager.RefreshUI();
        }
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
