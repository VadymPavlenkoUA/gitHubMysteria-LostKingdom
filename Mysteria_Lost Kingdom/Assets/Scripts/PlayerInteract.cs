using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 2f;
    public LayerMask interactableLayer;
    public GameObject promptUI;
    public TMP_Text promptNameText;
    public TMP_Text promptBTNText;

    private IInteractable currentTarget;
    private PlayerInputActions inputActions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        promptUI.SetActive(false);
    }

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Interact.performed += OnInteract;
    }

    private void OnDestroy()
    {
        inputActions.Player.Interact.performed -= OnInteract;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentTarget = interactable;
                promptNameText.text = $"{interactable.GetInteractionNameText()}";
                promptBTNText.text = $"{interactable.GetInteractionBTNText()}";
                promptUI.SetActive(true);
                return;
            }
        }
        currentTarget = null;
        promptUI.SetActive(false);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (currentTarget != null)
        {
            currentTarget.Interact();
            promptUI.SetActive(false);
        }
    }
}
