using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.2f;

    [Header("Attack Point")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject attackHitbox;

    private Animator animator;
    private PlayerController playerController;

    private bool canAttack = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // 히트스턴 중에는 공격 입력을 받지 않는다.
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