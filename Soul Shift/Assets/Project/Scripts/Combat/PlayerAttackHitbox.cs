using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;

    private BoxCollider2D boxCollider;

    // 이번 공격에서 이미 맞은 적
    private readonly HashSet<EnemyHealth> hitEnemies = new();

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        // 새로운 공격이 시작되면 적중 기록 초기화
        hitEnemies.Clear();

        // Hitbox가 켜지는 순간 이미 범위 안에 있는 적 검사
        DetectCurrentEnemies();
    }

    // =========================
    // 현재 공격 범위 안의 적 검사
    // =========================
    private void DetectCurrentEnemies()
    {
        if (boxCollider == null)
            return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            transform.eulerAngles.z
        );

        foreach (Collider2D hit in hits)
        {
            TryDamageEnemy(hit);
        }
    }

    // =========================
    // 이후 새로 들어오는 적 검사
    // =========================
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamageEnemy(other);
    }

    // =========================
    // 적에게 데미지 주기
    // =========================
    private void TryDamageEnemy(Collider2D other)
    {
        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy == null)
            return;

        // 같은 공격에서 같은 적을 중복 공격하지 않음
        if (hitEnemies.Contains(enemy))
            return;

        hitEnemies.Add(enemy);

        enemy.TakeDamage(damage);
    }

    // =========================
    // 에디터에서 공격 범위 표시
    // =========================
    private void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();

        if (box == null)
            return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(
            box.offset,
            box.size
        );
    }
}