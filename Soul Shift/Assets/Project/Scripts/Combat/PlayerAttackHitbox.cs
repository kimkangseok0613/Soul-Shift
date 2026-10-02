using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int baseDamage = 10;

    private int damage;
    private bool isInitialized;

    private BoxCollider2D boxCollider;

    private readonly HashSet<EnemyHealth> hitEnemies = new();
    private readonly HashSet<BossHealth> hitBosses = new();
    private readonly HashSet<BreakableChest> hitChests = new();

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        InitializeDamage();
    }

    private void OnEnable()
    {
        InitializeDamage();

        hitEnemies.Clear();
        hitBosses.Clear();
        hitChests.Clear();

        DetectCurrentTargets();
    }

    // ========================================
    // 공격력 초기화
    // ========================================
    private void InitializeDamage()
    {
        if (isInitialized)
            return;

        damage = baseDamage;
        isInitialized = true;
    }

    // ========================================
    // 현재 공격 범위 안의 적 / 보스 / 상자 확인
    // ========================================
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

    // ========================================
    // 공격 범위에 새로 들어온 대상 확인
    // ========================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamageTarget(other);
    }

    // ========================================
    // 적 / 보스 / 상자 공격
    // ========================================
    private void TryDamageTarget(Collider2D other)
    {
        if (other == null)
            return;

        // --------------------------------
        // 일반 적
        // --------------------------------
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

        // --------------------------------
        // 보스
        // --------------------------------
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

        // --------------------------------
        // 상자
        // --------------------------------
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

    // ========================================
    // 공격력 반환
    // ========================================
    public int GetDamage()
    {
        InitializeDamage();

        return damage;
    }

    public int GetBaseDamage()
    {
        return baseDamage;
    }

    // ========================================
    // 공격력 강화 적용
    // ========================================
    public void SetUpgradeLevel(
        int upgradeLevel,
        int damagePerLevel
    )
    {
        InitializeDamage();

        if (upgradeLevel < 0)
            upgradeLevel = 0;

        if (damagePerLevel < 0)
            damagePerLevel = 0;

        damage =
            baseDamage +
            upgradeLevel * damagePerLevel;

        Debug.Log(
            $"공격력 적용 : " +
            $"기본 {baseDamage} / " +
            $"강화 Lv.{upgradeLevel} / " +
            $"현재 {damage}"
        );
    }

    // ========================================
    // 공격 범위 표시
    // ========================================
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