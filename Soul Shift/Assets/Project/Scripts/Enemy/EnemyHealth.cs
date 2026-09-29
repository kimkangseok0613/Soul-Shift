using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 30;

    [Header("Hit Effect")]
    [SerializeField] private float hitFlashDuration = 0.08f;

    [Header("Damage Popup")]
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField]
    private Vector3 damagePopupOffset =
        new Vector3(0f, 0.5f, 0f);

    [Header("Death")]
    [SerializeField] private float deathDelay = 0.8f;

    private int currentHealth;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private EnemyController enemyController;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;

    private Coroutine hitFlashCoroutine;

    private bool isDead;

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
    }

    public void TakeDamage(int damage)
    {
        if (isDead ||
            currentHealth <= 0)
        {
            return;
        }

        currentHealth -= damage;

        Debug.Log(
            $"{gameObject.name} HP : " +
            $"{currentHealth} / {maxHealth}"
        );

        ShowDamagePopup(damage);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        PlayHitReaction();
    }

    private void PlayHitReaction()
    {
        if (enemyController != null)
        {
            enemyController.OnHit();
        }

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        StartHitFlash();
    }

    private void StartHitFlash()
    {
        if (spriteRenderer == null)
            return;

        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
        }

        hitFlashCoroutine =
            StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        Color originalColor =
            spriteRenderer.color;

        spriteRenderer.color = Color.red;

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

    private void ShowDamagePopup(int damage)
    {
        if (damagePopupPrefab == null)
            return;

        Vector3 spawnPosition =
            transform.position +
            damagePopupOffset;

        GameObject popup =
            Instantiate(
                damagePopupPrefab,
                spawnPosition,
                Quaternion.identity
            );

        DamagePopup damagePopup =
            popup.GetComponent<DamagePopup>();

        if (damagePopup != null)
        {
            damagePopup.SetDamage(damage);
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Enemy Dead");

        DisableBodyCollision();
        StopBodyPhysics();
        StopHitFlash();
        ResetSpriteColor();
        DisableAttackHitbox();
        PlayDeathAnimation();

        StartCoroutine(
            DeathSequence()
        );
    }

    private void DisableBodyCollision()
    {
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
    }

    private void StopBodyPhysics()
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;
    }

    private void StopHitFlash()
    {
        if (hitFlashCoroutine == null)
            return;

        StopCoroutine(
            hitFlashCoroutine
        );

        hitFlashCoroutine = null;
    }

    private void ResetSpriteColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void DisableAttackHitbox()
    {
        if (enemyController != null)
        {
            enemyController.DisableAttackHitbox();
        }
    }

    private void PlayDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(
            deathDelay
        );

        Destroy(gameObject);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }
}