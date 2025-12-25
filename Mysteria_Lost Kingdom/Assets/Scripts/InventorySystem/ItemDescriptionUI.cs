using TMPro;
using UnityEngine;

public class ItemDescriptionUI : MonoBehaviour
{
    public static ItemDescriptionUI Instance;

    public TextMeshProUGUI descriptionText;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDescription(string text, ItemInstance item)
    {
        if (item.item.categories == ItemCategory.Weapon)
        {
            descriptionText.text =
                 $"<b><size=130%><color=#d15508>{item.item.itemName}</color></size></b>\n\n" +
                 $"{text}\n\n" +
                 $"<color=#ff4c4c>Урон:</color> {item.currentDamage} | " +
                 $"<color=#4cff4c>Витривалість при атаці:</color> -{item.item.staminaCostPerAttack}\n" +
                 $"<color=#cfcfcf>Міцність:</color> {item.currentDurability} | " +
                 $"<color=#4ca6ff>Захист при блоці:</color> x{item.item.baseDefenseMultiplier}";
            return;
        }
        descriptionText.text = text;
    }

    public void ClearDescription()
    {
        descriptionText.text = "";
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
