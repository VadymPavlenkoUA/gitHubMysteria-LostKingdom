using Unity.Cinemachine;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public DialogueData dialogueData;

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
        DialogueManager.Instance.StartDialogue(dialogueData);
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
