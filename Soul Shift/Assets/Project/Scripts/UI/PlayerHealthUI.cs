using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Heart")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Sprite fullHeartSprite;

    private PlayerHealth playerHealth;

    private readonly List<Image> hearts = new();

    private int lastMaxHeartCount = -1;
    private int lastRemainingHitCount = -1;

    private void Start()
    {
        TryFindPlayer();
        RefreshUI(true);
    }

    private void Update()
    {
        if (playerHealth == null)
        {
            TryFindPlayer();
            RefreshUI(true);
            return;
        }

        RefreshUI(false);
    }

    private void TryFindPlayer()
    {
        playerHealth =
            FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            lastMaxHeartCount = -1;
            lastRemainingHitCount = -1;
        }
    }

    private void RefreshUI(bool force)
    {
        if (playerHealth == null)
            return;

        int maxHeartCount =
            playerHealth.GetMaxHitCount();

        int remainingHitCount =
            playerHealth.GetRemainingHits();

        bool maxChanged =
            maxHeartCount != lastMaxHeartCount;

        bool healthChanged =
            remainingHitCount != lastRemainingHitCount;

        if (force || maxChanged)
        {
            RefreshHeartCount(maxHeartCount);
        }

        if (force ||
            maxChanged ||
            healthChanged)
        {
            UpdateHearts(remainingHitCount);
        }

        lastMaxHeartCount = maxHeartCount;
        lastRemainingHitCount = remainingHitCount;
    }

    private void RefreshHeartCount(
        int maxHeartCount
    )
    {
        if (heartPrefab == null)
        {
            Debug.LogWarning(
                "Heart Prefab이 연결되지 않았습니다."
            );

            return;
        }

        while (hearts.Count < maxHeartCount)
        {
            CreateHeart();
        }

        while (hearts.Count > maxHeartCount)
        {
            RemoveHeart();
        }
    }

    private void CreateHeart()
    {
        GameObject heartObject =
            Instantiate(
                heartPrefab,
                transform
            );

        Image heartImage =
            heartObject.GetComponent<Image>();

        if (heartImage == null)
        {
            Debug.LogWarning(
                "Heart Prefab에 Image 컴포넌트가 없습니다."
            );

            Destroy(heartObject);
            return;
        }

        if (fullHeartSprite != null)
        {
            heartImage.sprite =
                fullHeartSprite;
        }

        heartImage.gameObject.SetActive(true);

        hearts.Add(heartImage);
    }

    private void RemoveHeart()
    {
        if (hearts.Count == 0)
            return;

        int lastIndex =
            hearts.Count - 1;

        Image heart =
            hearts[lastIndex];

        hearts.RemoveAt(lastIndex);

        if (heart != null)
        {
            Destroy(heart.gameObject);
        }
    }

    private void UpdateHearts(
        int remainingHitCount
    )
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].gameObject.SetActive(
                i < remainingHitCount
            );
        }
    }
}