using UnityEngine;

public class EnemyAIController : MonoBehaviour
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

    void Awake()
    {
        spawnPosition = transform.position;

        movement = GetComponent<EnemyMovement>();
        combat = GetComponent<EnemyCombat>();
        stats = GetComponent<EnemyStats>();
        animator = stats.animator;
    }

    void Update()
    {
        if (stats.CurrentHealthNormalized <= 0)
        {
            ChangeState(State.Dead);
            enabled = false;

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
            movement.MoveTo(patrolTarget);
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

        isAttacking = true;
        combat.TryAttack(player);
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
        movement.Stop();
        combat.DisableHitbox();
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

}
