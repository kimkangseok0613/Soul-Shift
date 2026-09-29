using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.2f;

    [Header("Attack Hitbox")]
    [SerializeField] private GameObject attackHitbox;

    private Animator animator;
    private bool canAttack = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        DisableAttackHitbox();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (!canAttack)
            return;

        canAttack = false;

        animator.SetBool("IsAttacking", true);
        animator.SetTrigger("Attack");

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