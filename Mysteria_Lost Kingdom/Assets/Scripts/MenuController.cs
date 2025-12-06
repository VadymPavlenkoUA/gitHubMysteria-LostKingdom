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
    public static PlayerInputActions Controls;
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

        //inputActions = new PlayerInputActions();
        if (Controls == null) Controls = new PlayerInputActions();
        inputActions = Controls;
    }
    private void OnEnable()
    {
        inputActions.Controls.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Controls.Disable();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (inputActions.Controls.Escape.WasPressedThisFrame())
        {
            HandleEscape();
        }

        if (inputBlocked) return;

        if (inputActions.Controls.MainPanel.WasPressedThisFrame())
        {
            OpenGameMenu();
        }
    }
    private void HandleEscape()
    {
        if (UseActionManager.Instance.isUsing)
        {
            UseActionManager.Instance.CancelUse();
        }
        if (isEduOpen)
        {
            return;
        }

        if (DialogueManager.Instance.isDialogueOpen)
        {
            DialogueManager.Instance.EndDialogue();
            return;
        }

        if (isGMOpen)
        {
            OpenGameMenu();
            return;
        }

        ToggleMainMenu(!isMMopen);
    }
    private void ToggleMainMenu(bool state)
    {
        isMMopen = state;

        if (state)
        {
            cinemachineInput.enabled = false;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
        else
        {
            cinemachineInput.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }

        mainMenu.SetActive(state);
        inputBlocked = state;
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
        if (isGMOpen) gameMenu.SetActive(false);
        inputActions.Player.Disable();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        educationMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideEducationMenu()
    {
        isEduOpen = false;
        if (isGMOpen) gameMenu.SetActive(true);
        inputActions.Player.Enable();
        if (!isGMOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        educationMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public bool IsInputBlocked()
    {
        return inputBlocked;
    }
}
