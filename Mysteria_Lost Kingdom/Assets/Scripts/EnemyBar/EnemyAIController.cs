using UnityEngine;

public class EnemyAIController : MonoBehaviour, ISaveable
{
    public enum State
    {
        Idle,
        Chase,
        Attack,
        Return,
        Dead
    }

    public State currentState;

    [Header("Distances")]
    public float viewRadius = 10f;
    public float attackRange = 2f;
    public float loseAggroDistance = 15f;

    [Header("Aggro")]
    public float loseAggroTime = 3f;

    [Header("Patrol Settings")]
    public float patrolRadius = 5f;          // радіус навколо спавну
    public float patrolWaitTime = 20f;        // скільки стоїть на місці
    private Vector3 patrolTarget;
    private bool isPatrolling;
    private float patrolTimer;
    public float patrolSpeedMultiplier = 0.5f;

    public LayerMask playerLayer;

    private Transform player;
    private EnemyMovement movement;
    private EnemyCombat combat;
    private EnemyStats stats;
    private Animator animator;

    private float aggroTimer;
    private bool isAttacking;
    private bool isHit;

    private Vector3 spawnPosition;

    private SaveableEntity saveableEntity;

    void Awake()
    {
        spawnPosition = transform.position;

        movement = GetComponent<EnemyMovement>();
        combat = GetComponent<EnemyCombat>();
        stats = GetComponent<EnemyStats>();
        animator = stats.animator;

        saveableEntity = GetComponent<SaveableEntity>();
        if (saveableEntity == null)
        {
            Debug.LogError($"[EnemyAI] Missing SaveableEntity on {gameObject.name}");
        }
    }

    public string GetSaveID() => saveableEntity.ID;

    void Update()
    {
        //if (stats.CurrentHealthNormalized <= 0)
        //{
        //    ChangeState(State.Dead);
        //    enabled = false;

        //    return;
        //}

        if (stats.IsDead)
        {
            HandleDeadState();
            return;
        }

        if (isHit || isAttacking) return;

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.Attack:
                UpdateAttack();
                break;
            case State.Return:
                UpdateReturn();
                break;
        }
    }

    // ================= STATES =================

    void UpdateIdle()
    {
        animator.SetBool("Walk", false);

        if (TryFindPlayer())
        {
            ChangeState(State.Chase);
        }

        PatrolAroundSpawn();
    }

    void PatrolAroundSpawn()
    {
        // Якщо немає поточної точки, обираємо нову
        if (!isPatrolling)
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            patrolTarget = spawnPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            isPatrolling = true;
            patrolTimer = 0f; // скидаємо таймер очікування
        }

        float dist = Vector3.Distance(transform.position, patrolTarget);

        if (dist > 0.2f)
        {
            // Рухаємось до точки
            movement.MoveTo(patrolTarget, patrolSpeedMultiplier);
            animator.SetBool("Walk", true);
        }
        else
        {
            // Досягли точки — стоїмо
            movement.Stop();
            animator.SetBool("Walk", false);

            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime)
            {
                isPatrolling = false; // обрати нову точку
            }
        }
    }


    void UpdateReturn()
    {
        if (TryFindPlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        float dist = Vector3.Distance(transform.position, spawnPosition);

        if (dist <= 0.3f)
        {
            movement.Stop();
            ChangeState(State.Idle);
            return;
        }

        movement.MoveTo(spawnPosition);
        animator.SetBool("Walk", true);
    }


    void UpdateChase()
    {
        if (!player)
        {
            if (!TryFindPlayer())
            {
                LoseAggro();
                return;
            }
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            ChangeState(State.Attack);
            return;
        }

        if (dist > loseAggroDistance)
        {
            LoseAggro();
            return;
        }

        movement.MoveTo(player.position);
        animator.SetBool("Walk", true);
    }

    void UpdateAttack()
    {
        if (!player)
        {
            ChangeState(State.Idle);
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > attackRange)
        {
            ChangeState(State.Chase);
            return;
        }

        movement.Stop();
        animator.SetBool("Walk", false);

        if (!isAttacking)
        {
            isAttacking = true;
            combat.TryAttack(player);
        }
    }

    // ================= HELPERS =================

    bool TryFindPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius, playerLayer);
        if (hits.Length == 0) return false;

        player = hits[0].transform;
        aggroTimer = 0f;
        return true;
    }

    void LoseAggro()
    {
        movement.Stop(); 
        animator.SetBool("Walk", false);
        aggroTimer += Time.deltaTime;

        if (aggroTimer >= loseAggroTime)
        {
            player = null;
            aggroTimer = 0f;
            ChangeState(State.Return);
        }
    }


    void ChangeState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (currentState)
        {
            case State.Idle:
                animator.SetBool("Walk", false);
                break;

            case State.Chase:
                animator.SetBool("Walk", true);
                break;

            case State.Return:
                animator.SetBool("Walk", true);
                break;

            case State.Attack:
                animator.SetBool("Walk", false);
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Attack");
                break;
        }
    }

    // ================= ANIMATION EVENTS =================

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    public void OnHitStart()
    {
        isHit = true;
        isAttacking = false;

        movement.Stop();

        combat.DisableHitbox();  
        animator.ResetTrigger("Attack");
    }

    public void OnHitEnd()
    {
        isHit = false;
        isAttacking = false;

        animator.Play("Idle", 0, 0f);

        if (player != null)
        {
            ChangeState(State.Chase);
        }
        else
        {
            ChangeState(State.Idle);
        }
    }

    public object CaptureState()
    {
        return new EnemySaveData
        {
            uniqueID = GetComponent<SaveableEntity>().ID,
            position = transform.position,
            rotation = transform.rotation,
            currentState = currentState,
            patrolTarget = patrolTarget,
            isPatrolling = isPatrolling,
            patrolTimer = patrolTimer,
            currentHealth = stats.currentHealth,
            lootDropped = stats.CaptureLootState()
        };
    }

    public void RestoreState(object state)
    {
        if (state is not EnemySaveData data) return;

        gameObject.SetActive(true);

        animator.enabled = true;
        animator.Rebind();
        animator.Update(0f);

        transform.position = data.position;
        transform.rotation = data.rotation;

        patrolTarget = data.patrolTarget;
        isPatrolling = data.isPatrolling;
        patrolTimer = data.patrolTimer;

        isAttacking = false;
        isHit = false;

        animator.enabled = true;
        animator.Rebind();
        animator.Update(0f);

        stats.RestoreLootState(data.lootDropped);

        stats.ShowHealth();
        stats.SetHealth(data.currentHealth);

        if (data.currentHealth <= 0f)
        {
            stats.HideHealth();
            HandleDeadState();
            return;
        }

        State restoredState = data.currentState;

        if (restoredState == State.Attack || restoredState == State.Chase)
        {
            restoredState = State.Idle;
        }

        currentState = restoredState;

        animator.Play("Idle", 0, 0f);

        enabled = true;
    }

    void HandleDeadState()
    {
        if (currentState == State.Dead) return;

        currentState = State.Dead;

        movement.Stop();
        combat.DisableHitbox();

        animator.Rebind();
        animator.Update(0f);
        animator.Play("Die", 0, 1f);
        animator.Update(0f);

        enabled = false;
    }


}
