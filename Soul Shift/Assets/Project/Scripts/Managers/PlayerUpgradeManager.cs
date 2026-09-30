using UnityEngine;

public class PlayerUpgradeManager : MonoBehaviour
{
    public static PlayerUpgradeManager Instance { get; private set; }

    public enum UpgradeType
    {
        Attack,
        MoveSpeed,
        MaxHP
    }

    [Header("Base Stats")]
    [SerializeField] private int baseAttack = 10;
    [SerializeField] private float baseMoveSpeed = 3f;
    [SerializeField] private int baseMaxHP = 5;

    [Header("Attack")]
    [SerializeField] private int attackUpgradePerLevel = 2;

    [Header("Move Speed")]
    [SerializeField] private float moveSpeedUpgradePerLevel = 0.5f;

    [Header("Max HP")]
    [SerializeField] private int maxHPUpgradePerLevel = 1;

    private int attackLevel;
    private int moveSpeedLevel;
    private int maxHPLevel;

    private GameObject lastPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        TryApplyToPlayer();
    }

    private void Update()
    {
        GameObject currentPlayer =
            GameObject.FindGameObjectWithTag("Player");

        if (currentPlayer == null)
            return;

        if (currentPlayer != lastPlayer)
        {
            lastPlayer = currentPlayer;

            ApplyToPlayer(currentPlayer);
        }
    }

    // =========================================================
    // Player 적용
    // =========================================================

    private void TryApplyToPlayer()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        lastPlayer = player;

        ApplyToPlayer(player);
    }

    public void ApplyToPlayer(GameObject player)
    {
        if (player == null)
            return;

        PlayerAttackHitbox attackHitbox =
            player.GetComponentInChildren<PlayerAttackHitbox>(
                true
            );

        PlayerController playerController =
            player.GetComponent<PlayerController>();

        PlayerHealth playerHealth =
            player.GetComponent<PlayerHealth>();

        if (attackHitbox != null)
        {
            attackHitbox.SetUpgradeLevel(
                attackLevel,
                attackUpgradePerLevel
            );
        }

        if (playerController != null)
        {
            playerController.SetUpgradeLevel(
                moveSpeedLevel,
                moveSpeedUpgradePerLevel
            );
        }

        if (playerHealth != null)
        {
            playerHealth.SetUpgradeLevel(
                maxHPLevel,
                maxHPUpgradePerLevel
            );
        }

        Debug.Log(
            $"강화 적용 - " +
            $"ATK {GetCurrentAttack()}, " +
            $"SPD {GetCurrentMoveSpeed():0.0}, " +
            $"HP {GetCurrentMaxHP()}"
        );
    }

    // =========================================================
    // Upgrade
    // =========================================================

    public bool TryUpgrade(
        UpgradeType type,
        int cost
    )
    {
        if (GoldManager.Instance == null)
        {
            Debug.LogWarning(
                "GoldManager를 찾을 수 없습니다."
            );

            return false;
        }

        if (cost < 0)
        {
            Debug.LogWarning(
                "강화 비용은 0 이상이어야 합니다."
            );

            return false;
        }

        if (!GoldManager.Instance.SpendGold(cost))
        {
            Debug.Log(
                $"{GetUpgradeName(type)} 강화 실패 : 골드 부족"
            );

            return false;
        }

        switch (type)
        {
            case UpgradeType.Attack:
                attackLevel++;
                break;

            case UpgradeType.MoveSpeed:
                moveSpeedLevel++;
                break;

            case UpgradeType.MaxHP:
                maxHPLevel++;
                break;
        }

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            ApplyToPlayer(player);
        }

        Debug.Log(
            $"{GetUpgradeName(type)} 강화 완료"
        );

        return true;
    }

    // =========================================================
    // Current Stats
    // =========================================================

    public int GetCurrentAttack()
    {
        return
            baseAttack +
            attackLevel *
            attackUpgradePerLevel;
    }

    public float GetCurrentMoveSpeed()
    {
        return
            baseMoveSpeed +
            moveSpeedLevel *
            moveSpeedUpgradePerLevel;
    }

    public int GetCurrentMaxHP()
    {
        return
            baseMaxHP +
            maxHPLevel *
            maxHPUpgradePerLevel;
    }

    public int GetNextAttack()
    {
        return
            GetCurrentAttack() +
            attackUpgradePerLevel;
    }

    public float GetNextMoveSpeed()
    {
        return
            GetCurrentMoveSpeed() +
            moveSpeedUpgradePerLevel;
    }

    public int GetNextMaxHP()
    {
        return
            GetCurrentMaxHP() +
            maxHPUpgradePerLevel;
    }

    // =========================================================
    // Level
    // =========================================================

    public int GetLevel(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Attack:
                return attackLevel;

            case UpgradeType.MoveSpeed:
                return moveSpeedLevel;

            case UpgradeType.MaxHP:
                return maxHPLevel;
        }

        return 0;
    }

    public int GetAttackLevel()
    {
        return attackLevel;
    }

    public int GetMoveSpeedLevel()
    {
        return moveSpeedLevel;
    }

    public int GetMaxHPLevel()
    {
        return maxHPLevel;
    }

    // =========================================================
    // Name
    // =========================================================

    public string GetUpgradeName(
        UpgradeType type
    )
    {
        switch (type)
        {
            case UpgradeType.Attack:
                return "공격력";

            case UpgradeType.MoveSpeed:
                return "이동속도";

            case UpgradeType.MaxHP:
                return "최대 HP";
        }

        return "";
    }

    // =========================================================
    // Base Stats
    // =========================================================

    public int GetBaseAttack()
    {
        return baseAttack;
    }

    public float GetBaseMoveSpeed()
    {
        return baseMoveSpeed;
    }

    public int GetBaseMaxHP()
    {
        return baseMaxHP;
    }
}