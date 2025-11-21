using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUIManager : MonoBehaviour
{
    public static CraftingUIManager Instance;

    public PlayerStats playerStats;
    public Inventory inventory;

    [Header("Profession Buttons")]
    public List<ProfessionButton> professionButtons;

    [Header("UI Elements")]
    public TextMeshProUGUI professionNameText;
    public TextMeshProUGUI levelText;
    public Slider expSlider;
    public GameObject craftingTableGroup;
    public Image craftingTableImage;

    public Transform recipeListParent;
    public GameObject recipeButtonPrefab;

    public Transform ingredientListParent;
    public GameObject ingredientPrefab;

    public Button craftButton;

    public CraftQuantitySelector quantitySelector;

    private CraftingProfession currentProfession;
    private CraftingRecipe selectedRecipe;

    public List<CraftingRecipe> allRecipes;

    public Sprite cookingPotSprite;
    public Sprite anvilSprite;
    public Sprite workbenchSprite;
    public Sprite magicAltarSprite;
    public Sprite defaultStationSprite;

    public Color baseColor = Color.white;
    public Color wrongColor = Color.red;

    public TabSwitcher tabSwitcher;
    public int craftingTabIndex;

    [HideInInspector]
    public CraftingStationType activeStation = CraftingStationType.None;

    void Awake()
    {
        Instance = this;
    }

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

    public void OpenFromStation(CraftingStationType station)
    {
        activeStation = station;
        MenuController.Instance.OpenGameMenu();
        PopulateRecipes();
        UpdateRecipeUI(selectedRecipe);
        if (tabSwitcher != null) tabSwitcher.OpenTab(craftingTabIndex);
    }

    public void UpdateCloseCraftUI()
    {
        activeStation = CraftingStationType.None;
        UpdateRecipeUI(selectedRecipe);
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

    public void UpdateRecipeUI(CraftingRecipe recipe)
    {
        if (recipe == null) return;
        craftingTableImage.sprite = recipe.craftIcon;

        if (recipe.requiredStation == CraftingStationType.None)
        {
            craftingTableGroup.SetActive(false);
        }
        else
        {
            craftingTableGroup.SetActive(true);
            craftingTableImage.sprite = GetStationSprite(recipe.requiredStation);
            bool isCorrectStation = activeStation == recipe.requiredStation;
            craftingTableImage.color = isCorrectStation ? baseColor : wrongColor;
        }
    }

    public Sprite GetStationSprite(CraftingStationType station)
    {
        switch (station)
        {
            case CraftingStationType.CookingPot: return cookingPotSprite ?? defaultStationSprite;
            case CraftingStationType.Anvil: return anvilSprite ?? defaultStationSprite;
            case CraftingStationType.Laboratory: return workbenchSprite ?? defaultStationSprite;
            case CraftingStationType.MagicAltar: return magicAltarSprite ?? defaultStationSprite;
            default: return defaultStationSprite;
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
        levelText.text = $"Рів.{p.level}";
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
        if (selectedRecipe == null)
        {
            craftingTableGroup.SetActive(false);
            return;
        }

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
        if (selectedRecipe.requiredStation != CraftingStationType.None && selectedRecipe.requiredStation != activeStation) return;
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
