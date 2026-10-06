using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 30;

    [Header("Gold Reward")]
    [SerializeField] private int goldReward = 10;

    [Header("Hit Effect")]
    [SerializeField] private float hitFlashDuration = 0.08f;

    [Header("Death")]
    [SerializeField] private float deathDelay = 0.8f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    private int currentHealth;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private EnemyController enemyController;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;

    private Coroutine hitFlashCoroutine;

    private bool isDead;

    private string enemySaveKey;

    private const string EnemyDeadKeyPrefix = "EnemyDead_";

    private void Awake()
    {
        currentHealth = maxHealth;

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        animator =
            GetComponent<Animator>();

        enemyController =
            GetComponent<EnemyController>();

        enemyCollider =
            GetComponent<Collider2D>();

        rb =
            GetComponent<Rigidbody2D>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        enemySaveKey =
            GetEnemySaveKey();

        // 이전에 죽인 적인지 확인
        if (PlayerPrefs.GetInt(enemySaveKey, 0) == 1)
        {
            isDead = true;

            gameObject.SetActive(false);

            return;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (currentHealth <= 0)
            return;

        currentHealth -= damage;

        Debug.Log(
            $"{gameObject.name} HP : " +
            $"{currentHealth} / {maxHealth}"
        );

        // 사망
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 피격 사운드
        PlayHitSound();

        // 피격 처리
        if (enemyController != null)
        {
            enemyController.OnHit();
        }

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        // 빨간색 피격 플래시
        if (spriteRenderer != null)
        {
            if (hitFlashCoroutine != null)
            {
                StopCoroutine(
                    hitFlashCoroutine
                );
            }

            hitFlashCoroutine =
                StartCoroutine(HitFlash());
        }
    }

    private IEnumerator HitFlash()
    {
        Color originalColor =
            spriteRenderer.color;

        spriteRenderer.color =
            Color.red;

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

        Debug.Log(
            $"{gameObject.name} Enemy Dead"
        );

        // 사망 사운드
        PlayDeathSound();

        // --------------------------------
        // 1. 적 사망 상태 저장
        // --------------------------------
        PlayerPrefs.SetInt(
            enemySaveKey,
            1
        );

        PlayerPrefs.Save();

        // --------------------------------
        // 2. 골드 지급
        // --------------------------------
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(
                goldReward
            );

            Debug.Log(
                $"골드 획득 : +{goldReward}"
            );
        }
        else
        {
            Debug.LogWarning(
                "GoldManager.Instance를 찾을 수 없습니다."
            );
        }

        // --------------------------------
        // 3. 충돌 제거
        // --------------------------------
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // --------------------------------
        // 4. 물리 이동 정지
        // --------------------------------
        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;

            rb.simulated =
                false;
        }

        // --------------------------------
        // 5. 공격 히트박스 제거
        // --------------------------------
        if (enemyController != null)
        {
            enemyController
                .DisableAttackHitbox();
        }

        // --------------------------------
        // 6. 피격 플래시 종료
        // --------------------------------
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

        // --------------------------------
        // 7. Death 애니메이션
        // --------------------------------
        if (animator != null)
        {
            animator.SetTrigger(
                "Death"
            );
        }

        // --------------------------------
        // 8. 일정 시간 후 적 삭제
        // --------------------------------
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

    // =========================================================
    // Audio
    // =========================================================

    private void PlayHitSound()
    {
        if (audioSource == null)
            return;

        if (hitSound == null)
            return;

        audioSource.PlayOneShot(hitSound);
    }

    private void PlayDeathSound()
    {
        if (audioSource == null)
            return;

        if (deathSound == null)
            return;

        audioSource.PlayOneShot(deathSound);
    }

    private string GetEnemySaveKey()
    {
        string sceneName =
            SceneManager
                .GetActiveScene()
                .name;

        string hierarchyPath =
            "";

        Transform current =
            transform;

        while (current != null)
        {
            hierarchyPath =
                current.name +
                "_" +
                current.GetSiblingIndex() +
                "/" +
                hierarchyPath;

            current =
                current.parent;
        }

        return
            EnemyDeadKeyPrefix +
            sceneName +
            "_" +
            hierarchyPath;
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