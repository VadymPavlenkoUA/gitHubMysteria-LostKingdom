using Unity.Cinemachine;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable, IClosableInteraction
{
    public DialogueData dialogueData;
    public Transform InteractionTransform => transform;

    public TraderData traderData;
    internal Inventory traderInventory = null;

    private int currentGold;
    public int Gold => currentGold;

    public void OnInteractionClosed()
    {
        if (!DialogueManager.Instance.isDialogueOpen && !TradeManager.Instance.isTradeOpen) return;
        else if (DialogueManager.Instance.isDialogueOpen)
        {
            DialogueManager.Instance.EndDialogue();
            Debug.Log("Діалог закрито через відстань");
        }
        else if (TradeManager.Instance.isTradeOpen)
        {
            TradeManager.Instance.CloseTrade();
            Debug.Log("Торгівлю закрито через відстань");
        }
    }

    public string GetInteractionNameText()
    {
        return $"Поговорити з {dialogueData.npcName}";
    }

    public string GetInteractionBTNText()
    {
        return $"Натисніть \"E\"";
    }

    public void InitTraderInventory()
    {
        if (traderInventory != null) return;

        traderInventory = gameObject.AddComponent<Inventory>();
        traderInventory.InitTrade(traderData.inventorySlots, ignoreWeight: true);

        currentGold = traderData.startGold;

        foreach (var tradeItem in traderData.items)
        {
            if (tradeItem.item == null) continue;

            // Визначаємо випадкову кількість між min та max
            int amount = Random.Range(tradeItem.minAmount, tradeItem.maxAmount + 1);

            if (tradeItem.item.isUnique)
            {
                for (int i = 0; i < amount; i++)
                {
                    traderInventory.AddInstance(new ItemInstance(tradeItem.item));
                }
            }
            else
            {
                traderInventory.AddItem(tradeItem.item, amount);
            }
        }
    }

    public bool TrySpendGold(int amount)
    {
        if (traderData.startGold >= amount)
        {
            currentGold -= amount;
            return true;
        }
        return false;
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
    }

    public void Interact()
    {
        InitTraderInventory();
        DialogueManager.Instance.StartDialogue(dialogueData, this);
        InteractionDistanceWatcher.Instance.StartWatching(this);
    }
}
