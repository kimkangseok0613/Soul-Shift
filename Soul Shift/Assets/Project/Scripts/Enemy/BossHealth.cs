using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 300;

    [Header("Armor")]
    [SerializeField] private int armorHitCount = 5;

    private int currentHp;
    private int currentArmorHitCount;

    private bool armorBroken;
    private bool isDead;

    private Animator animator;
    private BossController bossController;

    private Collider2D[] colliders;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        bossController = GetComponent<BossController>();

        colliders =
            GetComponentsInChildren<Collider2D>();

        currentHp = maxHp;
        currentArmorHitCount = armorHitCount;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (damage <= 0)
            return;

        currentHp -= damage;

        Debug.Log(
            $"Boss Hit : {currentHp} / {maxHp}"
        );

        if (!armorBroken)
        {
            currentArmorHitCount--;

            Debug.Log(
                $"Boss Armor : " +
                $"{Mathf.Max(currentArmorHitCount, 0)} / " +
                $"{armorHitCount}"
            );

            if (currentArmorHitCount <= 0)
            {
                BreakArmor();
            }
            else
            {
                if (bossController != null)
                {
                    bossController.OnHit();
                }
            }
        }
        else
        {
            if (bossController != null)
            {
                bossController.OnHit();
            }
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void BreakArmor()
    {
        if (armorBroken)
            return;

        armorBroken = true;
        currentArmorHitCount = 0;

        Debug.Log("Boss Armor Break!");

        if (bossController != null)
        {
            bossController.OnArmorBreak();
        }
        else if (animator != null)
        {
            animator.SetTrigger("ArmorBreak");
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Boss Death");

        if (bossController != null)
        {
            bossController.OnDeath();
        }

        // 죽은 보스가 물리적으로 플레이어를 막지 않도록 처리
        foreach (Collider2D col in colliders)
        {
            if (col == null)
                continue;

            if (col.GetComponentInParent<PlayerAttackHitbox>() == null &&
                col.GetComponentInParent<EnemyAttackHitbox>() == null)
            {
                col.enabled = false;
            }
        }

        if (animator != null &&
            bossController == null)
        {
            animator.SetTrigger("Death");
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

    public int GetCurrentArmorHitCount()
    {
        return Mathf.Max(currentArmorHitCount, 0);
    }

    public int GetMaxArmorHitCount()
    {
        return armorHitCount;
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