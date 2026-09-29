using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

    private bool isGameOver;

    private void Awake()
    {
        HideGameOver();
    }

    public void ShowGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "GameOverPanel이 연결되지 않았습니다."
            );
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

        RespawnManager.Instance.RespawnPlayer();

        HideGameOver();
    }

    private void HideGameOver()
    {
        isGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
}