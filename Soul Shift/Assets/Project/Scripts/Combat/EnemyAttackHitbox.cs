using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;

    private BoxCollider2D boxCollider;

    private readonly HashSet<PlayerHealth> hitPlayers = new();

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        hitPlayers.Clear();

        DetectCurrentPlayer();
    }

    private void DetectCurrentPlayer()
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
            TryDamagePlayer(hit);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (hitPlayers.Contains(playerHealth))
            return;

        hitPlayers.Add(playerHealth);

        // 공격자 위치 전달
        playerHealth.TakeDamage(
            damage,
            transform.root.position.x
        );
    }

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