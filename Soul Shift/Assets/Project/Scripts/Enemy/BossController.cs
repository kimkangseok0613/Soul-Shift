using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float maxVerticalDifference = 2f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private GameObject attackHitbox;

    private Rigidbody2D rb;
    private Animator animator;

    private Transform player;

    private float attackCooldownTimer;

    private bool isAttacking;
    private bool isStopped;

    private Vector3 originalScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        originalScale = transform.localScale;

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
        if (isStopped)
            return;

        if (TabMenuUI.IsMenuOpen)
        {
            StopHorizontalMovement();
            return;
        }

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
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
            return;
        }

        FacePlayer();

        if (isAttacking)
        {
            StopHorizontalMovement();
            return;
        }

        if (distanceX <= attackRange)
        {
            StopHorizontalMovement();

            if (attackCooldownTimer <= 0f)
            {
                StartAttack();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isStopped ||
            isAttacking ||
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
    // Player 찾기
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
    // 이동
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

        Vector2 movement =
            rb.position +
            Vector2.right *
            direction *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(movement);
    }

    private void StopHorizontalMovement()
    {
        if (rb == null)
            return;

        rb.linearVelocity =
            new Vector2(
                0f,
                rb.linearVelocity.y
            );
    }

    // =========================================================
    // 방향
    // =========================================================

    private void FacePlayer()
    {
        if (player == null)
            return;

        if (player.position.x < transform.position.x)
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
    // 공격
    // =========================================================

    private void StartAttack()
    {
        if (isAttacking)
            return;

        isAttacking = true;

        attackCooldownTimer = attackCooldown;

        StopHorizontalMovement();

        rb.constraints =
            RigidbodyConstraints2D.FreezePositionX |
            RigidbodyConstraints2D.FreezeRotation;

        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("ArmorBreak");
            animator.ResetTrigger("Death");

            animator.SetTrigger("Attack");
        }
    }

    // Animation Event
    public void EnableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
        }
    }

    // Animation Event
    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    // Animation Event
    public void EndAttack()
    {
        DisableAttackHitbox();

        isAttacking = false;

        rb.constraints =
            RigidbodyConstraints2D.FreezeRotation;
    }

    // =========================================================
    // 피격
    // =========================================================

    public void OnHit()
    {
        if (isStopped)
            return;

        DisableAttackHitbox();

        isAttacking = false;

        rb.constraints =
            RigidbodyConstraints2D.FreezeRotation;

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    // =========================================================
    // Armor Break
    // =========================================================

    public void OnArmorBreak()
    {
        if (isStopped)
            return;

        DisableAttackHitbox();

        isAttacking = false;

        rb.constraints =
            RigidbodyConstraints2D.FreezeRotation;

        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            animator.SetTrigger("ArmorBreak");
        }
    }

    // =========================================================
    // 사망
    // =========================================================

    public void OnDeath()
    {
        isStopped = true;

        DisableAttackHitbox();

        StopHorizontalMovement();

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("ArmorBreak");

            animator.SetTrigger("Death");
        }

        rb.constraints =
            RigidbodyConstraints2D.FreezeRotation;
    }

    // Animation Event
    public void FinishDeath()
    {
        if (BossRoomManager.Instance != null)
        {
            BossRoomManager.Instance.UnlockExit();
        }
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }
}