using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Gem")]
    [SerializeField] private TextMeshProUGUI gemCountText;

    private void OnEnable()
    {
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (gemCountText == null)
            return;

        if (InventoryManager.Instance == null)
        {
            gemCountText.text = "0";
            return;
        }

        gemCountText.text =
            InventoryManager.Instance.GetGemCount().ToString();
    }
}