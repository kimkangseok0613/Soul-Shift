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

    private void Start()
    {
        FindPlayer();

        if (playerHealth != null)
        {
            RefreshHeartCount();
            UpdateHearts();
        }
    }

    private void Update()
    {
        // 플레이어가 없으면 새 플레이어 찾기
        if (playerHealth == null)
        {
            FindPlayer();

            if (playerHealth == null)
                return;

            RefreshHeartCount();
            UpdateHearts();

            return;
        }

        int currentMaxHeartCount =
            playerHealth.GetMaxHitCount();

        // 최대 하트 수 변경
        if (currentMaxHeartCount != lastMaxHeartCount)
        {
            RefreshHeartCount();
        }

        UpdateHearts();
    }

    private void FindPlayer()
    {
        playerHealth =
            FindFirstObjectByType<PlayerHealth>();
    }

    private void RefreshHeartCount()
    {
        if (playerHealth == null)
            return;

        if (heartPrefab == null)
        {
            Debug.LogWarning(
                "Heart Prefab이 연결되지 않았습니다."
            );

            return;
        }

        int maxHeartCount =
            playerHealth.GetMaxHitCount();

        // 하트 추가
        while (hearts.Count < maxHeartCount)
        {
            CreateHeart();
        }

        // 하트 제거
        while (hearts.Count > maxHeartCount)
        {
            RemoveHeart();
        }

        lastMaxHeartCount = maxHeartCount;
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
            heartImage.sprite = fullHeartSprite;
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

    private void UpdateHearts()
    {
        if (playerHealth == null)
            return;

        int remainingHits =
            playerHealth.GetRemainingHits();

        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].gameObject.SetActive(
                i < remainingHits
            );
        }
    }
}