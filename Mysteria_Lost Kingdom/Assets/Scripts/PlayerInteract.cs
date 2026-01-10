using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 2f;
    public float sphereCastRadius = 0.25f;
    public float autoSelectRadius = 2f;
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
        inputActions = MenuController.Controls;
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
        if (InteractionBlocker.IsBlocked)
        {
            currentTarget = null;
            promptUI.SetActive(false);
            return;
        }

        IInteractable bestTarget = null;
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, interactRange, interactableLayer))
        {
            bestTarget = hit.collider.GetComponent<IInteractable>();
        }

        if (bestTarget == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, autoSelectRadius, interactableLayer);

            float bestDist = Mathf.Infinity;

            foreach (var col in hits)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable == null) continue;

                Vector3 dir = (col.transform.position - transform.position).normalized;

                float dot = Vector3.Dot(transform.forward, dir);
                if (dot < 0.4f) continue;

                float dist = Vector3.Distance(transform.position, col.transform.position);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestTarget = interactable;
                }
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            promptNameText.text = bestTarget.GetInteractionNameText();
            promptBTNText.text = bestTarget.GetInteractionBTNText();
            promptUI.SetActive(true);
        }
        else
        {
            currentTarget = null;
            promptUI.SetActive(false);
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (InteractionBlocker.IsBlocked) return;

        if (currentTarget != null)
        {
            currentTarget.Interact();
            promptUI.SetActive(false);
        }
    }
}
