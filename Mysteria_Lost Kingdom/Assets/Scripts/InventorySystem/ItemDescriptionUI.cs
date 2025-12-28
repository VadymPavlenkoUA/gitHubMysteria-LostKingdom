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

        if (item.item.categories == ItemCategory.Shield)
        {
            descriptionText.text =
                 $"<b><size=130%><color=#d15508>{item.item.itemName}</color></size></b>\n\n" +
                 $"{text}\n\n" +
                 $"<color=#4ca6ff>Сила блоку:</color> x{item.currentDefenseMultiplier} | " +
                 $"<color=#2563c4>Пасивний захист:</color> +{item.currentArmor} | " +
                 $"<color=#cfcfcf>Міцність:</color> {item.currentDurability}";
            return;
        }

        if (item.item.categories == ItemCategory.ArmourHead || item.item.categories == ItemCategory.ArmourChest || item.item.categories == ItemCategory.ArmourGloves || 
            item.item.categories == ItemCategory.ArmourLegs || item.item.categories == ItemCategory.ArmourBoots || item.item.categories == ItemCategory.ArmourBelt)
        {
            descriptionText.text =
                 $"<b><size=130%><color=#d15508>{item.item.itemName}</color></size></b>\n\n" +
                 $"{text}\n\n" +
                 $"<color=#2563c4>Захист:</color> +{item.currentArmor} | " +
                 $"<color=#cfcfcf>Міцність:</color> {item.currentDurability}";
            return;
        }

        if (item.item.categories == ItemCategory.Food)
        {
            descriptionText.text =
                 $"<b><size=130%><color=#d15508>{item.item.itemName}</color></size></b>\n\n" +
                 $"{text}\n\n" +
                 $"<color=#c92920>Здоров'я:</color> {item.item.healthRestore} | " +
                 $"<color=#a37837>Ситість:</color> {item.item.satietyRestore}\n" +
                 $"<color=#cfcfcf>Максимальний стак:</color> {item.item.maxStack}";
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
