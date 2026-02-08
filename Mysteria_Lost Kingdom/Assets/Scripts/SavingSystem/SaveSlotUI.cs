using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public TextMeshProUGUI slotText;
    public TextMeshProUGUI timeText;
    public Button saveButton;
    public Button loadButton;

    private int slot;

    public void Setup(SaveSlotInfo info)
    {
        slot = info.slot;
        slotText.text = $"Slot {slot}";

        if (info.exists)
        {
            timeText.text = info.saveTime;
            loadButton.interactable = true;
        }
        else
        {
            timeText.text = "Empty";
            loadButton.interactable = false;
        }

        saveButton.onClick.AddListener(() => SaveManager.Instance.SaveGame(slot));

        loadButton.onClick.AddListener(() => SceneLoader.LoadGameFromSave("MainScene", slot));
    }
}
