using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public enum CraftingStationType
{
    None,
    CookingPot,
    Anvil,
    Laboratory,
    MagicAltar
}

[CreateAssetMenu(menuName = "RPG/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public CraftingProfession profession;
    public Sprite craftIcon;
    public Item resultItem;
    public int resultAmount = 1;

    public float craftDuration;

    public List<Ingredient> ingredients;

    public CraftingStationType requiredStation = CraftingStationType.None;

    public int requiredLevel = 1;
    public float expGained = 10;
}

[System.Serializable]
public class Ingredient
{
    public Item item;
    public int amount;
    public Sprite ingrImage;
}
