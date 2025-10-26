using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ControlsMenuManager : MonoBehaviour
{
    [Header("Player Input Asset")]
    public InputActionAsset actions;

    [Header("UI Elements")]
    public Transform panel;
    public GameObject actionItemPrefab;

    private List<GameObject> instantiatedItems = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulateControls();
    }

    private void PopulateControls()
    {
        ClearControls();
        foreach (var map in actions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                if (action.name == "Look" || action.name == "Move") continue;
                GameObject item = Instantiate(actionItemPrefab, panel);
                instantiatedItems.Add(item);
                TMP_Text nameText = item.transform.Find("ControlName").GetComponent<TMP_Text>();
                Button rebindButton = item.transform.Find("ControlBTN").GetComponent<Button>();
                TMP_Text bindingText = rebindButton.GetComponentInChildren<TMP_Text>();

                nameText.text = action.name;
                string displayName = action.bindings[0].ToDisplayString();
                bindingText.text = displayName.ToUpper();

                int bindingIndex = 0;
                rebindButton.onClick.AddListener(() => StartRebind(action, bindingIndex, bindingText));
            }
        }
    }

    private void ClearControls()
    {
        foreach (var item in instantiatedItems) Destroy(item);
        instantiatedItems.Clear();
    }

    private void StartRebind(InputAction action, int bindingIndex, TMP_Text bindingText)
    {
        action.Disable();
        var rebind = action.PerformInteractiveRebinding(bindingIndex).WithControlsExcluding("Position").OnComplete(operation =>
        {
            string displayName = action.bindings[bindingIndex].ToDisplayString();
            bindingText.text = displayName.ToUpper();
            operation.Dispose();
            action.Enable();
        });
        rebind.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
