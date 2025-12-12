using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    public Item item;
    public int amount = 1;

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        Debug.Log("Pick up!");
    //        PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();
    //        if (playerInventory != null)
    //        {
    //            playerInventory.PickUpItem(item, amount);
    //            Destroy(gameObject);
    //        }
    //    }
    //}

    public string GetInteractionNameText()
    {
        return $"{item.itemName} {amount}x";
    }

    public string GetInteractionBTNText()
    {
        return $"Натисніть \"E\"";
    }

    public void Interact()
    {
        PlayerInventory playerInventory = FindAnyObjectByType<PlayerInventory>();
        if (playerInventory != null)
        {
            playerInventory.PickUpItem(item, amount);
            NotificationSystem.Instance.ShowNotification(item.icon, $"Підібрано {item.itemName} x{amount}");
            Destroy(gameObject);
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
