using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.2f;

    [Header("Attack Point")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject attackHitbox;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attack1Sound;
    [SerializeField] private AudioClip attack2Sound;
    [SerializeField] private AudioClip attack3Sound;

    private Animator animator;
    private PlayerController playerController;

    private bool canAttack = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // 스턴 중에는 공격 입력을 받지 않는다.
        if (playerController != null &&
            playerController.IsHitStunned())
        {
            return;
        }

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (!canAttack)
            return;

        if (playerController != null &&
            playerController.IsHitStunned())
        {
            return;
        }

        canAttack = false;

        animator.SetBool(
            "IsAttacking",
            true
        );

        animator.SetTrigger(
            "Attack"
        );

        Invoke(
            nameof(ResetAttackCooldown),
            attackCooldown
        );
    }

    private void ResetAttackCooldown()
    {
        canAttack = true;
    }

    // =========================================================
    // Attack Sound
    // Animation Event에서 호출
    // =========================================================

    public void PlayAttack1Sound()
    {
        PlaySound(attack1Sound);
    }

    public void PlayAttack2Sound()
    {
        PlaySound(attack2Sound);
    }

    public void PlayAttack3Sound()
    {
        PlaySound(attack3Sound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null)
            return;

        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    // =========================================================
    // Attack Hitbox
    // =========================================================

    public void EnableAttackHitbox()
    {
        if (attackHitbox == null)
            return;

        attackHitbox.SetActive(true);
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox == null)
            return;

        attackHitbox.SetActive(false);
    }

    public void EndAttack()
    {
        DisableAttackHitbox();

        animator.SetBool(
            "IsAttacking",
            false
        );
    }

    public void CancelAttack()
    {
        DisableAttackHitbox();

        animator.SetBool(
            "IsAttacking",
            false
        );
    }
}