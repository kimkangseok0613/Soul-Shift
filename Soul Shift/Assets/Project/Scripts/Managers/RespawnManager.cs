using UnityEngine;
using UnityEngine.SceneManagement;

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

    // 다음 씬에서 플레이어가 어느 쪽에서 등장할지
    public static SpawnSide NextSpawnSide { get; set; } =
        SpawnSide.Default;

    // --------------------------------
    // Title에서 게임을 시작할 때
    // 저장 위치를 사용할지 여부
    // --------------------------------
    public static bool UseSavedPositionForResume { get; set; }

    // --------------------------------
    // 저장 데이터
    // --------------------------------
    private const string ResumeSceneKey = "ResumeScene";
    private const string ResumePositionXKey = "ResumePositionX";
    private const string ResumePositionYKey = "ResumePositionY";
    private const string ResumePositionZKey = "ResumePositionZ";

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

    // =========================================================
    // Game Exit
    // =========================================================

    private void OnApplicationQuit()
    {
        if (currentPlayer == null)
        {
            return;
        }

        // --------------------------------
        // 현재 HP 저장
        // --------------------------------
        PlayerHealth playerHealth =
            currentPlayer.GetComponent<PlayerHealth>();

        if (playerHealth != null &&
            !playerHealth.IsDead())
        {
            playerHealth.SaveHealthForSceneChange();

            Debug.Log(
                $"게임 종료 - HP 저장 : " +
                $"{playerHealth.GetRemainingHits()} / " +
                $"{playerHealth.GetMaxHitCount()}"
            );
        }

        // --------------------------------
        // 현재 위치 + 씬 저장
        // --------------------------------
        SaveCurrentPlayerPosition();

        Debug.Log(
            "게임 종료 - 현재 위치 및 HP 저장 완료"
        );
    }

    // =========================================================
    // Player 생성
    // =========================================================

    public void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning(
                "Player Prefab이 연결되지 않았습니다."
            );

            return;
        }

        // --------------------------------
        // 1. Title 복귀용 저장 위치 확인
        // --------------------------------
        if (UseSavedPositionForResume &&
            HasSavedPositionForCurrentScene())
        {
            Vector3 savedPosition =
                LoadSavedPosition();

            currentPlayer = Instantiate(
                playerPrefab,
                savedPosition,
                Quaternion.identity
            );

            Debug.Log(
                $"Player Resume Spawn : " +
                $"{savedPosition}"
            );

            // 저장 위치는 한 번만 사용
            UseSavedPositionForResume = false;

            // 일반 씬 이동용 SpawnSide 초기화
            NextSpawnSide =
                SpawnSide.Default;

            return;
        }

        // --------------------------------
        // 2. 기존 SpawnSide 방식
        // --------------------------------
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

        // 들어온 방향에 따라 바라보는 방향 설정
        SetPlayerFacingDirection();

        Debug.Log(
            $"Player Spawned : " +
            $"{selectedSpawnPoint.name}"
        );

        // 한 번 사용한 이동 방향 초기화
        NextSpawnSide =
            SpawnSide.Default;
    }

    // =========================================================
    // Spawn Point 선택
    // =========================================================

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

    // =========================================================
    // Player 방향 설정
    // =========================================================

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

                // 기본 시작 방향 유지
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

    // =========================================================
    // Retry
    // =========================================================

    public void RespawnPlayer()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }

        // Retry는 저장 위치를 사용하지 않음
        UseSavedPositionForResume = false;

        // Retry는 기본 위치에서 시작
        NextSpawnSide =
            SpawnSide.Default;

        SpawnPlayer();
    }

    // =========================================================
    // 현재 Player 반환
    // =========================================================

    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }

    // =========================================================
    // Title 복귀용 위치 저장
    // =========================================================

    public void SaveCurrentPlayerPosition()
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning(
                "현재 Player가 없어서 위치를 저장할 수 없습니다."
            );

            return;
        }

        string currentSceneName =
            SceneManager.GetActiveScene().name;

        Vector3 position =
            currentPlayer.transform.position;

        PlayerPrefs.SetString(
            ResumeSceneKey,
            currentSceneName
        );

        PlayerPrefs.SetFloat(
            ResumePositionXKey,
            position.x
        );

        PlayerPrefs.SetFloat(
            ResumePositionYKey,
            position.y
        );

        PlayerPrefs.SetFloat(
            ResumePositionZKey,
            position.z
        );

        PlayerPrefs.Save();

        Debug.Log(
            $"Player 위치 저장 : " +
            $"{currentSceneName} / " +
            $"{position}"
        );
    }

    // =========================================================
    // 저장된 위치가 현재 씬에 존재하는지 확인
    // =========================================================

    private bool HasSavedPositionForCurrentScene()
    {
        string savedScene =
            PlayerPrefs.GetString(
                ResumeSceneKey,
                string.Empty
            );

        if (string.IsNullOrEmpty(savedScene))
            return false;

        string currentSceneName =
            SceneManager.GetActiveScene().name;

        if (savedScene != currentSceneName)
            return false;

        if (!PlayerPrefs.HasKey(ResumePositionXKey))
            return false;

        if (!PlayerPrefs.HasKey(ResumePositionYKey))
            return false;

        if (!PlayerPrefs.HasKey(ResumePositionZKey))
            return false;

        return true;
    }

    // =========================================================
    // 저장된 위치 불러오기
    // =========================================================

    private Vector3 LoadSavedPosition()
    {
        float x =
            PlayerPrefs.GetFloat(
                ResumePositionXKey,
                0f
            );

        float y =
            PlayerPrefs.GetFloat(
                ResumePositionYKey,
                0f
            );

        float z =
            PlayerPrefs.GetFloat(
                ResumePositionZKey,
                0f
            );

        return new Vector3(
            x,
            y,
            z
        );
    }

    // =========================================================
    // Title에서 저장 위치 복구 준비
    // =========================================================

    public static void PrepareResumePosition()
    {
        string savedScene =
            PlayerPrefs.GetString(
                ResumeSceneKey,
                string.Empty
            );

        bool hasPosition =
            PlayerPrefs.HasKey(ResumePositionXKey) &&
            PlayerPrefs.HasKey(ResumePositionYKey) &&
            PlayerPrefs.HasKey(ResumePositionZKey);

        UseSavedPositionForResume =
            !string.IsNullOrEmpty(savedScene) &&
            hasPosition;
    }

    // =========================================================
    // 저장 위치 사용하지 않도록 초기화
    // =========================================================

    public static void ClearResumePositionFlag()
    {
        UseSavedPositionForResume = false;
    }
}