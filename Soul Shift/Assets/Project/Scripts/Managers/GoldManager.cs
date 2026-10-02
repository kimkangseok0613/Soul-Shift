using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    [Header("Gold")]
    [SerializeField] private int startingGold = 0;

    private int currentGold;

    private const string GoldKey = "PlayerGold";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // 저장된 골드 불러오기
        currentGold =
            PlayerPrefs.GetInt(
                GoldKey,
                startingGold
            );
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        currentGold += amount;

        SaveGold();

        Debug.Log(
            $"Gold +{amount} / Current Gold : {currentGold}"
        );
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
            return false;

        if (currentGold < amount)
            return false;

        currentGold -= amount;

        SaveGold();

        Debug.Log(
            $"Gold -{amount} / Current Gold : {currentGold}"
        );

        return true;
    }

    public int GetGold()
    {
        return currentGold;
    }

    // ========================================
    // 골드 저장
    // ========================================
    private void SaveGold()
    {
        PlayerPrefs.SetInt(
            GoldKey,
            currentGold
        );

        PlayerPrefs.Save();
    }

    // ========================================
    // 골드 초기화
    // ========================================
    public void ResetGold()
    {
        currentGold = startingGold;

        SaveGold();

        Debug.Log(
            $"Gold Reset : {currentGold}"
        );
    }
}