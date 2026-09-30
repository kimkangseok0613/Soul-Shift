using UnityEngine;

public class GemPickup : MonoBehaviour
{
    [Header("Gem")]
    [SerializeField] private int gemAmount = 1;

    private bool collected;

    private GemController gemController;

    private void Awake()
    {
        gemController =
            GetComponentInParent<GemController>();

        if (gemController == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} : " +
                "GemController를 찾을 수 없습니다."
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        if (!other.CompareTag("Player"))
            return;

        // 아직 바닥에 떨어지지 않았다면 획득하지 않음
        if (gemController != null &&
            !gemController.HasLanded())
        {
            return;
        }

        Collect();
    }

    private void Collect()
    {
        if (collected)
            return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning(
                "InventoryManager를 찾을 수 없습니다."
            );

            return;
        }

        collected = true;

        InventoryManager.Instance.AddGem(
            gemAmount
        );

        Debug.Log(
            $"Gem 획득 : +{gemAmount}"
        );

        // Gem 전체 삭제
        Destroy(
            transform.root.gameObject
        );
    }
}