using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public enum SpawnSide
    {
        Default,
        Left,
        Right
    }

    public static RespawnManager Instance
    {
        get;
        private set;
    }

    // 다음 씬에서 플레이어가 어느 쪽에 등장할지
    public static SpawnSide NextSpawnSide { get; set; } =
        SpawnSide.Default;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Default Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    [Header("Side Spawn Points")]
    [SerializeField] private Transform leftSpawnPoint;
    [SerializeField] private Transform rightSpawnPoint;

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

    public void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning(
                "Player Prefab이 연결되지 않았습니다."
            );

            return;
        }

        Transform selectedSpawnPoint =
            GetSpawnPoint();

        if (selectedSpawnPoint == null)
        {
            Debug.LogWarning(
                "Spawn Point가 연결되지 않았습니다."
            );

            return;
        }

        currentPlayer = Instantiate(
            playerPrefab,
            selectedSpawnPoint.position,
            selectedSpawnPoint.rotation
        );

        // 씬에 들어온 방향에 따라 바라보는 방향 설정
        SetPlayerFacingDirection();

        Debug.Log(
            $"Player Spawned : " +
            $"{selectedSpawnPoint.name}"
        );

        // 한 번 사용한 이동 방향 초기화
        NextSpawnSide =
            SpawnSide.Default;
    }

    private Transform GetSpawnPoint()
    {
        switch (NextSpawnSide)
        {
            case SpawnSide.Left:

                if (leftSpawnPoint != null)
                    return leftSpawnPoint;

                Debug.LogWarning(
                    "Left Spawn Point가 연결되지 않았습니다."
                );

                break;

            case SpawnSide.Right:

                if (rightSpawnPoint != null)
                    return rightSpawnPoint;

                Debug.LogWarning(
                    "Right Spawn Point가 연결되지 않았습니다."
                );

                break;
        }

        return spawnPoint;
    }

    private void SetPlayerFacingDirection()
    {
        if (currentPlayer == null)
            return;

        switch (NextSpawnSide)
        {
            case SpawnSide.Left:
                // 왼쪽에서 들어왔으므로 오른쪽을 바라봄
                FaceRight();
                break;

            case SpawnSide.Right:
                // 오른쪽에서 들어왔으므로 왼쪽을 바라봄
                FaceLeft();
                break;

            case SpawnSide.Default:
                // 기본 시작 위치에서는 프리팹 기본 방향 유지
                break;
        }
    }

    private void FaceRight()
    {
        Vector3 scale =
            currentPlayer.transform.localScale;

        currentPlayer.transform.localScale =
            new Vector3(
                Mathf.Abs(scale.x),
                scale.y,
                scale.z
            );
    }

    private void FaceLeft()
    {
        Vector3 scale =
            currentPlayer.transform.localScale;

        currentPlayer.transform.localScale =
            new Vector3(
                -Mathf.Abs(scale.x),
                scale.y,
                scale.z
            );
    }

    public void RespawnPlayer()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }

        // Retry하면 기본 위치에서 시작
        NextSpawnSide =
            SpawnSide.Default;

        SpawnPlayer();
    }

    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }
}