using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

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
                "GameOverPanel이 연결되어 있지 않습니다."
            );
        }
    }

    public void ShowGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Debug.Log("Show Game Over");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void Retry()
    {
        // 게임 오버 상태 해제
        isGameOver = false;

        // 게임 오버 UI 닫기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 현재 씬 확인
        string currentSceneName =
            SceneManager.GetActiveScene().name;

        // =====================================================
        // Boss 씬
        // =====================================================

        if (currentSceneName == "Boss")
        {
            Debug.Log(
                "Boss 씬 Retry : Boss 씬 다시 로드"
            );

            Time.timeScale = 1f;

            SceneManager.LoadScene(
                currentSceneName
            );

            return;
        }

        // =====================================================
        // 일반 씬
        // =====================================================

        if (RespawnManager.Instance == null)
        {
            Debug.LogWarning(
                "RespawnManager를 찾을 수 없습니다."
            );

            return;
        }

        Time.timeScale = 1f;

        RespawnManager.Instance.RespawnPlayer();
    }
}