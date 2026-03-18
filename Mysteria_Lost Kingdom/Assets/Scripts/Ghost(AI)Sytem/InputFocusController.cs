using UnityEngine;
using TMPro;

public class InputFocusController : MonoBehaviour
{
    public TMP_InputField inputField;
    public PlayerController playerController;
    public static InputFocusController Instance;
    void Awake()
    {
        Instance = this;
    }

    public void ForceEnablePlayerController()
    {
        playerController.enabled = true;
    }

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
