using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 100;

    [Header("Hit Effect")]
    [SerializeField] private float hitFlashDuration = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip armorBreakSound;
    [SerializeField] private AudioClip deathSound;

    private int currentHp;

    private bool armorBroken;
    private bool isDead;

    private Animator animator;
    private BossController bossController;
    private SpriteRenderer spriteRenderer;

    private Coroutine hitFlashCoroutine;

    private const string BossDeadKey =
        "BossDefeated";

    private void Awake()
    {
        animator =
            GetComponent<Animator>();

        bossController =
            GetComponent<BossController>();

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        // BossHealth가 부모에 있고
        // SpriteRenderer가 자식에 있는 경우
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        currentHp =
            maxHp;

        armorBroken =
            false;

        isDead =
            false;

        // --------------------------------
        // 이전에 보스를 처치했는지 확인
        // --------------------------------
        if (PlayerPrefs.GetInt(
                BossDeadKey,
                0) == 1)
        {
            isDead = true;

            currentHp = 0;

            if (bossController != null)
            {
                bossController.OnDeath();
            }

            gameObject.SetActive(false);

            return;
        }

        // --------------------------------
        // 시작 시 갑옷 상태
        // --------------------------------
        if (animator != null)
        {
            animator.SetBool(
                "ArmorBroken",
                false
            );
        }
    }

    // =========================================================
    // Damage
    // =========================================================

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (damage <= 0)
            return;

        currentHp -= damage;

        currentHp =
            Mathf.Max(
                currentHp,
                0
            );

        Debug.Log(
            $"Boss HP : {currentHp} / {maxHp}"
        );

        // =====================================================
        // 사망하지 않았다면 피격 처리
        // =====================================================

        if (currentHp > 0)
        {
            StartHitFlash();

            // 일반 피격 사운드
            PlaySound(hitSound);
        }

        // =====================================================
        // Death
        // =====================================================

        if (currentHp <= 0)
        {
            Die();
            return;
        }

        // =====================================================
        // Armor Break
        // HP 50% 이하
        // =====================================================

        if (!armorBroken &&
            currentHp <= maxHp * 0.5f)
        {
            BreakArmor();

            return;
        }

        // =====================================================
        // Hit
        // =====================================================

        if (bossController != null)
        {
            bossController.OnHit();
        }
        else if (animator != null)
        {
            animator.SetTrigger(
                "Hit"
            );
        }
    }

    // =========================================================
    // Hit Flash
    // =========================================================

    private void StartHitFlash()
    {
        if (spriteRenderer == null)
            return;

        if (hitFlashCoroutine != null)
        {
            StopCoroutine(
                hitFlashCoroutine
            );

            spriteRenderer.color =
                Color.white;
        }

        hitFlashCoroutine =
            StartCoroutine(
                HitFlash()
            );
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

        hitFlashCoroutine =
            null;
    }

    // =========================================================
    // Armor Break
    // =========================================================

    private void BreakArmor()
    {
        if (armorBroken)
            return;

        armorBroken =
            true;

        Debug.Log(
            "Boss Armor Break!"
        );

        // 갑옷 파괴 사운드
        PlaySound(
            armorBreakSound
        );

        // Animator에 갑옷 파괴 상태 전달
        if (animator != null)
        {
            animator.SetBool(
                "ArmorBroken",
                true
            );
        }

        // BossController에서
        // ArmorBreak 애니메이션 실행
        if (bossController != null)
        {
            bossController.OnArmorBreak();
        }
        else if (animator != null)
        {
            animator.SetTrigger(
                "ArmorBreak"
            );
        }
    }

    // =========================================================
    // Death
    // =========================================================

    private void Die()
    {
        if (isDead)
            return;

        isDead =
            true;

        // 피격 플래시 종료
        if (hitFlashCoroutine != null)
        {
            StopCoroutine(
                hitFlashCoroutine
            );

            hitFlashCoroutine =
                null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color =
                Color.white;
        }

        Debug.Log(
            "Boss HP 0"
        );

        // =====================================================
        // 보스 처치 상태 저장
        // =====================================================

        PlayerPrefs.SetInt(
            BossDeadKey,
            1
        );

        PlayerPrefs.Save();

        // =====================================================
        // 사망 사운드
        // =====================================================

        PlaySound(
            deathSound
        );

        // =====================================================
        // 보스 사망 처리
        // =====================================================

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.BossDefeated();
        }
        if (bossController != null)
        {
            bossController.OnDeath();
        }
        else if (animator != null)
        {
            animator.SetTrigger(
                "Death"
            );
        }
    }

    // =========================================================
    // Audio
    // =========================================================

    private void PlaySound(
        AudioClip clip
    )
    {
        if (audioSource == null)
            return;

        if (clip == null)
            return;

        audioSource.PlayOneShot(
            clip
        );
    }

    // =========================================================
    // Get
    // =========================================================

    public int GetCurrentHp()
    {
        return currentHp;
    }

    public int GetMaxHp()
    {
        return maxHp;
    }

    public float GetHpPercent()
    {
        if (maxHp <= 0)
            return 0f;

        return
            (float)currentHp /
            maxHp;
    }

    public bool IsArmorBroken()
    {
        return armorBroken;
    }

    public bool IsDead()
    {
        return isDead;
    }
}