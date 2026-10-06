using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    [Header("Jump Assist")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.05f;

    [Header("Hit Stun")]
    [SerializeField] private float defaultHitStunDuration = 0.15f;
    [SerializeField] private float defaultKnockbackForce = 2.5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.8f;

    [Header("Dash Collision Fix")]
    [SerializeField] private float dashSeparationPadding = 0.02f;

    [Header("Screen Bounds")]
    [SerializeField] private bool useScreenBounds = true;
    [SerializeField] private float screenPadding = 0f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip dashSound;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider;

    private float moveInput;
    private bool isGrounded;
    private float jumpBufferCounter;
    private float coyoteCounter;
    private float hitStunTimer;

    private float dashTimer;
    private float dashCooldownTimer;
    private bool isDashing;
    private Vector2 dashDirection;

    private Vector3 baseScale;

    // 강화 전 기본 이동속도 저장
    private float baseMoveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        baseScale = transform.localScale;

        // 현재 Inspector에 설정된 이동속도를 기본값으로 저장
        baseMoveSpeed = moveSpeed;
    }

    private void Update()
    {
        UpdateDashTimers();

        if (TabMenuUI.IsMenuOpen)
        {
            moveInput = 0f;

            if (isDashing)
                EndDash();

            UpdateAnimation();
            return;
        }

        GetInput();
        CheckGround();
        Flip();
        UpdateJumpTimers();
        UpdateAnimation();
        UpdateHitStunTimer();
    }

    private void FixedUpdate()
    {
        if (TabMenuUI.IsMenuOpen)
        {
            rb.linearVelocity =
                new Vector2(
                    0f,
                    rb.linearVelocity.y
                );

            return;
        }

        if (isDashing)
        {
            DashMove();
            ClampToScreen();
            return;
        }

        if (hitStunTimer > 0f)
        {
            ClampToScreen();
            return;
        }

        Move();
        Jump();
        ClampToScreen();
    }

    private void GetInput()
    {
        if (Keyboard.current == null)
            return;

        if (hitStunTimer > 0f)
        {
            moveInput = 0f;
            return;
        }

        bool leftPressed =
            Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed;

        bool rightPressed =
            Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed;

        if (leftPressed && rightPressed)
        {
            moveInput = 0f;
        }
        else if (leftPressed)
        {
            moveInput = -1f;
        }
        else if (rightPressed)
        {
            moveInput = 1f;
        }
        else
        {
            moveInput = 0f;
        }

        if (Keyboard.current.spaceKey.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame ||
            Keyboard.current.rightShiftKey.wasPressedThisFrame)
        {
            TryDash();
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                groundCheck.position,
                groundCheckRadius
            );

        isGrounded = false;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            if (hit.CompareTag("Ground"))
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void UpdateJumpTimers()
    {
        if (hitStunTimer > 0f)
        {
            jumpBufferCounter = 0f;
            return;
        }

        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
    }

    private void UpdateHitStunTimer()
    {
        if (hitStunTimer <= 0f)
            return;

        hitStunTimer -= Time.deltaTime;

        if (hitStunTimer <= 0f)
        {
            hitStunTimer = 0f;

            rb.linearVelocity =
                new Vector2(
                    0f,
                    rb.linearVelocity.y
                );
        }
    }

    private void UpdateDashTimers()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;

            if (dashCooldownTimer < 0f)
            {
                dashCooldownTimer = 0f;
            }
        }
    }

    private void Move()
    {
        rb.linearVelocity =
            new Vector2(
                moveInput * moveSpeed,
                rb.linearVelocity.y
            );
    }

    private void Jump()
    {
        if (jumpBufferCounter > 0f &&
            coyoteCounter > 0f)
        {
            rb.linearVelocity =
                new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;

            PlayJumpSound();
        }
    }

    private void TryDash()
    {
        if (isDashing)
            return;

        if (dashCooldownTimer > 0f)
            return;

        if (hitStunTimer > 0f)
            return;

        dashDirection = GetDashDirection();

        if (dashDirection == Vector2.zero)
            return;

        isDashing = true;

        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        jumpBufferCounter = 0f;
        moveInput = 0f;

        PlayDashSound();

        if (animator != null)
        {
            animator.SetBool(
                "IsDashing",
                true
            );
        }

        // 대시 중에는 적과 겹칠 수 있도록 충돌 무시
        SetEnemyCollision(false);

        rb.linearVelocity =
            new Vector2(
                dashDirection.x * dashSpeed,
                0f
            );
    }

    private Vector2 GetDashDirection()
    {
        float direction = moveInput;

        if (Mathf.Approximately(direction, 0f))
        {
            direction =
                transform.localScale.x >= 0f
                    ? 1f
                    : -1f;
        }

        return new Vector2(
            Mathf.Sign(direction),
            0f
        );
    }

    private void DashMove()
    {
        if (!isDashing)
            return;

        rb.linearVelocity =
            new Vector2(
                dashDirection.x * dashSpeed,
                0f
            );

        dashTimer -= Time.fixedDeltaTime;

        if (dashTimer <= 0f)
        {
            EndDash();
        }
    }

    private void EndDash()
    {
        if (!isDashing)
            return;

        ResolveEnemyOverlapAfterDash();

        isDashing = false;

        rb.linearVelocity =
            new Vector2(
                0f,
                rb.linearVelocity.y
            );

        SetEnemyCollision(true);

        if (animator != null)
        {
            animator.SetBool(
                "IsDashing",
                false
            );
        }
    }

    private void ResolveEnemyOverlapAfterDash()
    {
        if (playerCollider == null)
            return;

        int enemyLayer =
            LayerMask.NameToLayer("Enemy");

        if (enemyLayer == -1)
            return;

        int enemyLayerMask =
            1 << enemyLayer;

        for (int i = 0; i < 4; i++)
        {
            Collider2D[] enemies =
                Physics2D.OverlapBoxAll(
                    playerCollider.bounds.center,
                    playerCollider.bounds.size,
                    0f,
                    enemyLayerMask
                );

            bool foundOverlap = false;

            foreach (Collider2D enemy in enemies)
            {
                if (enemy == null)
                    continue;

                if (enemy == playerCollider)
                    continue;

                if (!enemy.bounds.Intersects(
                        playerCollider.bounds))
                    continue;

                foundOverlap = true;

                Bounds playerBounds =
                    playerCollider.bounds;

                Bounds enemyBounds =
                    enemy.bounds;

                float moveX;

                if (dashDirection.x > 0f)
                {
                    moveX =
                        enemyBounds.max.x -
                        playerBounds.min.x +
                        dashSeparationPadding;
                }
                else
                {
                    moveX =
                        enemyBounds.min.x -
                        playerBounds.max.x -
                        dashSeparationPadding;
                }

                rb.position =
                    new Vector2(
                        rb.position.x + moveX,
                        rb.position.y
                    );

                Physics2D.SyncTransforms();
            }

            if (!foundOverlap)
                break;
        }
    }

    private void SetEnemyCollision(bool enabled)
    {
        int playerLayer =
            LayerMask.NameToLayer("Player");

        int enemyLayer =
            LayerMask.NameToLayer("Enemy");

        if (playerLayer == -1 ||
            enemyLayer == -1)
            return;

        Physics2D.IgnoreLayerCollision(
            playerLayer,
            enemyLayer,
            !enabled
        );
    }

    public void FaceTarget(float targetX)
    {
        if (targetX > transform.position.x)
        {
            transform.localScale =
                new Vector3(
                    Mathf.Abs(baseScale.x),
                    baseScale.y,
                    baseScale.z
                );
        }
        else if (targetX < transform.position.x)
        {
            transform.localScale =
                new Vector3(
                    -Mathf.Abs(baseScale.x),
                    baseScale.y,
                    baseScale.z
                );
        }
    }

    public void ApplyHitStun(
        float duration,
        float knockbackForce,
        float direction
    )
    {
        if (isDashing)
            EndDash();

        if (direction == 0f)
        {
            direction =
                transform.localScale.x >= 0f
                    ? -1f
                    : 1f;
        }

        if (duration <= 0f)
        {
            duration =
                defaultHitStunDuration;
        }

        if (knockbackForce <= 0f)
        {
            knockbackForce =
                defaultKnockbackForce;
        }

        hitStunTimer = duration;

        rb.linearVelocity =
            new Vector2(
                direction * knockbackForce,
                rb.linearVelocity.y
            );
    }

    public bool IsHitStunned()
    {
        return hitStunTimer > 0f;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetBaseMoveSpeed()
    {
        return baseMoveSpeed;
    }

    public void SetUpgradeLevel(
        int upgradeLevel,
        float speedPerLevel
    )
    {
        if (upgradeLevel < 0)
            upgradeLevel = 0;

        if (speedPerLevel < 0f)
            speedPerLevel = 0f;

        moveSpeed =
            baseMoveSpeed +
            upgradeLevel * speedPerLevel;
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    private void ClampToScreen()
    {
        if (!useScreenBounds)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null ||
            !mainCamera.orthographic)
            return;

        float cameraHeight =
            mainCamera.orthographicSize * 2f;

        float cameraWidth =
            cameraHeight * mainCamera.aspect;

        float leftScreen =
            mainCamera.transform.position.x -
            cameraWidth * 0.5f;

        float rightScreen =
            mainCamera.transform.position.x +
            cameraWidth * 0.5f;

        float colliderExtentX = 0f;

        if (playerCollider != null)
        {
            colliderExtentX =
                playerCollider.bounds.extents.x;
        }

        float minX =
            leftScreen +
            colliderExtentX +
            screenPadding;

        float maxX =
            rightScreen -
            colliderExtentX -
            screenPadding;

        Vector2 currentPosition =
            rb.position;

        float clampedX =
            Mathf.Clamp(
                currentPosition.x,
                minX,
                maxX
            );

        if (!Mathf.Approximately(
                currentPosition.x,
                clampedX))
        {
            rb.position =
                new Vector2(
                    clampedX,
                    currentPosition.y
                );

            if (!isDashing)
            {
                rb.linearVelocity =
                    new Vector2(
                        0f,
                        rb.linearVelocity.y
                    );
            }
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        animator.SetBool(
            "IsMoving",
            moveInput != 0f &&
            hitStunTimer <= 0f &&
            !isDashing
        );

        animator.SetBool(
            "IsGrounded",
            isGrounded
        );

        animator.SetFloat(
            "VerticalVelocity",
            rb.linearVelocity.y
        );

        animator.SetBool(
            "IsDashing",
            isDashing
        );
    }

    private void Flip()
    {
        if (hitStunTimer > 0f)
            return;

        if (isDashing)
            return;

        if (moveInput < 0f)
        {
            transform.localScale =
                new Vector3(
                    -Mathf.Abs(baseScale.x),
                    baseScale.y,
                    baseScale.z
                );
        }
        else if (moveInput > 0f)
        {
            transform.localScale =
                new Vector3(
                    Mathf.Abs(baseScale.x),
                    baseScale.y,
                    baseScale.z
                );
        }
    }

    private void OnDisable()
    {
        SetEnemyCollision(true);
    }

    private void PlayJumpSound()
    {
        if (audioSource == null)
            return;

        if (jumpSound == null)
            return;

        audioSource.PlayOneShot(jumpSound);
    }

    private void PlayDashSound()
    {
        if (audioSource == null)
            return;

        if (dashSound == null)
            return;

        audioSource.PlayOneShot(dashSound);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }

        if (!useScreenBounds)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        if (!mainCamera.orthographic)
            return;

        float cameraHeight =
            mainCamera.orthographicSize * 2f;

        float cameraWidth =
            cameraHeight * mainCamera.aspect;

        float leftScreen =
            mainCamera.transform.position.x -
            cameraWidth * 0.5f;

        float rightScreen =
            mainCamera.transform.position.x +
            cameraWidth * 0.5f;

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(
            new Vector3(
                leftScreen,
                mainCamera.transform.position.y -
                cameraHeight * 0.5f
            ),
            new Vector3(
                leftScreen,
                mainCamera.transform.position.y +
                cameraHeight * 0.5f
            )
        );

        Gizmos.DrawLine(
            new Vector3(
                rightScreen,
                mainCamera.transform.position.y -
                cameraHeight * 0.5f
            ),
            new Vector3(
                rightScreen,
                mainCamera.transform.position.y +
                cameraHeight * 0.5f
            )
        );
    }
}