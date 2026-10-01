using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HealingStatues : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Interaction Text")]
    [SerializeField] private TextMeshPro healText;

    private PlayerHealth playerHealth;
    private bool playerInRange;

    private void Awake()
    {
        if (healText != null)
        {
            healText.gameObject.SetActive(false);
        }
    }

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

        // Player가 자식 오브젝트의 Collider를 사용하는 경우
        if (playerHealth == null)
        {
            playerHealth =
                other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null)
            return;

        playerInRange = true;

        if (healText != null)
        {
            healText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;
        playerHealth = null;

        if (healText != null)
        {
            healText.gameObject.SetActive(false);
        }
    }

    private void Interact()
    {
        if (playerHealth == null)
            return;

        playerHealth.RestoreFullHealth();

        Debug.Log(
            "Healing Statue : 플레이어 HP 완전 회복"
        );
    }
}