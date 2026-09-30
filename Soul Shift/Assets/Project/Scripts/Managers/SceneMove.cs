using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMove : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string targetSceneName;

    [Header("Player Spawn Direction")]
    [SerializeField]
    private RespawnManager.SpawnSide targetSpawnSide =
        RespawnManager.SpawnSide.Default;

    private bool isMoving;

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (isMoving)
            return;

        if (!other.CompareTag("Player"))
            return;

        MoveToScene(other);
    }

    private void MoveToScene(Collider2D player)
    {
        if (string.IsNullOrEmpty(
                targetSceneName))
        {
            Debug.LogWarning(
                $"{gameObject.name}에 " +
                "이동할 Scene 이름이 설정되지 않았습니다."
            );

            return;
        }

        isMoving = true;

        // 현재 플레이어 HP 저장
        PlayerHealth playerHealth =
            player.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.SaveHealthForSceneChange();
        }

        // 다음 씬에서 등장할 방향 저장
        RespawnManager.NextSpawnSide =
            targetSpawnSide;

        StartCoroutine(
            LoadScene()
        );
    }

    private IEnumerator LoadScene()
    {
        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                targetSceneName
            );

        if (operation == null)
        {
            Debug.LogWarning(
                $"Scene을 불러오지 못했습니다 : " +
                $"{targetSceneName}"
            );

            RespawnManager.NextSpawnSide =
                RespawnManager.SpawnSide.Default;

            isMoving = false;

            yield break;
        }

        yield return operation;
    }
}