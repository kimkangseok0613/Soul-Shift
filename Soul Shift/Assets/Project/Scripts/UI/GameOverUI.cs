using UnityEngine;

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
                "GameOverPanel이 연결되지 않았습니다."
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
        if (RespawnManager.Instance == null)
        {
            Debug.LogWarning(
                "RespawnManager를 찾을 수 없습니다."
            );

            return;
        }

        // 게임 오버 UI 닫기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 게임 오버 상태 해제
        isGameOver = false;

        // 플레이어 다시 생성
        RespawnManager.Instance.RespawnPlayer();
    }
}