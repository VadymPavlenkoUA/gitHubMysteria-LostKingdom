using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ContextMenuUI : MonoBehaviour
{
    public static ContextMenuUI Instance;

    public GameObject panel;
    public Button useBTN;
    public Button splitBTN;
    public Button dropBTN;

    private InventorySlotUI currentSlot;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        panel.SetActive(false);
    }

    public void Show(InventorySlotUI slot, Vector3 position)
    {
        currentSlot = slot;
        panel.SetActive(true);

        panel.transform.position = position;

        useBTN.onClick.RemoveAllListeners();
        splitBTN.onClick.RemoveAllListeners();
        dropBTN.onClick.RemoveAllListeners();
        
        useBTN.onClick.AddListener(() =>
        { 
            slot.UseItem();
            Hide();
        });

        splitBTN.onClick.AddListener(() =>
        {
            slot.SplitItem();
            Hide();
        });

        dropBTN.onClick.AddListener(() =>
        {
            bool ctrl = Keyboard.current.leftCtrlKey.isPressed;
            if (ctrl)
            {
                slot.DropItem();
            }
            else
            {
                if (slot.slot.count > 1)
                {
                    InventoryUIManager.Instance.splitStackUI.Show(slot.slot.count, (amountChosen) =>
                    {
                        slot.DropItem(amountChosen);
                    });
                }
                else
                {
                    slot.DropItem();
                }
            }

            Hide();
        });
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentSlot = null;
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
