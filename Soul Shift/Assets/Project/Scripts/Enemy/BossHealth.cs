using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 100;

    [Header("Hit Effect")]
    [SerializeField] private float hitFlashDuration = 0.08f;

    private int currentHp;

    private bool armorBroken;
    private bool isDead;

    private Animator animator;
    private BossController bossController;
    private SpriteRenderer spriteRenderer;

    private Coroutine hitFlashCoroutine;

    private const string BossDeadKey = "BossDefeated";

    private void Awake()
    {
        animator =
            GetComponent<Animator>();

        bossController =
            GetComponent<BossController>();

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        // BossHealth가 부모에 있고
        // SpriteRenderer가 자식에 있는 경우도 대응
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        currentHp = maxHp;

        armorBroken = false;
        isDead = false;

        // --------------------------------
        // 이전에 보스를 처치했는지 확인
        // --------------------------------
        if (PlayerPrefs.GetInt(BossDeadKey, 0) == 1)
        {
            isDead = true;
            currentHp = 0;

            // 보스 사망 처리를 다시 호출
            if (bossController != null)
            {
                bossController.OnDeath();
            }

            gameObject.SetActive(false);

            return;
        }

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
        // 죽지 않았다면 빨간색 피격 효과
        // =====================================================

        if (currentHp > 0)
        {
            StartHitFlash();
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
            animator.SetTrigger("Hit");
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
            StopCoroutine(hitFlashCoroutine);

            // 이전 효과가 끝나기 전에 다시 맞으면
            // 원래 색상으로 먼저 복구
            spriteRenderer.color = Color.white;
        }

        hitFlashCoroutine =
            StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        Color originalColor =
            spriteRenderer.color;

        // 빨간색
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

    // =========================================================
    // Armor Break
    // =========================================================

    private void BreakArmor()
    {
        if (armorBroken)
            return;

        armorBroken = true;

        if (animator != null)
        {
            animator.SetBool(
                "ArmorBroken",
                true
            );
        }

        Debug.Log(
            "Boss Armor Break!"
        );

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

        isDead = true;

        // 피격 플래시가 실행 중이면 종료
        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
            hitFlashCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        Debug.Log(
            "Boss HP 0"
        );

        // --------------------------------
        // 보스 처치 상태 저장
        // --------------------------------
        PlayerPrefs.SetInt(
            BossDeadKey,
            1
        );

        PlayerPrefs.Save();

        // --------------------------------
        // 보스 사망 처리
        // --------------------------------
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