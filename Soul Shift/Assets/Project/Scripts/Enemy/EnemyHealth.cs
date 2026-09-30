using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 30;

    [Header("Gold Reward")]
    [SerializeField] private int goldReward = 10;

    [Header("Hit Effect")]
    [SerializeField] private float hitFlashDuration = 0.08f;

    [Header("Damage Popup")]
    [SerializeField] private GameObject damagePopupPrefab;

    [SerializeField]
    private Vector3 damagePopupOffset =
        new Vector3(0f, 0.5f, 0f);

    [Header("Death")]
    [SerializeField] private float deathDelay = 0.8f;

    private int currentHealth;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private EnemyController enemyController;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;

    private Coroutine hitFlashCoroutine;

    private bool isDead;

    private string enemyId;

    private void Awake()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        enemyController = GetComponent<EnemyController>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        // 씬 + Hierarchy 위치를 이용해
        // 현재 적만의 고유 ID 생성
        enemyId =
            EnemyPersistence.CreateEnemyId(gameObject);
    }

    private void Start()
    {
        // 이전에 죽었던 적이면 이번 씬에서는 생성하지 않음
        if (EnemyPersistence.IsDead(enemyId))
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (currentHealth <= 0)
            return;

        if (damage <= 0)
            return;

        currentHealth -= damage;

        Debug.Log(
            $"{gameObject.name} HP : {currentHealth} / {maxHealth}"
        );

        ShowDamagePopup(damage);

        // 사망
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 피격 처리
        if (enemyController != null)
        {
            enemyController.OnHit();
        }

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        // 빨간색 플래시
        if (spriteRenderer != null)
        {
            if (hitFlashCoroutine != null)
            {
                StopCoroutine(hitFlashCoroutine);
            }

            hitFlashCoroutine =
                StartCoroutine(HitFlash());
        }
    }

    private void ShowDamagePopup(int damage)
    {
        if (damagePopupPrefab == null)
            return;

        Vector3 spawnPosition =
            transform.position +
            damagePopupOffset;

        GameObject popup =
            Instantiate(
                damagePopupPrefab,
                spawnPosition,
                Quaternion.identity
            );

        DamagePopup damagePopup =
            popup.GetComponent<DamagePopup>();

        if (damagePopup != null)
        {
            damagePopup.SetDamage(damage);
        }
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

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // 죽었다는 사실을 저장
        EnemyPersistence.MarkDead(enemyId);

        Debug.Log(
            $"Enemy Dead : {gameObject.name}"
        );

        // -------------------------
        // 골드 지급
        // -------------------------
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(
                goldReward
            );
        }
        else
        {
            Debug.LogWarning(
                "GoldManager를 찾을 수 없습니다. " +
                "골드가 지급되지 않았습니다."
            );
        }

        // -------------------------
        // 1. 충돌 제거
        // -------------------------
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // -------------------------
        // 2. 물리 이동 정지
        // -------------------------
        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity = 0f;

            rb.simulated = false;
        }

        // -------------------------
        // 3. 공격 히트박스 제거
        // -------------------------
        if (enemyController != null)
        {
            enemyController.DisableAttackHitbox();
        }

        // -------------------------
        // 4. 피격 플래시 종료
        // -------------------------
        if (hitFlashCoroutine != null)
        {
            StopCoroutine(
                hitFlashCoroutine
            );

            hitFlashCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color =
                Color.white;
        }

        // -------------------------
        // 5. Death 애니메이션
        // -------------------------
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // -------------------------
        // 6. 일정 시간 후 삭제
        // -------------------------
        StartCoroutine(
            DeathSequence()
        );
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(
            deathDelay
        );

        Destroy(gameObject);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetGoldReward()
    {
        return goldReward;
    }

    public bool IsDead()
    {
        return isDead;
    }
}