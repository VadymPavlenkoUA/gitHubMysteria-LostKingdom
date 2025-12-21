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
    public AnimationCurve rollSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
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
    private bool isJumping = false;
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
            float animNormalizedTime = 0f;

            if (animator != null)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                animNormalizedTime = Mathf.Clamp01(state.normalizedTime);
            }

            float speedMultiplier = rollSpeedCurve.Evaluate(animNormalizedTime);
            Vector3 targetVel = rollDirection * rollSpeed * speedMultiplier;

            rb.linearVelocity = new Vector3(targetVel.x, 0f, targetVel.z);

            if (rollDirection.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(rollDirection);
                rb.MoveRotation(
                    Quaternion.Slerp(rb.rotation, rot, 25f * Time.fixedDeltaTime)
                );
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
        rollCoolDownTimer = rollCoolDown;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        Vector3 dir = camForward.normalized * moveInput.y +
                      camRight.normalized * moveInput.x;

        rollDirection = dir.sqrMagnitude > 0.01f
            ? dir.normalized
            : camForward.normalized;

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        rb.MoveRotation(Quaternion.LookRotation(rollDirection));

        if (animator != null)
        {
            animator.SetTrigger("Roll");
        }
    }
    public void RollStart()
    {
        isRolling = true;
    }
    public void RollRootMotion()
    {
        animator.applyRootMotion = useRootMotionForRoll;
    }

    public void RollEnd()
    {
        isRolling = false;
        rollDirection = Vector3.zero;

        rb.linearVelocity = Vector3.zero;

        if (useRootMotionForRoll)
            animator.applyRootMotion = false;
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
            if (IsGrounded() && !isJumping)
            {
                if (stats != null && TryUseStaminaSafe(jumpStaminaCost))
                {
                    StartJump();
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

        //if (combat != null && combat.IsAttacking)
        //{
        //    Debug.Log("ATTACKING: " + animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
        //}
    }

    private void StartJump()
    {
        isJumping = true;

        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
    }

    public void OnJumpImpulse()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void OnLand()
    {
        isJumping = false;

        if (animator != null)
        {
            animator.ResetTrigger("Jump");
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
