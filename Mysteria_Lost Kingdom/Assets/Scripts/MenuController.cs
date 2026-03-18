using Unity.Cinemachine;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    public GameObject mainMenu;
    public GameObject gameMenu;
    public GameObject educationMenu;
    public GameObject saveMenu;
    public AudioMixer mixer;
    public static PlayerInputActions Controls;
    internal PlayerInputActions inputActions;
    private bool isMMopen = false;
    internal bool isGMOpen = false;
    private bool isEduOpen = false;
    private bool isSavePanelOpen = false;
    public bool inputBlocked = false;
    [SerializeField] internal CinemachineInputAxisController cinemachineInput;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (Controls == null) Controls = new PlayerInputActions();

        inputActions = Controls;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        inputActions.Controls.Enable();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        inputActions?.Controls.Disable();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputBlocked = false;
        isMMopen = false;
        isGMOpen = false;
        isEduOpen = false;
        isSavePanelOpen = false;

        Time.timeScale = 1f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cinemachineInput = FindFirstObjectByType<CinemachineInputAxisController>();
    }

    bool IsGameplayScene()
    {
        return SceneManager.GetActiveScene().name == "MainScene";
    }


    // Update is called once per frame
    void Update()
    {
        if (!IsGameplayScene()) return;

        if (inputActions.Controls.Escape.WasPressedThisFrame())
        {
            HandleEscape();
        }

        if (inputBlocked) return;

        if (inputActions.Controls.CancelUse.WasPressedThisFrame())
        {
            if (UseActionManager.Instance.isUsing)
            {
                UseActionManager.Instance.CancelUse();
            }
        }

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
        if (TradeManager.Instance.isTradeOpen)
        {
            TradeManager.Instance.CloseTrade();
            return;
        }
        if (DialogueManager.Instance.isDialogueOpen)
        {
            DialogueManager.Instance.EndDialogue();
            return;
        }

        if (isSavePanelOpen)
        {
            isSavePanelOpen = false;
            saveMenu.SetActive(false);
            mainMenu.SetActive(true);
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
            if (cinemachineInput != null) cinemachineInput.enabled = false;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            Time.timeScale = 0f;
            inputActions.HotBar.Disable();
            inputActions.Combat.Disable();
            mixer.FindSnapshot("Paused").TransitionTo(0.2f);
        }
        else
        {
            if (cinemachineInput != null) cinemachineInput.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            inputActions.HotBar.Enable();
            inputActions.Combat.Enable();
            mixer.FindSnapshot("Gameplay").TransitionTo(0.2f);
        }

        mainMenu.SetActive(state);
        inputBlocked = state;
    }
    public void OpenGameMenu()
    {
        isGMOpen = !isGMOpen;
        if (isGMOpen)
        {
            if (TradeManager.Instance.isTradeOpen)
            {
                TradeManager.Instance.CloseTrade();
            }
            InteractionBlocker.Block(InteractionBlockReason.Menu);
            DialogueManager.Instance.EndDialogue();
            if (cinemachineInput != null) cinemachineInput.enabled = false;
            Cursor.lockState = CursorLockMode.Confined;
            inputActions.Combat.Disable();
        }
        else
        {
            if (InputFocusController.Instance != null)
            {
                InputFocusController.Instance.ForceEnablePlayerController();
            }
            InteractionBlocker.Unblock(InteractionBlockReason.Menu);
            if (cinemachineInput != null) cinemachineInput.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            if (CraftingUIManager.Instance != null)
            {
                CraftingUIManager.Instance.UpdateCloseCraftUI();
            }
            inputActions.Combat.Enable();
            InventoryUIManager.Instance.CloseChest();

        }
        Cursor.visible = isGMOpen;
        gameMenu.SetActive(isGMOpen);
        //Time.timeScale = isGMOpen ? 0f : 1f;
    }

    public void ShowEducationMenu()
    {
        isEduOpen = true;
        InteractionBlocker.Block(InteractionBlockReason.Education);
        if (isGMOpen) OpenGameMenu();
        inputActions.Player.Disable();
        inputActions.HotBar.Disable();
        inputActions.Combat.Disable();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        educationMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideEducationMenu()
    {
        isEduOpen = false;
        InteractionBlocker.Unblock(InteractionBlockReason.Education);
        //if (isGMOpen) gameMenu.SetActive(true);
        inputActions.Player.Enable();
        inputActions.HotBar.Enable();
        inputActions.Combat.Enable();
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

    public void ExitBTN()
    {
        SceneLoader.LoadScene("MainMenu");
    }

    public void ResumeBTN()
    {
        if (isMMopen) ToggleMainMenu(!isMMopen);
    }

    public void SaveBTN()
    {
        isSavePanelOpen = true;
        mainMenu.SetActive(false);
        saveMenu.SetActive(true);

    }

    public void ForceResumeGameState()
    {
        isMMopen = false;
        isGMOpen = false;
        isEduOpen = false;
        isSavePanelOpen = false;

        mainMenu.SetActive(false);
        gameMenu.SetActive(false);
        saveMenu.SetActive(false);
        educationMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        inputActions.Player.Enable();
        inputActions.HotBar.Enable();
        inputActions.Combat.Enable();

        mixer.FindSnapshot("Gameplay").TransitionTo(0.2f);
    }

}
