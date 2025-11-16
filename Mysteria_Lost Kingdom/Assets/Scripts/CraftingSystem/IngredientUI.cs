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
    private int multiplier = 1;

    public void Setup(Ingredient ing, Inventory inv, int craftAmount = 1)
    {
        ingredient = ing;
        inventory = inv;
        multiplier = Mathf.Max(1, craftAmount);

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
    public void SetMultiplier(int craftAmount)
    {
        multiplier = Mathf.Max(1, craftAmount);
        Refresh();
    }

    public void Refresh()
    {
        if (ingredient == null || ingredient.item == null)
            return;

        int have = inventory != null ? inventory.GetItemCount(ingredient.item) : 0;
        int need = ingredient.amount * multiplier;

        countText.text = $"{have} / {need}";

        bool enough = have >= need;
        countText.color = enough ? Color.red : Color.darkRed;
    }
}
