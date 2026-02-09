using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveInfoPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [Header("Preview")]
    [SerializeField] private Image previewImage;
    [SerializeField] private Sprite emptyPreview;

    private void Awake()
    {
        Hide();
    }

    public void Show(string title, string body, Sprite preview)
    {
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
    }
}
