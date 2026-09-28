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

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float moveInput;

    private bool isGrounded;
    private float jumpBufferCounter;
    private float coyoteCounter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        GetInput();
        CheckGround();
        Flip();
        UpdateJumpTimers();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        Move();
        Jump();
    }

    // =========================
    // 입력
    // =========================
    private void GetInput()
    {
        if (Keyboard.current == null)
            return;

        // --------------------------------
        // 이동 입력
        // --------------------------------
        bool leftPressed =
            Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed;

        bool rightPressed =
            Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed;

        // A + D 동시 입력 = 정지
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

        // --------------------------------
        // 점프 입력
        // Space를 누르고 있는 동안 입력 유지
        // --------------------------------
        if (Keyboard.current.spaceKey.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }

        // --------------------------------
        // 공격 입력
        // 지상 / 점프 / 낙하 관계없이 공격 가능
        // --------------------------------
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("Attack");
        }
    }

    // =========================
    // 바닥 판정
    // =========================
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
            // 자기 자신은 제외
            if (hit.gameObject == gameObject)
                continue;

            // Ground 태그 확인
            if (hit.CompareTag("Ground"))
            {
                isGrounded = true;
                break;
            }
        }
    }

    // =========================
    // 점프 보조
    // =========================
    private void UpdateJumpTimers()
    {
        // 점프 버퍼
        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // 코요테 타임
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
    }

    // =========================
    // 이동
    // =========================
    private void Move()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    // =========================
    // 점프
    // =========================
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

    // =========================
    // 애니메이션
    // =========================
    private void UpdateAnimation()
    {
        animator.SetBool("IsMoving", moveInput != 0f);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    // =========================
    // 방향 전환
    // =========================
    private void Flip()
    {
        if (moveInput < 0f)
        {
            spriteRenderer.flipX = true;
        }
        else if (moveInput > 0f)
        {
            spriteRenderer.flipX = false;
        }
    }

    // =========================
    // 공격 종료
    // Animation Event에서 호출
    // =========================
    public void EndAttack()
    {
        animator.SetBool("IsAttacking", false);
    }

    // =========================
    // GroundCheck 표시
    // =========================
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}