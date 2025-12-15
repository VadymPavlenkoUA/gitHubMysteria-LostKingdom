using System.ComponentModel.Design;
using System.IO;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private Rigidbody rb;
    private Animator animator;
    public PlayerStats stats;
    public PlayerCombat combat;

    private Vector2 moveInput;
    private bool isFreeLook = false;

    [Header("Movement")]
    public float moveSpeed = 1f;
    public float sprintMultiplier = 1.8f;
    public float sprintStaminaDrain = 12f;

    [Header("Jump")]
    public float jumpForce = 3f;
    public float jumpStaminaCost = 25f;

    [Header("Roll")]
    public float rollSpeed = 7f;
    public float rollDuration = 0.6f;
    public float rollStaminaCost = 20f;
    public float rollCoolDown = 0.5f;
    public float rollInvulTime = 0.35f; // час "ірвінгі" (без урону)

    [Header("Ground Check")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundMask = ~0;

    [Header("Debug")]
    public bool useRootMotionForRoll = false; // для root motion

    private bool isSprinting = false;
    private bool isRolling = false;
    private float rollTimer = 0f;
    private float rollCoolDownTimer = 0f;
    private Vector3 rollDirection = Vector3.zero;

    public Transform cameraTransform;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        stats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMoveCanceled;

        inputActions.Player.FreeLook.performed += ctx => isFreeLook = true;
        inputActions.Player.FreeLook.canceled += ctx => isFreeLook = false;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMoveCanceled;

        inputActions.Player.FreeLook.performed -= ctx => isFreeLook = true;
        inputActions.Player.FreeLook.canceled -= ctx => isFreeLook = false;

        inputActions?.Player.Disable();
    }


    private void OnMove(InputAction.CallbackContext ctx)
    {
        //if (combat != null && combat.IsAttacking)
        //{
        //    moveInput = Vector2.zero;
        //    return;
        //}

        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void FixedUpdate()
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * moveInput.y + camRight * moveInput.x;

        if (combat != null && combat.IsAttacking)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            moveDir = Vector3.zero; // рух не враховуємо
        }

        float currentSpeed = moveSpeed;

        if (isRolling)
        {
            Vector3 vel = rb.linearVelocity; // !!!
            Vector3 target = rollDirection * rollSpeed;
            rb.linearVelocity = new Vector3(target.x, vel.y, target.z);

            if (rollDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(rollDirection);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 20f * Time.fixedDeltaTime));
            }
            return;
        }

        if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;
        }

        Vector3 move = moveDir * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        if (!isFreeLook)
        {
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
            }
            else
            {
                Vector3 camForwardFlat = cameraTransform.forward;
                camForwardFlat.y = 0f;
                if (camForwardFlat.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(camForwardFlat);
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
                }
            }
        }
        else
        {
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
            }
        }
    }

    private void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;
        rollCoolDownTimer = rollCoolDown;

        //Vector3 horizontalMove = new Vector3(moveInput.x, 0f, moveInput.y);
        //Vector3 camForward = cameraTransform.forward; camForward.y = 0f; camForward.Normalize();
        //Vector3 camRight = cameraTransform.right; camRight.y = 0f; camRight.Normalize();
        //Vector3 moveDirWorld = camForward * moveInput.y + camRight * moveInput.x;

        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDirWorld = camForward * moveInput.y + camRight * moveInput.x;

        if (moveDirWorld.sqrMagnitude > 0.05f)
        {
            rollDirection = moveDirWorld.normalized;
        }
        else
        {
            rollDirection = camForward;
        }

        Quaternion targetRot = Quaternion.LookRotation(rollDirection);
        rb.MoveRotation(targetRot);

        if (animator != null)
        {
            animator.SetTrigger("Roll");
            animator.applyRootMotion = useRootMotionForRoll;
        }
    }

    private void EndRoll()
    {
        isRolling = false;
        rollDirection = Vector3.zero;
        if (animator != null)
        {
            if (useRootMotionForRoll) animator.applyRootMotion = false;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.None;
    }

    // Update is called once per frame
    private void Update()
    {
        if (MenuController.Instance.IsInputBlocked()) return;

        // JUMP
        if (!combat.IsAttacking && inputActions.Player.Jump.WasPressedThisFrame())
        {
            if (IsGrounded())
            {
                if (stats != null && TryUseStaminaSafe(jumpStaminaCost))
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    if (animator != null) animator.SetTrigger("Jump");
                }
            }
        }

        // ROLL
        if (!combat.IsAttacking && !isRolling)
        {
            if (inputActions.Player.Roll.WasPressedThisFrame())
            {
                if (IsGrounded() && rollCoolDownTimer <= 0f)
                {
                    if (TryUseStaminaSafe(rollStaminaCost))
                    {
                        StartRoll();
                    }
                }
            }
        }

        // SPRINT
        bool sprintPressed = inputActions.Player.Sprint.IsPressed();
        if (!combat.IsAttacking && !isRolling && sprintPressed && moveInput.sqrMagnitude > 0.01f && stats != null && stats.currentStamina > 0f)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        if (animator != null)
        {
            float baseSpeed = moveInput.magnitude;       
            float targetSpeed = isSprinting ? 1f : 0.4f * baseSpeed; 

            // Плавне згладжування від поточного значення Speed до targetSpeed
            float speedValue = Mathf.Lerp(animator.GetFloat("Speed"), targetSpeed, 5f * Time.deltaTime);

            animator.SetFloat("Speed", speedValue);
        }

        if (rollCoolDownTimer > 0f) rollCoolDownTimer -= Time.deltaTime;
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f)
            {
                EndRoll();
            }
        }

        if (isSprinting)
        {
            float drain = sprintStaminaDrain * Time.deltaTime;
            if (stats != null)
            {
                stats.UseStamina(drain);
                if (stats.currentStamina <= 0f)
                {
                    isSprinting = false;
                }
            }
        }

        if (combat != null && combat.IsAttacking)
        {
            Debug.Log("ATTACKING: " + animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
        }
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.05f, groundMask);
    }

    private bool TryUseStaminaSafe(float amount)
    {
        if (stats == null) return false;
        return stats.TryUseStamina(amount);
    }
}
