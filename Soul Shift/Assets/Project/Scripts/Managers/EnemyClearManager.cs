using UnityEngine;

public class EnemyClearManager : MonoBehaviour
{
    [Header("Exit Trigger")]
    [SerializeField] private GameObject leftExit;
    [SerializeField] private GameObject rightExit;

    [Header("Enemy Clear")]
    [SerializeField] private bool requireAllEnemiesDefeated = true;

    private bool isCleared;

    private void Start()
    {
        // 왼쪽 출구는 항상 열어둔다.
        SetLeftExit(true);

        // 오른쪽 출구만 적 처치 여부에 따라 결정한다.
        SetRightExit(false);

        CheckEnemyClear();
    }

    private void Update()
    {
        if (isCleared)
            return;

        CheckEnemyClear();
    }

    private void CheckEnemyClear()
    {
        if (!requireAllEnemiesDefeated)
        {
            ClearStage();
            return;
        }

        EnemyHealth[] enemies =
            FindObjectsByType<EnemyHealth>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        // 적이 한 마리도 없으면 클리어
        if (enemies.Length == 0)
        {
            ClearStage();
            return;
        }

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null)
                continue;

            if (!enemy.IsDead())
                return;
        }

        ClearStage();
    }

    private void ClearStage()
    {
        if (isCleared)
            return;

        isCleared = true;

        Debug.Log(
            $"{gameObject.scene.name} : 모든 적 처치 완료"
        );

        // 적을 모두 처치하면 오른쪽 출구만 연다.
        SetRightExit(true);

        // 왼쪽은 항상 열려있음
        SetLeftExit(true);
    }

    private void SetLeftExit(bool active)
    {
        if (leftExit != null)
            leftExit.SetActive(active);
    }

    private void SetRightExit(bool active)
    {
        if (rightExit != null)
            rightExit.SetActive(active);
    }
}