using System.Collections;
using UnityEngine;

public class SlimeBossHealth : MonoBehaviour
{
    [Header("Boss Save ID")]
    [SerializeField] private string bossId = "Map1_SlimeBoss";

    [Header("HP")]
    [SerializeField] private int maxHp = 100;

    [Header("Hit Effect")]
    [SerializeField] private float hitFlashDuration = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    private int currentHp;

    private bool isDead;

    private Animator animator;
    private SlimeBossController slimeBossController;
    private SpriteRenderer spriteRenderer;

    private Coroutine hitFlashCoroutine;

    private string BossDeadKey =>
        "BossDefeated_" + bossId;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        slimeBossController =
            GetComponent<SlimeBossController>();

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        currentHp = maxHp;

        if (PlayerPrefs.GetInt(BossDeadKey, 0) == 1)
        {
            isDead = true;
            currentHp = 0;

            // 이미 처치한 보스라면 비활성화
            gameObject.SetActive(false);
            return;
        }
    }

    // =========================================================
    // Take Damage
    // =========================================================

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHp = Mathf.Max(currentHp - damage, 0);

        Debug.Log(
            $"{bossId} HP: {currentHp} / {maxHp}"
        );

        if (currentHp <= 0)
        {
            Die();
            return;
        }

        PlaySound(hitSound);
        StartHitFlash();

        if (slimeBossController != null)
        {
            slimeBossController.OnHit();
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
            spriteRenderer.color = Color.white;
        }

        hitFlashCoroutine = StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        Color originalColor = spriteRenderer.color;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(hitFlashDuration);

        if (!isDead && spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        hitFlashCoroutine = null;
    }

    // =========================================================
    // Death
    // =========================================================

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
            hitFlashCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        // 이 보스만 처치 상태로 저장
        PlayerPrefs.SetInt(BossDeadKey, 1);
        PlayerPrefs.Save();

        Debug.Log(
            $"Slime Boss 처치 저장: {BossDeadKey}"
        );

        PlaySound(deathSound);

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.BossDefeated();
        }

        if (slimeBossController != null)
        {
            slimeBossController.OnDeath();
        }
        else if (animator != null)
        {
            animator.SetTrigger("Death");
        }
    }

    // =========================================================
    // Audio
    // =========================================================

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip);
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

        return (float)currentHp / maxHp;
    }

    public bool IsDead()
    {
        return isDead;
    }
}