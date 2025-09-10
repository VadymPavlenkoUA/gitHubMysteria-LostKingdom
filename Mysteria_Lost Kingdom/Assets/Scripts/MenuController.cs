using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    public GameObject mainMenu;
    public GameObject gameMenu;
    private PlayerInputActions inputActions;
    private bool isMMopen = false;
    private bool isGMOpen = false;
    public bool inputBlocked = false;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        inputActions = new PlayerInputActions();
    }
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (inputActions.Player.Escape.WasPressedThisFrame())
        {
            isMMopen = !isMMopen;
            inputBlocked = isMMopen;
            if (isGMOpen) isGMOpen = false;

            gameMenu.SetActive(isGMOpen);
            mainMenu.SetActive(isMMopen);

            Time.timeScale = isMMopen ? 0f : 1f;
        }

        if (inputBlocked) return;

        if (inputActions.Player.MainPanel.WasPressedThisFrame())
        {
            isGMOpen = !isGMOpen;
            gameMenu.SetActive(isGMOpen);

            //Time.timeScale = isGMOpen ? 0f : 1f;
        }
    }
    public bool IsInputBlocked()
    {
        return inputBlocked;
    }
}
