using UnityEngine;
using TMPro;

public class InputFocusController : MonoBehaviour
{
    public TMP_InputField inputField;
    public PlayerController playerController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputField.onSelect.AddListener(OnInputSelected);

        inputField.onDeselect.AddListener(OnInputDeselected);
    }

    private void OnInputSelected (string text)
    {
        playerController.enabled = false;
    }

    private void OnInputDeselected(string text)
    {
        playerController.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
