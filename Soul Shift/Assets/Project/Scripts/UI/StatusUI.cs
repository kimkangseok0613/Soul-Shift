using TMPro;
using UnityEngine;

public class StatusUI : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Attack")]
    [SerializeField] private TextMeshProUGUI attackText;

    [Header("Move Speed")]
    [SerializeField] private TextMeshProUGUI moveSpeedText;

    [Header("Gold")]
    [SerializeField] private TextMeshProUGUI goldText;

    private GameObject currentPlayer;

    private PlayerHealth playerHealth;
    private PlayerAttackHitbox playerAttackHitbox;
    private PlayerController playerController;

    private void OnEnable()
    {
        FindPlayer();
        UpdateUI();
    }

    private void Update()
    {
        GameObject player = GetCurrentPlayer();

        // 플레이어가 새로 생성되었으면 다시 찾기
        if (player != currentPlayer ||
            playerHealth == null ||
            playerAttackHitbox == null ||
            playerController == null)
        {
            FindPlayer();
        }

        UpdateUI();
    }

    // ========================================
    // 현재 생성된 Player 찾기
    // ========================================
    private GameObject GetCurrentPlayer()
    {
        // RespawnManager에서 현재 Player 가져오기
        if (RespawnManager.Instance != null)
        {
            GameObject player =
                RespawnManager.Instance.GetCurrentPlayer();

            if (player != null)
            {
                return player;
            }
        }

        // 혹시 RespawnManager가 아직 Player를 저장하지 못한 경우
        PlayerHealth health =
            FindFirstObjectByType<PlayerHealth>();

        if (health != null)
        {
            return health.gameObject;
        }

        return null;
    }

    // ========================================
    // Player와 관련 컴포넌트 다시 찾기
    // ========================================
    private void FindPlayer()
    {
        currentPlayer = GetCurrentPlayer();

        if (currentPlayer == null)
        {
            playerHealth = null;
            playerAttackHitbox = null;
            playerController = null;

            return;
        }

        // Player 본체
        playerHealth =
            currentPlayer.GetComponent<PlayerHealth>();

        playerController =
            currentPlayer.GetComponent<PlayerController>();

        // Player 자식까지 포함해서 AttackHitbox 찾기
        playerAttackHitbox =
            currentPlayer.GetComponentInChildren<PlayerAttackHitbox>(
                true
            );
    }

    // ========================================
    // UI 업데이트
    // ========================================
    private void UpdateUI()
    {
        // --------------------------------
        // HP
        // --------------------------------
        if (hpText != null)
        {
            if (playerHealth != null)
            {
                hpText.text =
                    $"hp  : {playerHealth.GetRemainingHits()} / " +
                    $"{playerHealth.GetMaxHitCount()}";
            }
            else
            {
                hpText.text = "hp  : -";
            }
        }

        // --------------------------------
        // Attack
        // --------------------------------
        if (attackText != null)
        {
            if (playerAttackHitbox != null)
            {
                attackText.text =
                    $"atk : {playerAttackHitbox.GetDamage()}";
            }
            else
            {
                attackText.text = "atk : -";
            }
        }

        // --------------------------------
        // Move Speed
        // --------------------------------
        if (moveSpeedText != null)
        {
            if (playerController != null)
            {
                moveSpeedText.text =
                    $"spd : {playerController.GetMoveSpeed():0.0}";
            }
            else
            {
                moveSpeedText.text = "spd : -";
            }
        }

        // --------------------------------
        // Gold
        // --------------------------------
        if (goldText != null)
        {
            if (GoldManager.Instance != null)
            {
                goldText.text =
                    $"gold : {GoldManager.Instance.GetGold()}";
            }
            else
            {
                goldText.text = "gold : 0";
            }
        }
    }
}