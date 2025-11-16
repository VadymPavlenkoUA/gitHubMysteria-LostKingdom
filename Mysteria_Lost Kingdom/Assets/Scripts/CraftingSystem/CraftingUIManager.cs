using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUIManager : MonoBehaviour
{
    public PlayerStats playerStats;
    public Inventory inventory;

    [Header("Profession Buttons")]
    public List<ProfessionButton> professionButtons;

    [Header("UI Elements")]
    public TextMeshProUGUI professionNameText;
    public TextMeshProUGUI levelText;
    public Slider expSlider;

    public Transform recipeListParent;
    public GameObject recipeButtonPrefab;

    public Transform ingredientListParent;
    public GameObject ingredientPrefab;

    public Button craftButton;

    private CraftingProfession currentProfession;
    private CraftingRecipe selectedRecipe;

    public List<CraftingRecipe> allRecipes;

    void Start()
    {
        craftButton.onClick.AddListener(Craft);
        foreach (var btn in professionButtons) btn.Setup(this);
        SelectProfession(0);

        inventory.OnInventoryChanged += OnInventoryChanged;
    }
    private void OnInventoryChanged()
    {
        PopulateIngredients();
        PopulateRecipes();
    }
    void RefreshProfessionButtons()
    {
        foreach (var btn in professionButtons)
            btn.Refresh();
    }

    public void SelectProfession(int profIndex)
    {
        selectedRecipe = null;
        currentProfession = (CraftingProfession)profIndex;
        UpdateProfessionPanel();
        PopulateRecipes();
        PopulateIngredients();
        RefreshProfessionButtons();
    }

    void UpdateProfessionPanel()
    {
        var p = playerStats.GetProfession(currentProfession);

        professionNameText.text = p.proffesionName;
        levelText.text = $"Ур.{p.level}";
        expSlider.maxValue = p.expToNext;
        expSlider.value = p.exp;
    }

    void PopulateRecipes()
    {
        ClearChildren(recipeListParent);

        foreach (var r in allRecipes)
        {
            if (r.profession == currentProfession)
            {
                var btn = Instantiate(recipeButtonPrefab, recipeListParent);
                btn.GetComponent<RecipeButton>().Setup(r, this);
            }
        }
    }

    public void SelectRecipe(CraftingRecipe r)
    {
        selectedRecipe = r;
        PopulateIngredients();
    }

    void PopulateIngredients()
    {
        ClearChildren(ingredientListParent);
        if (selectedRecipe == null) return;
        foreach (var ing in selectedRecipe.ingredients)
        {
            var obj = Instantiate(ingredientPrefab, ingredientListParent);
            obj.GetComponent<IngredientUI>().Setup(ing, inventory);
        }
    }

    void Craft()
    {
        if (selectedRecipe == null) return;

        foreach (var ing in selectedRecipe.ingredients)
        {
            if (!inventory.HasItem(ing.item, ing.amount))
            {
                Debug.Log("Не вистачає інгредієнтів");
                return;
            }
        }

        foreach (var ing in selectedRecipe.ingredients) inventory.RemoveItem(ing.item, ing.amount);

        inventory.AddItem(selectedRecipe.resultItem, selectedRecipe.resultAmount);

        playerStats.AddProfessionExp(currentProfession, selectedRecipe.expGained);

        UpdateProfessionPanel();
        PopulateIngredients();
        PopulateRecipes();
        RefreshProfessionButtons();
        InventoryUIManager.Instance.RefreshUI();
    }

    void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent) Destroy(child.gameObject);
    }
}
