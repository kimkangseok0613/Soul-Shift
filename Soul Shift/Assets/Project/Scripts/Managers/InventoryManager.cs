using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Gem")]
    [SerializeField] private int startingGem = 0;

    private int gemCount;

    private const string GemKey = "PlayerGem";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // 저장된 보석 불러오기
        gemCount =
            PlayerPrefs.GetInt(
                GemKey,
                startingGem
            );
    }

    public void AddGem(int amount)
    {
        if (amount <= 0)
            return;

        gemCount += amount;

        SaveInventory();

        Debug.Log(
            $"Gem +{amount} / Current Gem : {gemCount}"
        );
    }

    public bool RemoveGem(int amount)
    {
        if (amount <= 0)
            return false;

        if (gemCount < amount)
            return false;

        gemCount -= amount;

        SaveInventory();

        Debug.Log(
            $"Gem -{amount} / Current Gem : {gemCount}"
        );

        return true;
    }

    public int GetGemCount()
    {
        return gemCount;
    }

    // ========================================
    // 인벤토리 저장
    // ========================================
    private void SaveInventory()
    {
        PlayerPrefs.SetInt(
            GemKey,
            gemCount
        );

        PlayerPrefs.Save();
    }

    // ========================================
    // 인벤토리 초기화
    // ========================================
    public void ResetInventory()
    {
        gemCount = startingGem;

        SaveInventory();

        Debug.Log(
            $"Inventory Reset / Gem : {gemCount}"
        );
    }
}