using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfessionButton : MonoBehaviour
{
    public CraftingProfession profession;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;

    private CraftingUIManager uiManager;

    public void Setup(CraftingUIManager manager)
    {
        uiManager = manager;

        var p = manager.playerStats.GetProfession(profession);

        nameText.text = p.proffesionName;
        levelText.text = $"Ð³â.{p.level}";
    }

    public void Refresh()
    {
        var p = uiManager.playerStats.GetProfession(profession);
        levelText.text = $"Ð³â.{p.level}";
    }
}
    