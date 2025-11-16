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

    public CraftQuantitySelector quantitySelector;

    private CraftingProfession currentProfession;
    private CraftingRecipe selectedRecipe;

    public List<CraftingRecipe> allRecipes;

    void Start()
    {
        craftButton.onClick.AddListener(Craft);
        foreach (var btn in professionButtons) btn.Setup(this);
        SelectProfession(0);

        inventory.OnInventoryChanged += OnInventoryChanged;
        quantitySelector.onQuantityChanged += UpdateIngredientsForQuantity;
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

    void UpdateIngredientsForQuantity(int newQuantity)
    {
        if (selectedRecipe == null) return;
        int quant = quantitySelector.quantity;

        foreach (Transform child in ingredientListParent)
        {
            IngredientUI ingUI = child.GetComponent<IngredientUI>();
            if (ingUI != null)
            {
                ingUI.SetMultiplier(quant);
            }
        }
    }


    public void SelectProfession(int profIndex)
    {
        selectedRecipe = null;
        currentProfession = (CraftingProfession)profIndex;
        quantitySelector.SetQuantity(1);
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
        quantitySelector.SetQuantity(1);
        PopulateIngredients();
    }

    void PopulateIngredients()
    {
        ClearChildren(ingredientListParent);
        if (selectedRecipe == null) return;

        int quant = quantitySelector.quantity;

        foreach (var ing in selectedRecipe.ingredients)
        {
            var obj = Instantiate(ingredientPrefab, ingredientListParent);
            obj.GetComponent<IngredientUI>().Setup(ing, inventory, quant);
        }
    }

    void Craft()
    {
        if (selectedRecipe == null) return;
        int craftAmount = quantitySelector.quantity;

        foreach (var ing in selectedRecipe.ingredients)
        {
            if (!inventory.HasItem(ing.item, ing.amount * craftAmount))
            {
                Debug.Log("Не вистачає інгредієнтів");
                return;
            }
        }

        foreach (var ing in selectedRecipe.ingredients) inventory.RemoveItem(ing.item, ing.amount * craftAmount);

        inventory.AddItem(selectedRecipe.resultItem, selectedRecipe.resultAmount * craftAmount);

        playerStats.AddProfessionExp(currentProfession, selectedRecipe.expGained * craftAmount);

        UpdateProfessionPanel();
        PopulateIngredients();
        PopulateRecipes();
        RefreshProfessionButtons();
        InventoryUIManager.Instance.RefreshUI();
        inventory.NotifyInventoryChanged();
    }

    void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent) Destroy(child.gameObject);
    }
}
