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
    [SerializeField] private float hitStunDuration = 0.15f;
    [SerializeField] private float knockbackForce = 2.5f;

    [Header("Screen Bounds")]
    [SerializeField] private bool useScreenBounds = true;
    [SerializeField] private float screenPadding = 0f;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider;

    private float moveInput;
    private float jumpBufferCounter;
    private float coyoteCounter;
    private float hitStunTimer;

    private bool isGrounded;
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
        UpdateHitStunTimer();
        GetInput();
        CheckGround();
        UpdateJumpTimers();
        Flip();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (IsHitStunned)
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

        if (IsHitStunned)
        {
            moveInput = 0f;
            jumpBufferCounter = 0f;
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
        if (IsHitStunned)
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

        if (hitStunTimer > 0f)
            return;

        hitStunTimer = 0f;

        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
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
        if (jumpBufferCounter <= 0f || coyoteCounter <= 0f)
            return;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
    }

    public void FaceTarget(float targetX)
    {
        if (targetX > transform.position.x)
        {
            SetFacing(1f);
        }
        else if (targetX < transform.position.x)
        {
            SetFacing(-1f);
        }
    }

    public void ApplyHitStun(
        float duration,
        float knockbackForce,
        float direction
    )
    {
        if (Mathf.Approximately(direction, 0f))
        {
            direction =
                transform.localScale.x >= 0f
                    ? -1f
                    : 1f;
        }

        hitStunTimer = Mathf.Max(0f, duration);

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

        if (mainCamera == null || !mainCamera.orthographic)
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

        float colliderExtentX =
            playerCollider != null
                ? playerCollider.bounds.extents.x
                : 0f;

        float minX =
            leftScreen +
            colliderExtentX +
            screenPadding;

        float maxX =
            rightScreen -
            colliderExtentX -
            screenPadding;

        Vector2 currentPosition = rb.position;

        float clampedX = Mathf.Clamp(
            currentPosition.x,
            minX,
            maxX
        );

        if (Mathf.Approximately(currentPosition.x, clampedX))
            return;

        rb.position = new Vector2(
            clampedX,
            currentPosition.y
        );

        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }

    private void UpdateAnimation()
    {
        animator.SetBool(
            "IsMoving",
            moveInput != 0f && !IsHitStunned
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
        if (IsHitStunned)
            return;

        if (moveInput < 0f)
        {
            SetFacing(-1f);
        }
        else if (moveInput > 0f)
        {
            SetFacing(1f);
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

        if (mainCamera == null || !mainCamera.orthographic)
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

    private bool IsHitStunned =>
        hitStunTimer > 0f;
}