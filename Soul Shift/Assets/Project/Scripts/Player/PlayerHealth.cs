using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Hit Count")]
    [SerializeField] private int baseMaxHitCount = 5;

    [Header("Hit Effect")]
    [SerializeField] private float hitFlashDuration = 0.08f;

    [Header("Hit Stun")]
    [SerializeField] private float hitStunDuration = 0.15f;
    [SerializeField] private float knockbackForce = 2.5f;

    [Header("Invincible")]
    [SerializeField] private float invincibleTime = 0.3f;

    private int maxHitCount;
    private int currentHitCount;

    private float invincibleTimer;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerCombat playerCombat;
    private PlayerController playerController;
    private Rigidbody2D rb;

    private GameOverUI gameOverUI;

    private Coroutine hitFlashCoroutine;

    private bool isDead;

    private void Awake()
    {
        maxHitCount = baseMaxHitCount;
        currentHitCount = 0;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        gameOverUI = FindFirstObjectByType<GameOverUI>();

        if (gameOverUI == null)
        {
            Debug.LogWarning(
                "GameOverUI를 찾을 수 없습니다."
            );
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;

            if (invincibleTimer < 0f)
            {
                invincibleTimer = 0f;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(
            damage,
            transform.position.x - 1f
        );
    }

    public void TakeDamage(int damage, float attackerX)
    {
        if (isDead)
            return;

        if (invincibleTimer > 0f)
            return;

        currentHitCount++;

        if (currentHitCount > maxHitCount)
        {
            currentHitCount = maxHitCount;
        }

        invincibleTimer = invincibleTime;

        Debug.Log(
            $"Player Hit : {currentHitCount} / {maxHitCount}"
        );

        // 적을 바라봄
        if (playerController != null)
        {
            playerController.FaceTarget(attackerX);
        }

        // 넉백 방향
        float knockbackDirection;

        if (attackerX < transform.position.x)
        {
            // 적이 왼쪽
            // 플레이어는 오른쪽으로 넉백
            knockbackDirection = 1f;
        }
        else
        {
            // 적이 오른쪽
            // 플레이어는 왼쪽으로 넉백
            knockbackDirection = -1f;
        }

        // 공격 중이면 공격 취소
        if (playerCombat != null)
        {
            playerCombat.CancelAttack();
        }

        // 경직 + 넉백
        if (playerController != null)
        {
            playerController.ApplyHitStun(
                hitStunDuration,
                knockbackForce,
                knockbackDirection
            );
        }

        // 마지막 하트
        if (currentHitCount >= maxHitCount)
        {
            Die();
            return;
        }

        // 피격 애니메이션
        if (animator != null)
        {
            animator.ResetTrigger("Death");
            animator.SetTrigger("Hit");
        }

        // 빨간색 플래시
        if (spriteRenderer != null)
        {
            if (hitFlashCoroutine != null)
            {
                StopCoroutine(hitFlashCoroutine);
            }

            hitFlashCoroutine = StartCoroutine(HitFlash());
        }
    }

    private IEnumerator HitFlash()
    {
        Color originalColor = spriteRenderer.color;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(
            hitFlashDuration
        );

        if (!isDead)
        {
            spriteRenderer.color = originalColor;
        }

        hitFlashCoroutine = null;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Player Dead");

        // 이동 정지
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // 공격 비활성화
        if (playerCombat != null)
        {
            playerCombat.CancelAttack();
            playerCombat.enabled = false;
        }

        // 이동 비활성화
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // 피격 플래시 정리
        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
            hitFlashCoroutine = null;
        }

        // 색상 복구
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        if (animator != null)
        {
            // 기존 Trigger 초기화
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("Death");

            // 이동 관련 파라미터 초기화
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsGrounded", false);
            animator.SetBool("IsAttacking", false);
            animator.SetFloat("VerticalVelocity", 0f);

            // Death 상태 직접 재생
            animator.Play(
                "Base Layer.Player_Death",
                0,
                0f
            );

            // 즉시 Animator 적용
            animator.Update(0f);
        }
    }

    // Player_Death 애니메이션 마지막 프레임
    // Animation Event에서 호출
    public void EndDeath()
    {
        Debug.Log("Death Animation End");

        // 재시작 UI 표시
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
        else
        {
            Debug.LogWarning(
                "GameOverUI를 찾을 수 없습니다."
            );
        }

        // 플레이어 삭제
        Destroy(gameObject);
    }

    public void IncreaseMaxHitCount(int amount)
    {
        if (amount <= 0)
            return;

        maxHitCount += amount;

        Debug.Log(
            $"Max Heart Increased : {maxHitCount}"
        );
    }

    public void DecreaseMaxHitCount(int amount)
    {
        if (amount <= 0)
            return;

        maxHitCount -= amount;

        if (maxHitCount < 1)
        {
            maxHitCount = 1;
        }

        if (currentHitCount > maxHitCount)
        {
            currentHitCount = maxHitCount;
        }
    }

    public void RestoreFullHealth()
    {
        if (isDead)
            return;

        currentHitCount = 0;
    }

    public int GetCurrentHitCount()
    {
        return currentHitCount;
    }

    public int GetMaxHitCount()
    {
        return maxHitCount;
    }

    public int GetRemainingHits()
    {
        return maxHitCount - currentHitCount;
    }

    public bool IsDead()
    {
        return isDead;
    }
}