using UnityEngine;

public class BossController : MonoBehaviour
{
    private enum ActionType
    {
        None,
        Attack,
        Hit,
        ArmorBreak
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float maxVerticalDifference = 2f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private GameObject attackHitbox;

    [Header("Action Safety")]
    [SerializeField] private float attackFailsafeTime = 2f;
    [SerializeField] private float hitFailsafeTime = 2f;
    [SerializeField] private float armorBreakFailsafeTime = 2f;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D bodyCollider;

    private Transform player;

    private float attackCooldownTimer;
    private float actionTimer;

    private bool isActionLocked;
    private bool isDead;

    private ActionType currentAction = ActionType.None;

    private Vector3 originalScale;

    private float fixedY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider2D>();

        originalScale = transform.localScale;

        // 보스의 현재 높이를 고정
        fixedY = transform.position.y;

        // Kinematic으로 사용
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        rb.constraints =
            RigidbodyConstraints2D.FreezeRotation;

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
            animator.SetBool("Run", false);
            animator.SetBool("ArmorBroken", false);
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        if (TabMenuUI.IsMenuOpen)
        {
            StopHorizontalMovement();
            SetRunAnimation(false);

            UpdateActionTimer();

            return;
        }

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (isActionLocked)
        {
            StopHorizontalMovement();
            SetRunAnimation(false);

            UpdateActionTimer();

            return;
        }

        if (player == null)
        {
            FindPlayer();

            if (player == null)
                return;
        }

        float distanceX =
            Mathf.Abs(
                player.position.x -
                transform.position.x
            );

        float distanceY =
            Mathf.Abs(
                player.position.y -
                transform.position.y
            );

        if (distanceX > detectionRange ||
            distanceY > maxVerticalDifference)
        {
            StopHorizontalMovement();
            SetRunAnimation(false);
            return;
        }

        FacePlayer();

        if (distanceX <= attackRange)
        {
            StopHorizontalMovement();
            SetRunAnimation(false);

            if (attackCooldownTimer <= 0f)
            {
                StartAttack();
            }

            return;
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        // 보스의 Y 위치는 항상 고정
        Vector2 currentPosition = rb.position;

        if (!Mathf.Approximately(
                currentPosition.y,
                fixedY))
        {
            currentPosition.y = fixedY;
            rb.MovePosition(currentPosition);
        }

        if (isActionLocked ||
            TabMenuUI.IsMenuOpen)
        {
            StopHorizontalMovement();
            return;
        }

        if (player == null)
            return;

        float distanceX =
            Mathf.Abs(
                player.position.x -
                transform.position.x
            );

        float distanceY =
            Mathf.Abs(
                player.position.y -
                transform.position.y
            );

        if (distanceX > detectionRange ||
            distanceY > maxVerticalDifference)
        {
            StopHorizontalMovement();
            return;
        }

        if (distanceX > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopHorizontalMovement();
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
        if (player == null)
            return;

        float direction =
            Mathf.Sign(
                player.position.x -
                transform.position.x
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

    private void StopHorizontalMovement()
    {
        if (rb == null)
            return;

        Vector2 currentPosition =
            rb.position;

        currentPosition.y = fixedY;

        rb.MovePosition(currentPosition);

        rb.linearVelocity = Vector2.zero;

        SetRunAnimation(false);
    }

    // =========================================================
    // Face Player
    // =========================================================

    private void FacePlayer()
    {
        if (player == null)
            return;

        if (player.position.x <
            transform.position.x)
        {
            transform.localScale =
                new Vector3(
                    -Mathf.Abs(originalScale.x),
                    originalScale.y,
                    originalScale.z
                );
        }
        else
        {
            transform.localScale =
                new Vector3(
                    Mathf.Abs(originalScale.x),
                    originalScale.y,
                    originalScale.z
                );
        }
    }

    // =========================================================
    // Run
    // =========================================================

    private void SetRunAnimation(bool value)
    {
        if (animator == null)
            return;

        animator.SetBool("Run", value);
    }

    // =========================================================
    // Attack
    // =========================================================

    private void StartAttack()
    {
        if (isActionLocked ||
            isDead)
        {
            return;
        }

        isActionLocked = true;

        currentAction =
            ActionType.Attack;

        actionTimer =
            attackFailsafeTime;

        attackCooldownTimer =
            attackCooldown;

        StopHorizontalMovement();

        if (animator == null)
            return;

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Hit");
        animator.ResetTrigger("ArmorBreak");

        bool armorBroken =
            animator.GetBool("ArmorBroken");

        if (armorBroken)
        {
            int attackIndex =
                Random.Range(0, 2);

            if (attackIndex == 0)
            {
                animator.SetTrigger("Attack");
            }
            else
            {
                animator.SetTrigger("Attack2");
            }
        }
        else
        {
            animator.SetTrigger("Attack");
        }
    }

    // =========================================================
    // Attack Animation Event
    // =========================================================

    public void EnableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
        }
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    public void EndAttack()
    {
        if (currentAction != ActionType.Attack &&
            !isActionLocked)
        {
            return;
        }

        DisableAttackHitbox();

        isActionLocked = false;

        currentAction =
            ActionType.None;

        actionTimer = 0f;

        ReturnToIdle();

        Debug.Log("Boss Attack 종료");
    }

    // =========================================================
    // Action Safety
    // =========================================================

    private void UpdateActionTimer()
    {
        if (currentAction ==
            ActionType.None)
        {
            return;
        }

        actionTimer -= Time.deltaTime;

        if (actionTimer > 0f)
            return;

        Debug.LogWarning(
            $"Boss 액션 강제 종료 : {currentAction}"
        );

        switch (currentAction)
        {
            case ActionType.Attack:
                EndAttack();
                break;

            case ActionType.Hit:
                EndHit();
                break;

            case ActionType.ArmorBreak:
                EndArmorBreak();
                break;
        }
    }

    // =========================================================
    // Idle
    // =========================================================

    private void ReturnToIdle()
    {
        if (animator == null)
            return;

        bool armorBroken =
            animator.GetBool("ArmorBroken");

        if (armorBroken)
        {
            animator.Play(
                "Boss_Idle_NoArmor",
                0,
                0f
            );
        }
        else
        {
            animator.Play(
                "Boss_Idle",
                0,
                0f
            );
        }

        SetRunAnimation(false);
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

        currentAction =
            ActionType.Hit;

        actionTimer =
            hitFailsafeTime;

        StopHorizontalMovement();

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("ArmorBreak");

            animator.SetTrigger("Hit");
        }
    }

    public void EndHit()
    {
        if (isDead)
            return;

        isActionLocked = false;

        currentAction =
            ActionType.None;

        actionTimer = 0f;

        ReturnToIdle();
    }

    // =========================================================
    // Armor Break
    // =========================================================

    public void OnArmorBreak()
    {
        if (isDead)
            return;

        DisableAttackHitbox();

        isActionLocked = true;

        currentAction =
            ActionType.ArmorBreak;

        actionTimer =
            armorBreakFailsafeTime;

        StopHorizontalMovement();

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("Hit");

            animator.SetTrigger(
                "ArmorBreak"
            );
        }
    }

    public void EndArmorBreak()
    {
        if (isDead)
            return;

        isActionLocked = false;

        currentAction =
            ActionType.None;

        actionTimer = 0f;

        ReturnToIdle();
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

        currentAction =
            ActionType.None;

        actionTimer = 0f;

        // 보스가 죽어도 현재 위치 유지
        fixedY = transform.position.y;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;

            rb.bodyType =
                RigidbodyType2D.Kinematic;

            rb.gravityScale = 0f;

            rb.constraints =
                RigidbodyConstraints2D.FreezePositionY |
                RigidbodyConstraints2D.FreezeRotation;
        }

        // 몸 충돌만 제거
        if (bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("ArmorBreak");

            animator.SetBool("Run", false);

            animator.SetTrigger("Death");
        }

        Debug.Log("Boss Death 시작");
    }

    // Animation Event
    public void FinishDeath()
    {
        if (BossRoomManager.Instance != null)
        {
            BossRoomManager.Instance.UnlockExit();
        }
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