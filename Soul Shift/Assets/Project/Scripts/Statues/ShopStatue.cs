using UnityEngine;
using UnityEngine.InputSystem;

public class ShopStatue : MonoBehaviour
{
    public enum StatueType
    {
        Attack,
        MoveSpeed,
        MaxHP
    }

    [Header("Statue")]
    [SerializeField] private StatueType statueType;

    [Header("Upgrade")]
    [SerializeField] private int upgradeCost = 50;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip upgradeSuccessSound;

    private bool playerInRange;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (TabMenuUI.IsMenuOpen)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        Debug.Log(
            $"{GetStatueName()} 석상 상호작용 가능"
        );
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        Debug.Log(
            $"{GetStatueName()} 석상 상호작용 종료"
        );
    }

    private void Interact()
    {
        if (PlayerUpgradeManager.Instance == null)
        {
            Debug.LogWarning(
                "PlayerUpgradeManager를 찾을 수 없습니다."
            );

            return;
        }

        bool success =
            PlayerUpgradeManager.Instance.TryUpgrade(
                ConvertStatueType(),
                upgradeCost
            );

        if (success)
        {
            PlayUpgradeSuccessSound();

            Debug.Log(
                $"{GetStatueName()} 강화 완료"
            );
        }
    }

    // =========================================================
    // Audio
    // =========================================================

    private void PlayUpgradeSuccessSound()
    {
        if (audioSource == null)
            return;

        if (upgradeSuccessSound == null)
            return;

        audioSource.PlayOneShot(
            upgradeSuccessSound
        );
    }

    // =========================================================
    // Convert
    // =========================================================

    private PlayerUpgradeManager.UpgradeType
        ConvertStatueType()
    {
        switch (statueType)
        {
            case StatueType.Attack:
                return PlayerUpgradeManager.UpgradeType.Attack;

            case StatueType.MoveSpeed:
                return PlayerUpgradeManager.UpgradeType.MoveSpeed;

            case StatueType.MaxHP:
                return PlayerUpgradeManager.UpgradeType.MaxHP;
        }

        return PlayerUpgradeManager.UpgradeType.Attack;
    }

    // =========================================================
    // Statue Name
    // =========================================================

    private string GetStatueName()
    {
        switch (statueType)
        {
            case StatueType.Attack:
                return "공격력";

            case StatueType.MoveSpeed:
                return "이동속도";

            case StatueType.MaxHP:
                return "최대 HP";
        }

        return "알 수 없음";
    }

    // =========================================================
    // Get
    // =========================================================

    public StatueType GetStatueType()
    {
        return statueType;
    }

    public int GetUpgradeCost()
    {
        return upgradeCost;
    }

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }
}