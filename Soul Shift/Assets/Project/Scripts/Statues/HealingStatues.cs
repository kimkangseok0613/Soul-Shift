using UnityEngine;
using UnityEngine.InputSystem;

public class HealingStatues : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    private PlayerHealth playerHealth;
    private bool playerInRange;

    private void Update()
    {
        // 탭 메뉴가 열려 있으면 상호작용하지 않음
        if (TabMenuUI.IsMenuOpen)
            return;

        // 플레이어가 범위 안에 있고 E를 눌렀을 때
        if (playerInRange &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerHealth =
            other.GetComponent<PlayerHealth>();

        // Player가 자식 오브젝트의 Collider를 사용한다면
        if (playerHealth == null)
        {
            playerHealth =
                other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null)
            return;

        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;
        playerHealth = null;
    }

    private void Interact()
    {
        if (playerHealth == null)
            return;

        playerHealth.RestoreFullHealth();

        Debug.Log("Healing Statue : 플레이어 HP 완전 회복");
    }
}