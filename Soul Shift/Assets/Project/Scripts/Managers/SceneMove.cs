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

    private void MoveToScene(
        Collider2D player
    )
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

        // --------------------------------
        // 일반 씬 이동에서는
        // 저장 위치를 사용하지 않음
        // --------------------------------
        RespawnManager
            .ClearResumePositionFlag();

        // --------------------------------
        // 현재 Player HP 저장
        // --------------------------------
        PlayerHealth playerHealth =
            player.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.SaveHealthForSceneChange();
        }

        // --------------------------------
        // 다음 씬 SpawnSide 설정
        // --------------------------------
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

            RespawnManager
                .ClearResumePositionFlag();

            isMoving = false;

            yield break;
        }

        yield return operation;
    }
}