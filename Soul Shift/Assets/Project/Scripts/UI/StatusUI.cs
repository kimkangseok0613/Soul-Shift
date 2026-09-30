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

    private PlayerHealth playerHealth;
    private PlayerController playerController;

    private void OnEnable()
    {
        FindPlayer();
        UpdateUI();
    }

    private void Update()
    {
        if (playerHealth == null ||
            playerController == null)
        {
            FindPlayer();
        }

        UpdateUI();
    }

    private void FindPlayer()
    {
        playerHealth =
            FindFirstObjectByType<PlayerHealth>();

        playerController =
            FindFirstObjectByType<PlayerController>();
    }

    private void UpdateUI()
    {
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

        if (attackText != null)
        {
            if (PlayerUpgradeManager.Instance != null)
            {
                attackText.text =
                    $"atk : " +
                    $"{PlayerUpgradeManager.Instance.GetCurrentAttack()}";
            }
            else
            {
                attackText.text = "atk : -";
            }
        }

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