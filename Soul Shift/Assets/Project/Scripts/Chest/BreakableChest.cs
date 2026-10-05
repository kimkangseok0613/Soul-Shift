using UnityEngine;
using UnityEngine.SceneManagement;

public class BreakableChest : MonoBehaviour
{
    [Header("Chest ID")]
    [SerializeField] private string chestId;

    [Header("Chest")]
    [SerializeField] private int requiredHits = 3;

    [Header("Gem Drop")]
    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private int gemAmount = 1;

    [Header("Gem Launch")]
    [SerializeField] private float launchForceX = 1.5f;
    [SerializeField] private float launchForceY = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip chestHitSound;
    [SerializeField] private AudioClip chestOpenSound;

    private int hitCount;

    private Animator animator;
    private Collider2D chestCollider;

    private bool isOpening;
    private bool gemsSpawned;

    private string chestSaveKey;

    private const string ChestOpenedKeyPrefix = "ChestOpened_";

    private static readonly int HitHash =
        Animator.StringToHash("Hit");

    private static readonly int OpenHash =
        Animator.StringToHash("Open");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        chestCollider = GetComponent<Collider2D>();

        // AudioSource가 Inspector에 연결되지 않았다면
        // 같은 오브젝트에서 자동으로 찾음
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        chestSaveKey = GetChestSaveKey();

        // 이미 열었던 상자인지 확인
        if (PlayerPrefs.GetInt(chestSaveKey, 0) == 1)
        {
            Destroy(gameObject);
            return;
        }

        hitCount = 0;
        isOpening = false;
        gemsSpawned = false;
    }

    public void TakeDamage(int damage)
    {
        if (isOpening)
            return;

        // 데미지 수치는 사용하지 않고
        // 공격 1회를 맞은 것으로 처리
        hitCount++;

        Debug.Log(
            $"{gameObject.name} Hit : " +
            $"{hitCount} / {requiredHits}"
        );

        // 3번째 공격
        if (hitCount >= requiredHits)
        {
            OpenChest();
            return;
        }

        // 1~2번째 공격
        PlayHitSound();
        PlayHitAnimation();
    }

    private void PlayHitAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(OpenHash);
        animator.SetTrigger(HitHash);
    }

    private void PlayHitSound()
    {
        if (audioSource == null)
            return;

        if (chestHitSound == null)
            return;

        audioSource.PlayOneShot(chestHitSound);
    }

    private void PlayOpenSound()
    {
        if (audioSource == null)
            return;

        if (chestOpenSound == null)
            return;

        audioSource.PlayOneShot(chestOpenSound);
    }

    private void OpenChest()
    {
        if (isOpening)
            return;

        isOpening = true;

        // Chest Open 사운드
        PlayOpenSound();

        // 열린 Chest 기록 저장
        PlayerPrefs.SetInt(
            chestSaveKey,
            1
        );

        PlayerPrefs.Save();

        Debug.Log(
            $"Chest 저장 : {chestSaveKey}"
        );

        // 다시 공격되지 않도록
        if (chestCollider != null)
        {
            chestCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.ResetTrigger(HitHash);
            animator.SetTrigger(OpenHash);
        }
        else
        {
            SpawnGems();
            FinishChest();
        }
    }

    // Chest_Open 마지막 프레임 Animation Event
    public void SpawnGems()
    {
        if (gemsSpawned)
            return;

        gemsSpawned = true;

        if (gemPrefab == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} : " +
                "Gem Prefab이 연결되지 않았습니다."
            );

            return;
        }

        for (int i = 0; i < gemAmount; i++)
        {
            Vector3 spawnPosition =
                transform.position;

            GameObject gem =
                Instantiate(
                    gemPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            Rigidbody2D gemRb =
                gem.GetComponent<Rigidbody2D>();

            if (gemRb != null)
            {
                float randomX =
                    Random.Range(
                        -launchForceX,
                        launchForceX
                    );

                gemRb.AddForce(
                    new Vector2(
                        randomX,
                        launchForceY
                    ),
                    ForceMode2D.Impulse
                );
            }
        }

        Debug.Log(
            $"{gameObject.name} : " +
            $"Gem {gemAmount}개 생성"
        );
    }

    // Chest_Open 마지막 프레임 Animation Event
    public void FinishChest()
    {
        Destroy(gameObject);
    }

    private string GetChestSaveKey()
    {
        string sceneName =
            SceneManager.GetActiveScene().name;

        if (string.IsNullOrEmpty(chestId))
        {
            Debug.LogWarning(
                $"{gameObject.name} : " +
                "Chest ID가 비어 있습니다."
            );
        }

        return
            ChestOpenedKeyPrefix +
            sceneName +
            "_" +
            chestId;
    }

    public int GetHitCount()
    {
        return hitCount;
    }

    public int GetRequiredHits()
    {
        return requiredHits;
    }

    public bool IsOpening()
    {
        return isOpening;
    }
}