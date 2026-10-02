using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 100;

    private int currentHp;

    private bool armorBroken;
    private bool isDead;

    private Animator animator;
    private BossController bossController;

    private const string BossDeadKey = "BossDefeated";

    private void Awake()
    {
        animator =
            GetComponent<Animator>();

        bossController =
            GetComponent<BossController>();

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
            // BossController / BossRoomManager에서
            // 문이나 이동 처리를 담당한다면 유지하기 위해 실행
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