using Unity.Cinemachine;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    public GameObject mainMenu;
    public GameObject gameMenu;
    public GameObject educationMenu;
    private PlayerInputActions inputActions;
    private bool isMMopen = false;
    private bool isGMOpen = false;
    private bool isEduOpen = false;
    public bool inputBlocked = false;
    [SerializeField] private CinemachineInputAxisController cinemachineInput;
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
            if (isMMopen)
            {
                cinemachineInput.enabled = false;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                cinemachineInput.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
            }
            Cursor.visible = isMMopen;
            inputBlocked = isMMopen;
            if (isGMOpen) isGMOpen = false;

            gameMenu.SetActive(isGMOpen);
            mainMenu.SetActive(isMMopen);

            Time.timeScale = isMMopen ? 0f : 1f;
        }

        if (inputBlocked) return;

        if (inputActions.Player.MainPanel.WasPressedThisFrame())
        {
            OpenGameMenu();
        }
    }
    public void OpenGameMenu()
    {
        isGMOpen = !isGMOpen;
        if (isGMOpen)
        {
            cinemachineInput.enabled = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            cinemachineInput.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            if (CraftingUIManager.Instance != null)
            {
                CraftingUIManager.Instance.UpdateCloseCraftUI();
            }
        }
        Cursor.visible = isGMOpen;
        gameMenu.SetActive(isGMOpen);
        //Time.timeScale = isGMOpen ? 0f : 1f;
    }

    public void ShowEducationMenu()
    {
        isEduOpen = true;

        cinemachineInput.enabled = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        educationMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideEducationMenu()
    {
        isEduOpen = false;

        cinemachineInput.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        educationMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public bool IsInputBlocked()
    {
        return inputBlocked;
    }
}
