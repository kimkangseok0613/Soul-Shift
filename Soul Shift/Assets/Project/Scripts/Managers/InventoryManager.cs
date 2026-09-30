using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Gem")]
    [SerializeField] private int startingGem = 0;

    private int gemCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        gemCount = startingGem;
    }

    public void AddGem(int amount)
    {
        if (amount <= 0)
            return;

        gemCount += amount;

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

        Debug.Log(
            $"Gem -{amount} / Current Gem : {gemCount}"
        );

        return true;
    }

    public int GetGemCount()
    {
        return gemCount;
    }

    public void ResetInventory()
    {
        gemCount = startingGem;

        Debug.Log(
            $"Inventory Reset / Gem : {gemCount}"
        );
    }
}