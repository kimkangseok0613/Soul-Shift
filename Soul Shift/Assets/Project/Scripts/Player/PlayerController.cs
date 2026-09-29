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

    [Header("Screen Bounds")]
    [SerializeField] private bool useScreenBounds = true;
    [SerializeField] private float screenPadding = 0f;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider;

    private float moveInput;

    private bool isGrounded;
    private float jumpBufferCounter;
    private float coyoteCounter;

    private float hitStunTimer;

    private Vector3 baseScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();

        baseScale = transform.localScale;
    }

    private void Update()
    {
        GetInput();
        CheckGround();
        Flip();
        UpdateJumpTimers();
        UpdateAnimation();

        UpdateHitStunTimer();
    }

    private void FixedUpdate()
    {
        // 피격 경직 중에는 이동과 점프를 하지 않음
        if (hitStunTimer > 0f)
        {
            ClampToScreen();
            return;
        }

        Move();
        Jump();

        // 화면 밖으로 나가지 못하게 제한
        ClampToScreen();
    }

    private void GetInput()
    {
        if (Keyboard.current == null)
            return;

        // 경직 중에는 이동 입력 무시
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

        // A + D 동시에 누르면 정지
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

        // Space를 누르고 있으면 점프 버퍼에 저장
        if (Keyboard.current.spaceKey.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
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
        // 경직 중에는 점프 버퍼를 쌓지 않음
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

            // 경직이 끝나면 수평 속도 제거
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void Jump()
    {
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    // 피격 시 적을 바라보게 함
    public void FaceTarget(float targetX)
    {
        if (targetX > transform.position.x)
        {
            // 오른쪽을 바라봄
            transform.localScale = new Vector3(
                Mathf.Abs(baseScale.x),
                baseScale.y,
                baseScale.z
            );
        }
        else if (targetX < transform.position.x)
        {
            // 왼쪽을 바라봄
            transform.localScale = new Vector3(
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
        if (direction == 0f)
        {
            direction =
                transform.localScale.x >= 0f
                    ? -1f
                    : 1f;
        }

        hitStunTimer = duration;

        // 적 반대 방향으로 넉백
        rb.linearVelocity = new Vector2(
            direction * knockbackForce,
            rb.linearVelocity.y
        );
    }

    private void ClampToScreen()
    {
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

        Vector2 currentPosition = rb.position;

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

            // 화면 밖으로 밀려났을 때 수평 속도 제거
            rb.linearVelocity =
                new Vector2(
                    0f,
                    rb.linearVelocity.y
                );
        }
    }

    private void UpdateAnimation()
    {
        animator.SetBool(
            "IsMoving",
            moveInput != 0f && hitStunTimer <= 0f
        );

        animator.SetBool(
            "IsGrounded",
            isGrounded
        );

        animator.SetFloat(
            "VerticalVelocity",
            rb.linearVelocity.y
        );
    }

    private void Flip()
    {
        // 경직 중에는 방향 전환하지 않음
        if (hitStunTimer > 0f)
            return;

        if (moveInput < 0f)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(baseScale.x),
                baseScale.y,
                baseScale.z
            );
        }
        else if (moveInput > 0f)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(baseScale.x),
                baseScale.y,
                baseScale.z
            );
        }
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
                mainCamera.transform.position.y - cameraHeight * 0.5f
            ),
            new Vector3(
                leftScreen,
                mainCamera.transform.position.y + cameraHeight * 0.5f
            )
        );

        Gizmos.DrawLine(
            new Vector3(
                rightScreen,
                mainCamera.transform.position.y - cameraHeight * 0.5f
            ),
            new Vector3(
                rightScreen,
                mainCamera.transform.position.y + cameraHeight * 0.5f
            )
        );
    }
}