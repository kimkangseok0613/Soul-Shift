using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f;

    [Header("Vertical Detection")]
    [SerializeField] private float maxVerticalDifference = 1f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private GameObject attackHitbox;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    private Rigidbody2D rb;
    private Animator animator;
    private EnemyHealth enemyHealth;
    private Collider2D enemyCollider;

    private EnemyState currentState = EnemyState.Idle;

    private float attackCooldownTimer;

    // 공격 애니메이션 재생 중인지
    private bool isAttackPlaying;

    // 피격 애니메이션 재생 중인지
    private bool isHitPlaying;

    private Vector3 baseScale;

    // 원래 Rigidbody Constraints 저장
    private RigidbodyConstraints2D originalConstraints;

    // 공격 중 X축 고정 여부
    private bool isPositionXLocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        enemyCollider = GetComponent<Collider2D>();

        baseScale = transform.localScale;

        // 현재 Rigidbody Constraints 저장
        originalConstraints = rb.constraints;

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        // Player가 없으면 다시 찾음
        if (target == null)
        {
            FindPlayer();
        }

        // 사망 확인
        if (enemyHealth != null && enemyHealth.IsDead())
        {
            ChangeState(EnemyState.Dead);

            isAttackPlaying = false;
            isHitPlaying = false;

            DisableAttackHitbox();
            UnlockPositionX();

            return;
        }

        UpdateAttackCooldown();

        // 피격 중에는 상태 변경하지 않음
        if (!isHitPlaying)
        {
            UpdateState();
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (enemyHealth != null && enemyHealth.IsDead())
        {
            StopMovement();
            UnlockPositionX();
            return;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                UnlockPositionX();
                StopMovement();
                break;

            case EnemyState.Chase:
                UnlockPositionX();
                ChaseTarget();
                break;

            case EnemyState.Attack:
                // 공격 중에는 X축 고정
                LockPositionX();

                StopMovement();
                break;

            case EnemyState.Dead:
                UnlockPositionX();
                StopMovement();
                break;
        }
    }

    private void FindPlayer()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
    }

    private void UpdateState()
    {
        if (target == null)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        float horizontalDistance =
            Mathf.Abs(
                target.position.x -
                transform.position.x
            );

        float verticalDistance =
            Mathf.Abs(
                target.position.y -
                transform.position.y
            );

        bool verticalInRange =
            verticalDistance <= maxVerticalDifference;

        // 현재 공격 상태
        if (currentState == EnemyState.Attack)
        {
            // 공격 애니메이션이 끝날 때까지 유지
            if (isAttackPlaying)
                return;

            // 공격 범위 안
            if (verticalInRange &&
                horizontalDistance <= attackRange)
            {
                ChangeState(EnemyState.Attack);

                if (attackCooldownTimer <= 0f)
                {
                    Attack();
                }

                return;
            }

            // 공격 범위 밖 + 감지 범위 안
            if (verticalInRange &&
                horizontalDistance <= detectionRange)
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            // 감지 범위 밖
            ChangeState(EnemyState.Idle);
            return;
        }

        // 높이 차이가 너무 크면 추적하지 않음
        if (!verticalInRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 공격 범위
        if (horizontalDistance <= attackRange)
        {
            ChangeState(EnemyState.Attack);

            if (attackCooldownTimer <= 0f)
            {
                Attack();
            }

            return;
        }

        // 감지 범위
        if (horizontalDistance <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 감지 범위 밖
        ChangeState(EnemyState.Idle);
    }

    private void ChaseTarget()
    {
        if (target == null)
        {
            StopMovement();
            return;
        }

        float direction =
            Mathf.Sign(
                target.position.x -
                transform.position.x
            );

        if (Mathf.Approximately(
                target.position.x,
                transform.position.x))
        {
            StopMovement();
            return;
        }

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );

        Flip(direction);
    }

    private void Attack()
    {
        if (target == null)
            return;

        if (isAttackPlaying)
            return;

        if (isHitPlaying)
            return;

        isAttackPlaying = true;

        attackCooldownTimer = attackCooldown;

        // 공격 시작하면서 X축 고정
        LockPositionX();

        StopMovement();

        float direction =
            Mathf.Sign(
                target.position.x -
                transform.position.x
            );

        if (direction != 0f)
        {
            Flip(direction);
        }

        DisableAttackHitbox();

        animator.SetTrigger("Attack");
    }

    private void LockPositionX()
    {
        if (isPositionXLocked)
            return;

        rb.constraints =
            originalConstraints |
            RigidbodyConstraints2D.FreezePositionX;

        isPositionXLocked = true;
    }

    private void UnlockPositionX()
    {
        if (!isPositionXLocked)
            return;

        rb.constraints = originalConstraints;

        isPositionXLocked = false;
    }

    // Attack Animation Event
    public void EnableAttackHitbox()
    {
        if (enemyHealth != null &&
            enemyHealth.IsDead())
            return;

        if (isHitPlaying)
            return;

        if (attackHitbox == null)
            return;

        attackHitbox.SetActive(true);
    }

    // Attack Animation Event
    public void DisableAttackHitbox()
    {
        if (attackHitbox == null)
            return;

        attackHitbox.SetActive(false);
    }

    // Attack 마지막 Animation Event
    public void EndAttack()
    {
        DisableAttackHitbox();

        isAttackPlaying = false;

        // 공격 종료 후 X축 고정 해제
        UnlockPositionX();

        if (enemyHealth != null &&
            enemyHealth.IsDead())
        {
            ChangeState(EnemyState.Dead);
            return;
        }

        if (target == null)
        {
            FindPlayer();

            if (target == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }
        }

        float horizontalDistance =
            Mathf.Abs(
                target.position.x -
                transform.position.x
            );

        float verticalDistance =
            Mathf.Abs(
                target.position.y -
                transform.position.y
            );

        bool verticalInRange =
            verticalDistance <= maxVerticalDifference;

        // 공격 범위 안
        if (verticalInRange &&
            horizontalDistance <= attackRange)
        {
            ChangeState(EnemyState.Attack);

            if (attackCooldownTimer <= 0f)
            {
                Attack();
            }

            return;
        }

        // 공격 범위 밖 + 감지 범위 안
        if (verticalInRange &&
            horizontalDistance <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        ChangeState(EnemyState.Idle);
    }

    // 피격됐을 때 EnemyHealth에서 호출
    public void OnHit()
    {
        if (enemyHealth != null &&
            enemyHealth.IsDead())
            return;

        DisableAttackHitbox();

        isAttackPlaying = false;

        UnlockPositionX();

        isHitPlaying = true;

        StopMovement();

        animator.SetTrigger("Hit");
    }

    // Hit 마지막 Animation Event
    public void EndHit()
    {
        isHitPlaying = false;

        if (enemyHealth != null &&
            enemyHealth.IsDead())
        {
            ChangeState(EnemyState.Dead);
            return;
        }

        if (target == null)
        {
            FindPlayer();

            if (target == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }
        }

        float horizontalDistance =
            Mathf.Abs(
                target.position.x -
                transform.position.x
            );

        float verticalDistance =
            Mathf.Abs(
                target.position.y -
                transform.position.y
            );

        bool verticalInRange =
            verticalDistance <= maxVerticalDifference;

        // 공격 범위 안
        if (verticalInRange &&
            horizontalDistance <= attackRange)
        {
            ChangeState(EnemyState.Attack);

            if (attackCooldownTimer <= 0f)
            {
                Attack();
            }

            return;
        }

        // 공격 범위 밖 + 감지 범위 안
        if (verticalInRange &&
            horizontalDistance <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        ChangeState(EnemyState.Idle);
    }

    private void UpdateAttackCooldown()
    {
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;

            if (attackCooldownTimer < 0f)
            {
                attackCooldownTimer = 0f;
            }
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
    }

    private void StopMovement()
    {
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }

    private void Flip(float direction)
    {
        if (direction > 0f)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(baseScale.x),
                baseScale.y,
                baseScale.z
            );
        }
        else if (direction < 0f)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(baseScale.x),
                baseScale.y,
                baseScale.z
            );
        }
    }

    private void UpdateAnimation()
    {
        bool isMoving =
            currentState == EnemyState.Chase &&
            !isHitPlaying;

        animator.SetBool(
            "IsMoving",
            isMoving
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}