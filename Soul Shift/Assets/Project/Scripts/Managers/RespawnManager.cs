using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance
    {
        get;
        private set;
    }

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    private GameObject currentPlayer;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SpawnPlayer();
    }

    public void RespawnPlayer()
    {
        DestroyCurrentPlayer();
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (!CanSpawn())
            return;

        currentPlayer = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("Player Spawned");
    }

    private bool CanSpawn()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning(
                "Player Prefab이 연결되지 않았습니다."
            );

            return false;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "Spawn Point가 연결되지 않았습니다."
            );

            return false;
        }

        return true;
    }

    private void DestroyCurrentPlayer()
    {
        if (currentPlayer == null)
            return;

        Destroy(currentPlayer);
        currentPlayer = null;
    }

    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }
}