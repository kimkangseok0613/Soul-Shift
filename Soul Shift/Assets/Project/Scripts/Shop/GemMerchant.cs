using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GemMerchant : MonoBehaviour
{
    [Header("Sell Settings")]
    [SerializeField] private int gemSellPrice = 10;

    [Header("Interaction Text")]
    [SerializeField] private TextMeshPro interactionText;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    private bool playerInRange;

    private void Awake()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateInteractionText();

        if (!playerInRange)
            return;

        if (TabMenuUI.IsMenuOpen)
            return;

        if (InventoryManager.Instance == null)
            return;

        if (InventoryManager.Instance.GetGemCount() <= 0)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            SellGem();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;
    }

    // =========================================================
    // Interaction Text
    // =========================================================

    private void UpdateInteractionText()
    {
        if (interactionText == null)
            return;

        bool canSell = false;

        if (playerInRange &&
            InventoryManager.Instance != null &&
            InventoryManager.Instance.GetGemCount() > 0)
        {
            canSell = true;
        }

        interactionText.gameObject.SetActive(canSell);
    }

    // =========================================================
    // Sell Gem
    // =========================================================

    private void SellGem()
    {
        if (InventoryManager.Instance == null)
            return;

        if (GoldManager.Instance == null)
            return;

        int gemCount =
            InventoryManager.Instance.GetGemCount();

        if (gemCount <= 0)
        {
            return;
        }

        bool removed =
            InventoryManager.Instance.RemoveGem(1);

        if (!removed)
        {
            Debug.LogWarning(
                "보석 판매 실패 : 보석을 제거하지 못했습니다."
            );

            return;
        }

        GoldManager.Instance.AddGold(gemSellPrice);

        Debug.Log(
            $"보석 판매 : Gem -1 / Gold +{gemSellPrice}"
        );
    }

    // =========================================================
    // Get
    // =========================================================

    public int GetGemSellPrice()
    {
        return gemSellPrice;
    }
}