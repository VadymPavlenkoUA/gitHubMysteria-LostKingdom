using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultipleChoiceRenderer : MonoBehaviour
{
    public RectTransform container;
    public Button choiceButtonPrefab;

    private List<Button> spawned = new();
    private string selected = "";

    public void Render(List<string> choices)
    {
        Clear();
        if (choices == null || choices.Count == 0) return;
        foreach (var choice in choices)
        {
            var btn = Instantiate(choiceButtonPrefab, container);
            btn.GetComponentInChildren<TMP_Text>().text = choice;

            btn.onClick.AddListener(() =>
            {
                selected = choice;
                Highlight(btn);
            });

            spawned.Add(btn);
        }
    }

    private void Highlight(Button active)
    {
        foreach (var b in spawned)
        {
            var img = b.GetComponent<Image>();
            img.color = (b == active) ? new Color(0.7f, 1f, 0.7f) : Color.white;
        }
    }

    public string GetAnswer() => selected;

    public void Clear()
    {
        selected = "";
        foreach (var b in spawned) Destroy(b.gameObject);
        spawned.Clear();
    }

    public void SetInteractable(bool v)
    {
        foreach (var b in spawned) b.interactable = v;
    }
}
