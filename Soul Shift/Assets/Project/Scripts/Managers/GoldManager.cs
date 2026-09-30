using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    [Header("Gold")]
    [SerializeField] private int startingGold = 0;

    private int currentGold;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ¾ÀÀÌ ¹Ù²î¾îµµ °ñµå À¯Áö
        DontDestroyOnLoad(gameObject);

        currentGold = startingGold;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        currentGold += amount;

        Debug.Log($"Gold +{amount} / Current Gold : {currentGold}");
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
            return false;

        if (currentGold < amount)
            return false;

        currentGold -= amount;

        Debug.Log($"Gold -{amount} / Current Gold : {currentGold}");

        return true;
    }

    public int GetGold()
    {
        return currentGold;
    }

    public void ResetGold()
    {
        currentGold = startingGold;

        Debug.Log($"Gold Reset : {currentGold}");
    }
}