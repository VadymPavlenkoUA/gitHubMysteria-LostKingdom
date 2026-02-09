using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    public TextMeshProUGUI slotText;
    public TextMeshProUGUI timeText;

    public Button slotButton;
    public Button saveButton;
    public Button loadButton;

    private int slot;
    private SaveSlotInfo currentInfo;

    public event Action<SaveSlotInfo> OnSlotClicked;

    public void Setup(SaveSlotInfo info)
    {
        slot = info.slot;
        currentInfo = info;

        slotText.text = $"Збереження {slot + 1}";
        UpdateUI(info);

        if (info.exists)
        {
            timeText.text = info.saveTime;
            loadButton.interactable = true;
        }
        else
        {
            timeText.text = "Пустий слот";
            loadButton.interactable = false;
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() =>
        {
            OnSlotClicked?.Invoke(currentInfo);
        });


        saveButton.onClick.RemoveAllListeners();
        saveButton.onClick.AddListener(() =>
        {
            SaveManager.Instance.SaveGame(slot);
        });

        loadButton.onClick.RemoveAllListeners();
        loadButton.onClick.AddListener(() => SceneLoader.LoadGameFromSave("MainScene", slot));
    }

    public void UpdateUI(SaveSlotInfo info)
    {
        currentInfo = info;

        if (info.exists)
        {
            timeText.text = info.saveTime;
            loadButton.interactable = true;
        }
        else
        {
            timeText.text = "Пустий слот";
            loadButton.interactable = false;
        }
    }
}
