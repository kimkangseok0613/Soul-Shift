using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int baseDamage = 10;

    private int damage;

    private BoxCollider2D boxCollider;

    private readonly HashSet<EnemyHealth> hitEnemies = new();
    private readonly HashSet<BossHealth> hitBosses = new();
    private readonly HashSet<BreakableChest> hitChests = new();

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        damage = baseDamage;
    }

    private void OnEnable()
    {
        hitEnemies.Clear();
        hitBosses.Clear();
        hitChests.Clear();

        DetectCurrentTargets();
    }

    private void DetectCurrentTargets()
    {
        if (boxCollider == null)
            return;

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                boxCollider.bounds.center,
                boxCollider.bounds.size,
                transform.eulerAngles.z
            );

        foreach (Collider2D hit in hits)
        {
            TryDamageTarget(hit);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamageTarget(other);
    }

    private void TryDamageTarget(Collider2D other)
    {
        if (other == null)
            return;

        // =====================================================
        // Normal Enemy
        // =====================================================

        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            if (!hitEnemies.Contains(enemy))
            {
                hitEnemies.Add(enemy);

                enemy.TakeDamage(damage);
            }

            return;
        }

        // =====================================================
        // Boss
        // =====================================================

        BossHealth boss =
            other.GetComponentInParent<BossHealth>();

        if (boss != null)
        {
            if (!hitBosses.Contains(boss))
            {
                hitBosses.Add(boss);

                boss.TakeDamage(damage);
            }

            return;
        }

        // =====================================================
        // Chest
        // =====================================================

        BreakableChest chest =
            other.GetComponentInParent<BreakableChest>();

        if (chest != null)
        {
            if (!hitChests.Contains(chest))
            {
                hitChests.Add(chest);

                chest.TakeDamage(damage);
            }
        }
    }

    // =========================================================
    // Damage
    // =========================================================

    public int GetDamage()
    {
        return damage;
    }

    public int GetBaseDamage()
    {
        return baseDamage;
    }

    public void SetUpgradeLevel(
        int upgradeLevel,
        int damagePerLevel
    )
    {
        if (upgradeLevel < 0)
            upgradeLevel = 0;

        if (damagePerLevel < 0)
            damagePerLevel = 0;

        damage =
            baseDamage +
            upgradeLevel *
            damagePerLevel;

        Debug.Log(
            $"공격력 적용 : " +
            $"기본 {baseDamage} / " +
            $"강화 Lv.{upgradeLevel} / " +
            $"현재 {damage}"
        );
    }

    // =========================================================
    // Gizmos
    // =========================================================

    private void OnDrawGizmos()
    {
        BoxCollider2D box =
            GetComponent<BoxCollider2D>();

        if (box == null)
            return;

        Gizmos.matrix =
            transform.localToWorldMatrix;

        Gizmos.DrawWireCube(
            box.offset,
            box.size
        );
    }
}