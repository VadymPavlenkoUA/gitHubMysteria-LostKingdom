using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeButton : MonoBehaviour
{
    [Header("UI References")]
    public Image icon;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI levelText;

    private Button button;

    private CraftingRecipe recipe;
    private CraftingUIManager manager;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    public void Setup(CraftingRecipe r, CraftingUIManager mgr)
    {
        recipe = r;
        manager = mgr;

        titleText.text = r.resultItem != null
            ? r.resultItem.itemName
            : "Без назви";

        if (icon != null)
            icon.sprite = r.craftIcon != null ? r.craftIcon : null;

        if (icon != null)
            icon.enabled = (r.craftIcon != null && r.craftIcon != null);

        if (levelText != null)
            levelText.text = r.requiredLevel > 0
                ? $"Рів. {r.requiredLevel}"
                : "";
    }

    void OnClick()
    {
        if (manager != null && recipe != null)
        {
            manager.SelectRecipe(recipe);
            manager.UpdateRecipeUI(recipe);
        }

    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }
}
