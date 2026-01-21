using Unity.Cinemachine;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable, IClosableInteraction
{
    public DialogueData dialogueData;
    public Transform InteractionTransform => transform;

    public Inventory traderInventory = null;

    public void OnInteractionClosed()
    {
        if (!DialogueManager.Instance.isDialogueOpen) return;

        DialogueManager.Instance.EndDialogue();
        Debug.Log("Діалог закрито через відстань");
    }

    public string GetInteractionNameText()
    {
        return $"Поговорити з {dialogueData.npcName}";
    }

    public string GetInteractionBTNText()
    {
        return $"Натисніть \"E\"";
    }

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(dialogueData, traderInventory);
        InteractionDistanceWatcher.Instance.StartWatching(this);
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
