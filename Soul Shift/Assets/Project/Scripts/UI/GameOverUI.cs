using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

    private const string PlayerCurrentHitCountKey =
        "PlayerCurrentHitCount";

    private const string BossSceneName =
        "Boss";

    private const string BossReturnSceneName =
        "Shop";

    private bool isGameOver;

    private void Awake()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "GameOverPanel이 연결되지 않았습니다."
            );
        }
    }

    public void ShowGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Debug.Log(
            "Show Game Over"
        );

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void Retry()
    {
        // =====================================================
        // 현재 Scene 확인
        // =====================================================

        string currentSceneName =
            SceneManager.GetActiveScene().name;

        // =====================================================
        // Retry 시 플레이어 HP 풀피
        // =====================================================

        PlayerPrefs.DeleteKey(
            PlayerCurrentHitCountKey
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Retry : 저장된 현재 HP 삭제 -> 풀피로 시작"
        );

        // =====================================================
        // Game Over UI 닫기
        // =====================================================

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        isGameOver = false;

        // 시간 정지 상태가 남아있지 않도록 복구
        Time.timeScale = 1f;

        // =====================================================
        // Boss Scene인 경우
        // =====================================================

        if (currentSceneName == BossSceneName)
        {
            if (RespawnManager.Instance != null)
            {
                // Shop의 오른쪽에서
                // Boss 방향으로 들어왔던 것이므로
                // Boss에서 Shop로 돌아오면
                // Shop의 오른쪽 Spawn Point 사용
                RespawnManager.NextSpawnSide =
                    RespawnManager.SpawnSide.Right;
            }

            Debug.Log(
                "Boss Retry : Shop으로 이동"
            );

            SceneManager.LoadScene(
                BossReturnSceneName
            );

            return;
        }

        // =====================================================
        // 일반 Scene Retry
        // =====================================================

        if (RespawnManager.Instance == null)
        {
            Debug.LogWarning(
                "RespawnManager를 찾을 수 없습니다."
            );

            return;
        }

        RespawnManager.Instance.RespawnPlayer();
    }
}