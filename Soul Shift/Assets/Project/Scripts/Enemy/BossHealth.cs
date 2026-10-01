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

    private void Awake()
    {
        animator =
            GetComponent<Animator>();

        bossController =
            GetComponent<BossController>();

        currentHp = maxHp;

        armorBroken = false;
        isDead = false;

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
        // HP 50% юлго
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