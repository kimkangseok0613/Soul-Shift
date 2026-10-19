using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Save ID")]
    [SerializeField] private string bossId = "Map1_Boss";

    [Header("HP")]
    [SerializeField] private int maxHp = 100;

    [Header("Hit Effect")]
    [SerializeField] private float hitFlashDuration = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip armorBreakSound;
    [SerializeField] private AudioClip deathSound;

    private int currentHp;

    private bool armorBroken;
    private bool isDead;

    private Animator animator;
    private BossController bossController;
    private SpriteRenderer spriteRenderer;

    private Coroutine hitFlashCoroutine;

    private string BossDeadKey =>
        "BossDefeated_" + bossId;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        bossController =
            GetComponent<BossController>();

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        // SpriteRenderer가 자식에 있는 경우
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        currentHp = maxHp;
        armorBroken = false;
        isDead = false;

        // --------------------------------
        // 해당 보스의 처치 여부 확인
        // --------------------------------
        if (PlayerPrefs.GetInt(BossDeadKey, 0) == 1)
        {
            isDead = true;
            currentHp = 0;

            if (bossController != null)
            {
                bossController.OnDeath();
            }

            gameObject.SetActive(false);

            return;
        }

        // --------------------------------
        // 시작 시 갑옷 상태
        // --------------------------------
        if (animator != null)
        {
            animator.SetBool(
                "ArmorBroken",
                false
            );
        }
    }

    // =========================================================
    // Take Damage
    // =========================================================

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (damage <= 0)
            return;

        currentHp -= damage;

        currentHp = Mathf.Max(currentHp, 0);

        Debug.Log(
            $"{bossId} HP : {currentHp} / {maxHp}"
        );

        if (currentHp > 0)
        {
            StartHitFlash();

            // 일반 피격 사운드
            PlaySound(hitSound);
        }

        if (currentHp <= 0)
        {
            Die();
            return;
        }

        // HP가 50% 이하가 되면 갑옷 파괴
        if (!armorBroken &&
            currentHp <= maxHp * 0.5f)
        {
            BreakArmor();
            return;
        }

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

            spriteRenderer.color = Color.white;
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

        if (!isDead && spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
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

        Debug.Log($"{bossId} Armor Break!");

        PlaySound(armorBreakSound);

        if (animator != null)
        {
            animator.SetBool(
                "ArmorBroken",
                true
            );
        }

        if (bossController != null)
        {
            bossController.OnArmorBreak();
        }
        else if (animator != null)
        {
            animator.SetTrigger("ArmorBreak");
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

        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
            hitFlashCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        Debug.Log($"{bossId} HP 0 - Boss Defeated");

        // --------------------------------
        // 현재 보스만 처치 상태로 저장
        // --------------------------------
        PlayerPrefs.SetInt(BossDeadKey, 1);
        PlayerPrefs.Save();

        // 사망 사운드
        PlaySound(deathSound);

        // 보스 BGM에서 일반 BGM으로 전환
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.BossDefeated();
        }

        if (bossController != null)
        {
            bossController.OnDeath();
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
        if (audioSource == null)
            return;

        if (clip == null)
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

    public bool IsArmorBroken()
    {
        return armorBroken;
    }

    public bool IsDead()
    {
        return isDead;
    }
}