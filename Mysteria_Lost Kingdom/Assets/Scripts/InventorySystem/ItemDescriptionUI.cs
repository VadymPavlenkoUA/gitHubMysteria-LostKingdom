using System.Collections;
using TMPro;
using UnityEngine;

public class ItemDescriptionUI : MonoBehaviour
{
    public static ItemDescriptionUI Instance;

    [Header("Panels")]
    [SerializeField] private RectTransform tradePanel;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.25f;
    [SerializeField] private float showXOffset = -220f;
    [SerializeField] private float hiddenXOffset = -220f;

    private Coroutine tradeAnim;

    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI descriptionTradeText;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDescription(string text, ItemInstance item, bool isTrade = false)
    {
        if (isTrade) ShowTradePanel();
        TextMeshProUGUI target = isTrade ? descriptionTradeText : descriptionText;
        string tradeLine = isTrade ? GetTradePriceLine(item.item) : "";

        if (item.item.categories == ItemCategory.Weapon)
        {
            target.text =
                 $"<b><size=130%><color=#d15508>{item.item.itemName}</color></size></b>\n\n" +
                 $"{text}\n\n" +
                 $"<color=#ff4c4c>Урон:</color> {item.currentDamage} | " +
                 $"<color=#4cff4c>Витривалість при атаці:</color> -{item.item.staminaCostPerAttack}\n" +
                 $"<color=#cfcfcf>Міцність:</color> {item.currentDurability} | " +
                 $"<color=#4ca6ff>Захист при блоці:</color> x{item.item.baseDefenseMultiplier}\n\n" +
                 $"<color=#e0b04f>Базова ціна:</color> {item.item.basePrice}" +
                 tradeLine;
            return;
        }

        if (item.item.categories == ItemCategory.Shield)
        {
            target.text =
                 $"<b><size=130%><color=#d15508>{item.item.itemName}</color></size></b>\n\n" +
                 $"{text}\n\n" +
                 $"<color=#4ca6ff>Сила блоку:</color> x{item.currentDefenseMultiplier} | " +
                 $"<color=#2563c4>Пасивний захист:</color> +{item.currentArmor} | " +
                 $"<color=#cfcfcf>Міцність:</color> {item.currentDurability}\n\n" +
                 $"<color=#e0b04f>Базова ціна:</color> {item.item.basePrice}" +
                 tradeLine;
            return;
        }

        if (item.item.categories == ItemCategory.ArmourHead || item.item.categories == ItemCategory.ArmourChest || item.item.categories == ItemCategory.ArmourGloves || 
            item.item.categories == ItemCategory.ArmourLegs || item.item.categories == ItemCategory.ArmourBoots || item.item.categories == ItemCategory.ArmourBelt)
        {
            target.text =
                 $"<b><size=130%><color=#d15508>{item.item.itemName}</color></size></b>\n\n" +
                 $"{text}\n\n" +
                 $"<color=#2563c4>Захист:</color> +{item.currentArmor} | " +
                 $"<color=#cfcfcf>Міцність:</color> {item.currentDurability}\n\n" +
                 $"<color=#e0b04f>Базова ціна:</color> {item.item.basePrice}" +
                 tradeLine;
            return;
        }

        if (item.item.categories == ItemCategory.Food)
        {
            target.text =
                 $"<b><size=130%><color=#d15508>{item.item.itemName}</color></size></b>\n\n" +
                 $"{text}\n\n" +
                 $"<color=#c92920>Здоров'я:</color> {item.item.healthRestore} | " +
                 $"<color=#a37837>Ситість:</color> {item.item.satietyRestore}\n" +
                 $"<color=#cfcfcf>Максимальний стак:</color> {item.item.maxStack}\n\n" +
                 $"<color=#e0b04f>Базова ціна:</color> {item.item.basePrice}" +
                 tradeLine;
            return;
        }

        if (item.item.categories == ItemCategory.Lockpick)
        {
            target.text =
                 $"<b><size=130%><color=#d15508>{item.item.itemName}</color></size></b>\n\n" +
                 $"{text}\n\n" +
                 $"<color=#77b52b>Максимальний рівень замка:</color> Звичайний\n" +
                 $"<color=#827560>Шанс поломки при успішному взломі:</color> 50%\n\n" +
                 $"<color=#e0b04f>Базова ціна:</color> {item.item.basePrice}" +
                 tradeLine;
            return;
        }
        if (!isTrade) descriptionText.text = text;
        else descriptionTradeText.text = text;
    }

    private string GetTradePriceLine(Item item)
    {
        var trade = TradeManager.Instance;
        if (trade == null || trade.CurrentMode == TradeMode.None) return "";

        int price = trade.GetItemTradePrice(item);

        if (trade.CurrentMode == TradeMode.Buy)
        {
            return $"\n<color=#ff6b6b>Вартість покупки:</color> <b>{price}</b>";
        }
        else if (trade.CurrentMode == TradeMode.Sell)
        {
            return $"\n<color=#6bff6b>Вартість продажу:</color> <b>{price}</b>";
        }

        return "";
    }


    public void ClearDescription(bool isTrade = false)
    {
        if (!isTrade) descriptionText.text = "";
        else
        {
            descriptionTradeText.text = "";
            HideTradePanel();
        }
    }

    private void ShowTradePanel()
    {
        if (tradeAnim != null) StopCoroutine(tradeAnim);
        tradeAnim = StartCoroutine(SlideTradePanel(showXOffset));
    }

    private void HideTradePanel()
    {
        if (tradeAnim != null) StopCoroutine(tradeAnim);
        tradeAnim = StartCoroutine(SlideTradePanel(hiddenXOffset));
    }

    private IEnumerator SlideTradePanel(float targetX)
    {
        float startX = tradePanel.anchoredPosition.x;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / slideDuration;
            float x = Mathf.Lerp(startX, targetX, Mathf.SmoothStep(0f, 1f, t));
            tradePanel.anchoredPosition = new Vector2(x, tradePanel.anchoredPosition.y);
            yield return null;
        }

        tradePanel.anchoredPosition = new Vector2(targetX, tradePanel.anchoredPosition.y);
    }

}
