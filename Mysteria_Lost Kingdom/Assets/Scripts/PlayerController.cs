using System.ComponentModel.Design;
using System.IO;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Windows;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private Rigidbody rb;
    internal Animator animator;
    public PlayerStats stats;
    public PlayerCombat combat;
    public PlayerAudio audioPlayer;

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

    [Header("Invulnerability")]
    [SerializeField] private bool isInvulnerable = false;
    public bool IsInvulnerable => isInvulnerable;

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
    private bool wantsToInterruptAttack;

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

        combat.OnBlockStarted += StopMovement;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMoveCanceled;

        inputActions.Player.FreeLook.performed -= ctx => isFreeLook = true;
        inputActions.Player.FreeLook.canceled -= ctx => isFreeLook = false;

        combat.OnBlockStarted -= StopMovement;

        inputActions?.Player.Disable();
    }


    private void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        if (combat != null && combat.isBlocking && input.sqrMagnitude > 0.01f)
        {
            combat.EndBlock();
        }

        moveInput = input;
    }

    void StopMovement()
    {
        moveInput = Vector2.zero;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        animator.SetFloat("Speed", 0f);
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }
    Surface GetSurfaceUnderPlayer()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.2f;

        if (Physics.Raycast(origin, Vector3.down, out hit, 1f, groundMask))
        {
            if (hit.collider.TryGetComponent<Terrain>(out _))
                return TerrainSurfaceUtility.GetSurface(hit.point);

            if (hit.collider.TryGetComponent(out SurfaceType st))
                return st.surface;
        }

        return Surface.Default;
    }

    public void OnFootstep()
    {
        if (!IsGrounded() || isRolling) return;
        if (moveInput.sqrMagnitude < 0.01f) return;
        Surface surface = GetSurfaceUnderPlayer();
        audioPlayer.PlaySurfaceSound(surface, isSprinting ? SurfaceAction.Sprint : SurfaceAction.Walk);
    }

    public void OnRolling()
    {
        Surface surface = GetSurfaceUnderPlayer();
        audioPlayer.PlaySurfaceSound(surface, SurfaceAction.Roll);
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

        if (!combat.CanMove())
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            moveDir = Vector3.zero;
            return;
        }
        //else if (wantsToInterruptAttack)
        //{
        //    wantsToInterruptAttack = false;
        //    combat.InterruptAttack();
        //    Debug.Log($"Trigger");
        //}
        
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

            rb.linearVelocity = new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z);

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
        animator.applyRootMotion = useRootMotionForRoll;
    }
    //public void RollRootMotion()
    //{
    //    animator.applyRootMotion = useRootMotionForRoll;
    //}

    public void RollEnd()
    {
        isRolling = false;
        rollDirection = Vector3.zero;

        rb.linearVelocity = Vector3.zero;

        EndInvulnerability();

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

        if (moveInput.sqrMagnitude > 0.01f)
        {
            combat.TryInterruptByMovement();
        }

        // JUMP
        if ((!combat.IsAttacking || combat.CanCancelAttack) && !isRolling && !combat.IsInStag && inputActions.Player.Jump.WasPressedThisFrame())
        {
            if (IsGrounded() && !isJumping)
            {
                if (stats != null && TryUseStaminaSafe(jumpStaminaCost))
                {
                    if (combat.isBlocking) combat.EndBlock();
                    if (combat.IsAttacking && combat.CanCancelAttack)
                    {
                        combat.TryInterruptForAction();
                        Debug.Log($"Trigger");
                    }
                    StartJump();
                }
            }
        }

        // ROLL
        if ((!combat.IsAttacking || combat.CanCancelAttack) && !isRolling && !combat.IsInStag)
        {
            if (inputActions.Player.Roll.WasPressedThisFrame())
            {
                if (IsGrounded() && rollCoolDownTimer <= 0f)
                {
                    if (TryUseStaminaSafe(rollStaminaCost))
                    {
                        if (combat.IsAttacking && combat.CanCancelAttack)
                        {
                            combat.TryInterruptForAction();
                            Debug.Log($"Trigger");
                        }
                        if (combat.isBlocking) combat.EndBlock();
                        StartRoll();
                    }
                }
            }
        }

        // SPRINT
        bool sprintPressed = inputActions.Player.Sprint.IsPressed();
        if (!combat.IsAttacking && !isRolling && sprintPressed && moveInput.sqrMagnitude > 0.01f && stats != null && stats.currentStamina > 0f)
        {
            if (combat.isBlocking) combat.EndBlock();
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

            if (combat != null && combat.equipment != null && !combat.IsAttacking && speedValue > 0.15f && !combat.equipment.isLeftHandDrawn && !combat.equipment.isRightHandDrawn)
            {
                combat.equipment.CancelForceCombatIdle();
            }

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
        Surface surface = GetSurfaceUnderPlayer();
        audioPlayer.PlaySurfaceSound(surface, SurfaceAction.Jump);

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
        Surface surface = GetSurfaceUnderPlayer();
        audioPlayer.PlaySurfaceSound(surface, SurfaceAction.Land);

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
    public void StartInvulnerability()
    {
        isInvulnerable = true;
        Debug.Log("INVUL START");
    }

    public void EndInvulnerability()
    {
        isInvulnerable = false;
        Debug.Log("INVUL END");
    }

}
