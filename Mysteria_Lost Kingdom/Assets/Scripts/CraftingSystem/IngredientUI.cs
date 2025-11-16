using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;

    private Ingredient ingredient;
    private Inventory inventory;

    public void Setup(Ingredient ing, Inventory inv)
    {
        ingredient = ing;
        inventory = inv;

        if (ingredient == null || ingredient.item == null)
        {
            nameText.text = "???";
            countText.text = "";

            if (icon != null)
            {
                icon.sprite = null;
                icon.enabled = false;
            }
            return;
        }

        nameText.text = ingredient.item.itemName;

        if (icon != null)
        {
            if (ingredient.ingrImage != null)
            {
                icon.sprite = ingredient.ingrImage;
            }
            else
            {
                icon.sprite = ingredient.item.icon;
                icon.enabled = ingredient.item.icon != null;
            }
        }

        Refresh();
    }

    public void Refresh()
    {
        if (ingredient == null || ingredient.item == null)
            return;

        int have = inventory != null ? inventory.GetItemCount(ingredient.item) : 0;

        countText.text = $"{have} / {ingredient.amount}";

        bool enough = have >= ingredient.amount;
        countText.color = enough ? Color.red : Color.darkRed;
    }
}
