using Unity.Cinemachine;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public string npcName = "Марко";
    public GameObject dialogPanel;
    [SerializeField] private CinemachineInputAxisController cinemachineInput;

    public string GetInteractionNameText()
    {
        return $"Поговорити з {npcName}";
    }

    public string GetInteractionBTNText()
    {
        return $"Натисніть \"E\"";
    }

    public void Interact()
    {
        //DialogueManager.Instance.StartDialogue(npcName);
        dialogPanel.SetActive(true);
        cinemachineInput.enabled = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
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
