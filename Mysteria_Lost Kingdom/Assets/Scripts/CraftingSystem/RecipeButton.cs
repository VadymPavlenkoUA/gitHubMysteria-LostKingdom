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
        if (button != null) button.onClick.AddListener(OnClick);
    }

    public void Setup(CraftingRecipe recipe, CraftingUIManager manager)
    {
        if (button == null) button = GetComponentInChildren<Button>();
        this.recipe = recipe;
        this.manager = manager;

        titleText.text = recipe.resultItem != null
            ? recipe.resultItem.itemName
            : "Без назви";

        if (icon != null)
        {
            icon.sprite = recipe.craftIcon;
            icon.enabled = recipe.craftIcon != null;
        }

        var profession = manager.playerStats.GetProfession(recipe.profession);

        int playerLevel = profession != null ? profession.level : 0;

        bool levelOk = playerLevel >= recipe.requiredLevel;

        if (button == null)
        {
            Debug.LogError("Button is NULL on " + gameObject.name, this);
        }
        button.interactable = levelOk;

        if (levelText != null)
            levelText.text = recipe.requiredLevel > 0
                ? $"Рів. {recipe.requiredLevel}"
                : "";

        // Текст вимоги рівня
        //if (recipe.requiredLevel > 0 && !levelOk)
        //{
        //    levelRequirementText.gameObject.SetActive(true);
        //    levelRequirementText.text = $"Рівень {recipe.requiredLevel}";
        //}
        //else
        //{
        //    levelRequirementText.gameObject.SetActive(false);
        //}
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
