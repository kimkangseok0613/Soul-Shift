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

    private EnemyState currentState =
        EnemyState.Idle;

    private float attackCooldownTimer;

    private bool isAttackPlaying;
    private bool isHitPlaying;
    private bool isPositionXLocked;

    private Vector3 baseScale;

    private RigidbodyConstraints2D originalConstraints;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        baseScale = transform.localScale;
        originalConstraints = rb.constraints;

        DisableAttackHitbox();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (target == null)
        {
            FindPlayer();
        }

        if (IsDead())
        {
            SetDeadState();
            return;
        }

        UpdateAttackCooldown();

        if (!isHitPlaying)
        {
            UpdateState();
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (IsDead())
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

        target =
            player != null
                ? player.transform
                : null;
    }

    private void UpdateState()
    {
        if (!TryGetTargetDistance(
                out float horizontalDistance,
                out bool verticalInRange))
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 공격 애니메이션은 끝까지 유지
        if (currentState == EnemyState.Attack &&
            isAttackPlaying)
        {
            return;
        }

        if (!verticalInRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 공격 범위
        if (horizontalDistance <= attackRange)
        {
            ChangeState(EnemyState.Attack);

            if (!isAttackPlaying &&
                attackCooldownTimer <= 0f)
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

    private bool TryGetTargetDistance(
        out float horizontalDistance,
        out bool verticalInRange
    )
    {
        horizontalDistance = 0f;
        verticalInRange = false;

        if (target == null)
            return false;

        horizontalDistance =
            Mathf.Abs(
                target.position.x -
                transform.position.x
            );

        float verticalDistance =
            Mathf.Abs(
                target.position.y -
                transform.position.y
            );

        verticalInRange =
            verticalDistance <= maxVerticalDifference;

        return true;
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
        if (target == null ||
            isAttackPlaying ||
            isHitPlaying)
        {
            return;
        }

        isAttackPlaying = true;
        attackCooldownTimer = attackCooldown;

        LockPositionX();
        StopMovement();

        float direction =
            Mathf.Sign(
                target.position.x -
                transform.position.x
            );

        if (!Mathf.Approximately(
                direction,
                0f))
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

        rb.constraints =
            originalConstraints;

        isPositionXLocked = false;
    }

    public void EnableAttackHitbox()
    {
        if (IsDead() ||
            isHitPlaying ||
            attackHitbox == null)
        {
            return;
        }

        attackHitbox.SetActive(true);
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox == null)
            return;

        attackHitbox.SetActive(false);
    }

    public void EndAttack()
    {
        DisableAttackHitbox();

        isAttackPlaying = false;

        UnlockPositionX();

        ReevaluateTargetState();
    }

    public void OnHit()
    {
        if (IsDead())
            return;

        DisableAttackHitbox();

        isAttackPlaying = false;

        UnlockPositionX();

        isHitPlaying = true;

        StopMovement();

        animator.SetTrigger("Hit");
    }

    public void EndHit()
    {
        isHitPlaying = false;

        ReevaluateTargetState();
    }

    private void ReevaluateTargetState()
    {
        if (target == null)
        {
            FindPlayer();
        }

        if (IsDead())
        {
            SetDeadState();
            return;
        }

        UpdateState();
    }

    private void UpdateAttackCooldown()
    {
        if (attackCooldownTimer <= 0f)
            return;

        attackCooldownTimer -=
            Time.deltaTime;

        if (attackCooldownTimer < 0f)
        {
            attackCooldownTimer = 0f;
        }
    }

    private void SetDeadState()
    {
        ChangeState(EnemyState.Dead);

        isAttackPlaying = false;
        isHitPlaying = false;

        DisableAttackHitbox();
        UnlockPositionX();
    }

    private bool IsDead()
    {
        return enemyHealth != null &&
               enemyHealth.IsDead();
    }

    private void ChangeState(
        EnemyState newState
    )
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
            SetFacing(1f);
        }
        else if (direction < 0f)
        {
            SetFacing(-1f);
        }
    }

    private void SetFacing(float direction)
    {
        float sign = Mathf.Sign(direction);

        transform.localScale = new Vector3(
            Mathf.Abs(baseScale.x) * sign,
            baseScale.y,
            baseScale.z
        );
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