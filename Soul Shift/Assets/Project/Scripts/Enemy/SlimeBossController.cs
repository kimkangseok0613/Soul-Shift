using UnityEngine;

public class SlimeBossController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float maxVerticalDifference = 2f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackFailsafeTime = 2f;
    [SerializeField] private float hitFailsafeTime = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D bodyCollider;

    private Transform player;

    private float attackCooldownTimer;
    private float actionTimer;
    private float fixedY;

    private bool isActionLocked;
    private bool isDead;

    private enum ActionType
    {
        None,
        Attack,
        Hit
    }

    private ActionType currentAction = ActionType.None;

    private Vector3 originalScale;

    private static readonly int RunHash =
        Animator.StringToHash("Run");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int HitHash =
        Animator.StringToHash("Hit");

    private static readonly int DeathHash =
        Animator.StringToHash("Death");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider2D>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        originalScale = transform.localScale;
        fixedY = transform.position.y;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints =
                RigidbodyConstraints2D.FreezeRotation;
        }

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void Start()
    {
        FindPlayer();

        if (animator != null)
        {
            animator.SetBool(RunHash, false);
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        UpdateActionTimer();

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;

            if (attackCooldownTimer < 0f)
            {
                attackCooldownTimer = 0f;
            }
        }

        if (TabMenuUI.IsMenuOpen)
        {
            StopMovement();
            return;
        }

        if (isActionLocked)
        {
            StopMovement();
            return;
        }

        if (player == null)
        {
            FindPlayer();

            if (player == null)
                return;
        }

        float distanceX = Mathf.Abs(
            player.position.x - transform.position.x
        );

        float distanceY = Mathf.Abs(
            player.position.y - transform.position.y
        );

        if (distanceX > detectionRange ||
            distanceY > maxVerticalDifference)
        {
            StopMovement();
            return;
        }

        FacePlayer();

        if (distanceX <= attackRange)
        {
            StopMovement();

            if (attackCooldownTimer <= 0f)
            {
                StartAttack();
            }

            return;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || rb == null)
            return;

        Vector2 position = rb.position;

        if (!Mathf.Approximately(position.y, fixedY))
        {
            position.y = fixedY;
            rb.MovePosition(position);
        }

        if (isActionLocked || TabMenuUI.IsMenuOpen)
        {
            StopMovement();
            return;
        }

        if (player == null)
            return;

        float distanceX = Mathf.Abs(
            player.position.x - transform.position.x
        );

        float distanceY = Mathf.Abs(
            player.position.y - transform.position.y
        );

        if (distanceX > detectionRange ||
            distanceY > maxVerticalDifference)
        {
            StopMovement();
            return;
        }

        if (distanceX > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMovement();
        }
    }

    // =========================================================
    // Player
    // =========================================================

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    // =========================================================
    // Movement
    // =========================================================

    private void MoveTowardsPlayer()
    {
        if (player == null || rb == null)
            return;

        float direction = Mathf.Sign(
            player.position.x - transform.position.x
        );

        Vector2 targetPosition =
            rb.position +
            Vector2.right *
            direction *
            moveSpeed *
            Time.fixedDeltaTime;

        targetPosition.y = fixedY;

        rb.MovePosition(targetPosition);

        SetRunAnimation(true);
    }

    private void StopMovement()
    {
        if (rb != null)
        {
            Vector2 position = rb.position;
            position.y = fixedY;

            rb.MovePosition(position);
            rb.linearVelocity = Vector2.zero;
        }

        SetRunAnimation(false);
    }

    private void FacePlayer()
    {
        if (player == null)
            return;

        float direction =
            player.position.x - transform.position.x;

        if (direction < 0f)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z
            );
        }
        else if (direction > 0f)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z
            );
        }
    }

    private void SetRunAnimation(bool value)
    {
        if (animator == null)
            return;

        animator.SetBool(RunHash, value);
    }

    // =========================================================
    // Attack
    // =========================================================

    private void StartAttack()
    {
        if (isDead || isActionLocked)
            return;

        isActionLocked = true;
        currentAction = ActionType.Attack;
        actionTimer = attackFailsafeTime;
        attackCooldownTimer = attackCooldown;

        StopMovement();

        if (animator == null)
        {
            EndAttack();
            return;
        }

        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(HitHash);
        animator.SetTrigger(AttackHash);
    }

    // Attack Animation Event
    public void PlayAttackSound()
    {
        if (audioSource == null || attackSound == null)
            return;

        audioSource.PlayOneShot(attackSound);
    }

    // Attack Animation Event
    public void EnableAttackHitbox()
    {
        if (isDead || currentAction != ActionType.Attack)
            return;

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
        }
    }

    // Attack Animation Event
    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    // Attack Animation Event
    public void EndAttack()
    {
        if (currentAction != ActionType.Attack)
            return;

        DisableAttackHitbox();

        isActionLocked = false;
        currentAction = ActionType.None;
        actionTimer = 0f;

        ReturnToIdle();

        Debug.Log("Slime Boss Attack 종료");
    }

    // =========================================================
    // Hit
    // =========================================================

    public void OnHit()
    {
        if (isDead)
            return;

        DisableAttackHitbox();

        isActionLocked = true;
        currentAction = ActionType.Hit;
        actionTimer = hitFailsafeTime;

        StopMovement();

        if (animator != null)
        {
            animator.ResetTrigger(AttackHash);
            animator.SetTrigger(HitHash);
        }
    }

    // Hit Animation Event
    public void EndHit()
    {
        if (isDead)
            return;

        if (currentAction != ActionType.Hit)
            return;

        isActionLocked = false;
        currentAction = ActionType.None;
        actionTimer = 0f;

        ReturnToIdle();
    }

    // =========================================================
    // Action Safety
    // =========================================================

    private void UpdateActionTimer()
    {
        if (currentAction == ActionType.None)
            return;

        actionTimer -= Time.deltaTime;

        if (actionTimer > 0f)
            return;

        Debug.LogWarning(
            $"Slime Boss 액션 강제 종료: {currentAction}"
        );

        switch (currentAction)
        {
            case ActionType.Attack:
                EndAttack();
                break;

            case ActionType.Hit:
                EndHit();
                break;
        }
    }

    // =========================================================
    // Idle
    // =========================================================

    private void ReturnToIdle()
    {
        if (isDead || animator == null)
            return;

        animator.SetBool(RunHash, false);

        animator.Play("Enemy Idle", 0, 0f);
    }

    // =========================================================
    // Death
    // =========================================================

    public void OnDeath()
    {
        if (isDead)
            return;

        isDead = true;

        DisableAttackHitbox();

        isActionLocked = false;
        currentAction = ActionType.None;
        actionTimer = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints =
                RigidbodyConstraints2D.FreezePositionY |
                RigidbodyConstraints2D.FreezeRotation;
        }

        if (bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool(RunHash, false);

            animator.ResetTrigger(AttackHash);
            animator.ResetTrigger(HitHash);

            animator.SetTrigger(DeathHash);
        }

        Debug.Log("Slime Boss Death 시작");
    }

    // Death Animation Event
    public void FinishDeath()
    {
        if (!isDead)
            return;

        if (BossRoomManager2.Instance != null)
        {
            BossRoomManager2.Instance.UnlockExit();
        }
        else
        {
            Debug.LogError(
                "BossRoomManager2.Instance를 찾을 수 없습니다."
            );
        }

        Destroy(gameObject);
    }

    // =========================================================
    // State
    // =========================================================

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsActionLocked()
    {
        return isActionLocked;
    }
}