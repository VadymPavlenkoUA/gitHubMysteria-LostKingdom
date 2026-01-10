using System;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class ChestInventory : MonoBehaviour, IInteractable
{
    public TaskDatabase taskDB;
    public enum ChestType
    {
        Звичайна,
        Велика,
        Залізна,
        Магічна,
        Легендарна
    }

    [Header("Inventory")]
    public Inventory chestInventory;
    public ChestVisual chestVisual;

    [Header("Chest Settings")]
    public ChestType chestType = ChestType.Звичайна; 
    public bool spawnsRandomLoot = false;
    public bool isLocked = true;                     
    private bool hasLootSpawned = false;

    [Header("Loot")]
    public LootTable defaultChestLoot;

    public string GetInteractionNameText()
    {
        string lockText = isLocked ? " (замкнено)" : "";
        return $"Відчинити \"{chestType} скриня{lockText}\"";
    }

    public string GetInteractionBTNText()
    {
        return $"Натисніть \"E\"";
    }

    public void Interact()
    {
        Inventory playerInventory = FindPlayerInventory();
        if (playerInventory == null)
        {
            Debug.LogError("Не знайдено Inventory гравця!");
            return;
        }

        if (spawnsRandomLoot && !hasLootSpawned)
        {
            SpawnRandomLoot();
            hasLootSpawned = true;
        }

        if (isLocked)
        {
            if (!TryUseLockpick(playerInventory)) return;
        }
        else
        {
            UseActionManager.Instance.StartUse(0.5f, () => InventoryUIManager.Instance.OpenChest(chestInventory, chestVisual, $"{chestType} скриня"), () => Debug.Log("Скасовано!"));
        }
    }

    private Inventory FindPlayerInventory()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Inventory inv = player.GetComponent<Inventory>();
            if (inv != null) return inv;
        }

        return FindFirstObjectByType<Inventory>();
    }

    private TaskUIController FindTaskUIController()
    {
        GameObject taskRoot = GameObject.FindWithTag("TaskRoot");
        if (taskRoot != null)
        {
            TaskUIController taskUIController = taskRoot.GetComponent<TaskUIController>();
            if (taskUIController != null) return taskUIController;
        }

        return FindFirstObjectByType<TaskUIController>();
    }

    public bool TryUseLockpick(Inventory playerInventory)
    {
        int requiredLevel = GetRequiredLockpickLevel();
        Item lockpick = playerInventory.GetLockpickForLevel(requiredLevel);

        if (lockpick == null)
        {
            NotificationSystem.Instance.ShowNotification(NotificationSystem.Instance.lockSprite, $"У вас немає відмички {requiredLevel+1} чи вище!");
            return false;
        }

        TaskRequirement task = taskDB.GetRandomTask();
        if (task != null)
        {
            TaskUIController taskUI = FindTaskUIController();
            Action<TaskResult> onComplete = null;
            onComplete = (result) =>
            {
                taskUI.OnTaskComplete -= onComplete;

                if (result.correct)
                {
                    NotificationSystem.Instance.ShowNotification(NotificationSystem.Instance.unlockSprite, $"\"{chestType} скриня\" відчинена!");
                    isLocked = false;
                    UseActionManager.Instance.StartUse(0.5f, () => InventoryUIManager.Instance.OpenChest(chestInventory, chestVisual, $"{chestType} скриня"), () => Debug.Log("Скасовано!"));
                }
                else
                {
                    NotificationSystem.Instance.ShowNotification(NotificationSystem.Instance.lockSprite, $"Відмичка {lockpick.lockpickLevel + 1} рівня зламалася!");
                    playerInventory.RemoveItem(lockpick, 1);
                }
            };

            taskUI.OnTaskComplete += onComplete;
            taskUI.ShowTask(task);

            return true;
        }
        else
        {
            NotificationSystem.Instance.ShowNotification(NotificationSystem.Instance.unlockSprite, $"\"{chestType} скриня\" відчинена!");
            Debug.Log($"Відкриваємо скриню \"{chestType}\" без завдання");
            isLocked = false;
            UseActionManager.Instance.StartUse(0.5f, () => InventoryUIManager.Instance.OpenChest(chestInventory, chestVisual, $"{chestType} скриня"), () => Debug.Log("Скасовано!"));
            return true;
        }
    }


    private void SpawnRandomLoot()
    {
        if (defaultChestLoot == null || defaultChestLoot.lootItems == null || defaultChestLoot.lootItems.Count == 0)
            return;

        int freeSlots = chestInventory.slots.Count(s => s.IsEmpty);
        if (freeSlots <= 0)
            return;

        foreach (var loot in defaultChestLoot.lootItems)
        {
            if (freeSlots <= 0)
                break;

            if (UnityEngine.Random.value > loot.dropChance)
                continue;

            int amount = UnityEngine.Random.Range(loot.minAmount, loot.maxAmount + 1);

            if (loot.item.isUnique)
            {
                ItemInstance inst = new ItemInstance(loot.item, 1);

                bool added = chestInventory.AddInstance(inst);
                if (added)
                    freeSlots--;
            }
            else
            {
                int beforeEmpty = chestInventory.slots.Count(s => s.IsEmpty);

                bool added = chestInventory.AddItem(loot.item, amount);

                int afterEmpty = chestInventory.slots.Count(s => s.IsEmpty);
                if (added && afterEmpty < beforeEmpty)
                    freeSlots--;
            }
        }
        
        chestInventory.NotifyInventoryChanged();
        Debug.Log("Лут у скрині згенеровано!");
    }



    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    public int GetRequiredLockpickLevel()
    {
        switch (chestType)
        {
            case ChestType.Звичайна: return 0;
            case ChestType.Велика: return 1;
            case ChestType.Залізна: return 2;
            case ChestType.Магічна: return 3;
            case ChestType.Легендарна: return 4;
            default: return 0;
        }
    }
}