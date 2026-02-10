using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SaveInfoPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [Header("Preview")]
    [SerializeField] private Image previewImage;
    [SerializeField] private Sprite emptyPreview;

    [Header("Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    private int currentSlot = -1;

    public event Action<int> OnSaveClicked;
    public event Action<int> OnLoadClicked;


    private void Awake()
    {
        Hide();

        saveButton.onClick.AddListener(() =>
        {
            if (currentSlot >= 0) OnSaveClicked?.Invoke(currentSlot);
        });

        loadButton.onClick.AddListener(() =>
        {
            if (currentSlot >= 0) OnLoadClicked?.Invoke(currentSlot);
        });
    }

    public void Show(string title, string body, Sprite preview, int slot, bool slotExists)
    {
        currentSlot = slot;

        titleText.text = title;
        bodyText.text = body;

        if (preview != null)
        {
            previewImage.sprite = preview;
            previewImage.gameObject.SetActive(true);
        }
        else
        {
            previewImage.sprite = emptyPreview;
            previewImage.gameObject.SetActive(emptyPreview != null);
        }

        loadButton.interactable = slotExists;

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    public void Clear()
    {
        titleText.text = "";
        bodyText.text = "";
        previewImage.sprite = emptyPreview;
        currentSlot = -1;
    }
}
