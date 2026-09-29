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
        maxHitCount = Mathf.Max(
            1,
            baseMaxHitCount
        );

        currentHitCount = 0;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        gameOverUI =
            FindFirstObjectByType<GameOverUI>();

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

        if (invincibleTimer <= 0f)
            return;

        invincibleTimer -= Time.deltaTime;

        if (invincibleTimer < 0f)
        {
            invincibleTimer = 0f;
        }
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(
            damage,
            transform.position.x - 1f
        );
    }

    public void TakeDamage(
        int damage,
        float attackerX
    )
    {
        if (isDead)
            return;

        if (damage <= 0)
            return;

        if (invincibleTimer > 0f)
            return;

        currentHitCount = Mathf.Min(
            currentHitCount + 1,
            maxHitCount
        );

        invincibleTimer = invincibleTime;

        Debug.Log(
            $"Player Hit : {currentHitCount} / {maxHitCount}"
        );

        // 적을 바라봄
        if (playerController != null)
        {
            playerController.FaceTarget(
                attackerX
            );
        }

        // 적의 반대 방향으로 넉백
        float knockbackDirection;

        if (attackerX < transform.position.x)
        {
            knockbackDirection = 1f;
        }
        else
        {
            knockbackDirection = -1f;
        }

        // 공격 중이면 취소
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

        // 일반 피격
        PlayHitAnimation();
        StartHitFlash();
    }

    private void PlayHitAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger("Death");
        animator.SetTrigger("Hit");
    }

    private void StartHitFlash()
    {
        if (spriteRenderer == null)
            return;

        StopHitFlash();

        hitFlashCoroutine =
            StartCoroutine(
                HitFlash()
            );
    }

    private IEnumerator HitFlash()
    {
        Color originalColor =
            spriteRenderer.color;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(
            hitFlashDuration
        );

        if (!isDead)
        {
            spriteRenderer.color =
                originalColor;
        }

        hitFlashCoroutine = null;
    }

    private void StopHitFlash()
    {
        if (hitFlashCoroutine == null)
            return;

        StopCoroutine(
            hitFlashCoroutine
        );

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

        // 공격 취소
        if (playerCombat != null)
        {
            playerCombat.CancelAttack();
            playerCombat.enabled = false;
        }

        // 이동/점프 정지
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // 피격 플래시 종료
        StopHitFlash();

        // 색상 복구
        if (spriteRenderer != null)
        {
            spriteRenderer.color =
                Color.white;
        }

        // Death 재생
        PlayDeathAnimation();
    }

    private void PlayDeathAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning(
                "Player Animator를 찾을 수 없습니다."
            );

            return;
        }

        // 모든 Trigger 초기화
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Hit");
        animator.ResetTrigger("Death");

        // 다른 애니메이션 조건 제거
        animator.SetBool(
            "IsMoving",
            false
        );

        animator.SetBool(
            "IsGrounded",
            false
        );

        animator.SetBool(
            "IsAttacking",
            false
        );

        animator.SetFloat(
            "VerticalVelocity",
            0f
        );

        // Animator가 꺼져 있을 가능성 방지
        animator.enabled = true;

        // 현재 상태를 즉시 갱신
        animator.Update(0f);

        // Death State 직접 재생
        int deathStateHash =
            Animator.StringToHash(
                "Base Layer.Player_Death"
            );

        animator.Play(
            deathStateHash,
            0,
            0f
        );

        // 이번 프레임에 즉시 적용
        animator.Update(0f);

        Debug.Log(
            "Player_Death 재생"
        );
    }

    // Player_Death 마지막 프레임 Animation Event
    public void EndDeath()
    {
        Debug.Log(
            "Death Animation End"
        );

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

        Destroy(gameObject);
    }

    public void IncreaseMaxHitCount(int amount)
    {
        if (isDead)
            return;

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

        maxHitCount = Mathf.Max(
            1,
            maxHitCount - amount
        );

        currentHitCount = Mathf.Min(
            currentHitCount,
            maxHitCount
        );
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