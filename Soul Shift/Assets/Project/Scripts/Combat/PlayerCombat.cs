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

    private bool canAttack = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void Update()
    {
        // 메뉴가 열려 있으면 공격 입력 무시
        if (TabMenuUI.IsMenuOpen)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (!canAttack)
            return;

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