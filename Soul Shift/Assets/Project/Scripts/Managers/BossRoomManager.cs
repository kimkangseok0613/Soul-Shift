using UnityEngine;

public class BossRoomManager : MonoBehaviour
{
    public static BossRoomManager Instance { get; private set; }

    [Header("Boss Room Door")]
    [SerializeField] private GameObject leftDoor;

    private const string BossDeadKey = "BossDefeated";

    private bool bossDefeated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 저장된 보스 처치 여부 확인
        bossDefeated =
            PlayerPrefs.GetInt(
                BossDeadKey,
                0
            ) == 1;
    }

    private void Start()
    {
        // 이미 보스를 처치했다면
        // 처음부터 왼쪽 문을 열어둔다.
        if (bossDefeated)
        {
            UnlockExit();

            Debug.Log(
                "Boss Room : 이미 보스 처치 완료 - 왼쪽 문 오픈"
            );

            return;
        }

        // 아직 보스를 잡지 않았다면 문 잠금
        LockExit();
    }

    // =========================================================
    // 문 잠금
    // =========================================================

    public void LockExit()
    {
        bossDefeated = false;

        if (leftDoor != null)
        {
            leftDoor.SetActive(true);
        }

        Debug.Log(
            "Boss Room : 보스 미처치 - 왼쪽 문 잠금"
        );
    }

    // =========================================================
    // 보스 처치 후 문 열기
    // =========================================================

    public void UnlockExit()
    {
        bossDefeated = true;

        if (leftDoor != null)
        {
            leftDoor.SetActive(false);
        }

        Debug.Log(
            "Boss Room : 보스 처치 완료 - 왼쪽 문 개방"
        );
    }

    // =========================================================
    // 보스 처치 여부
    // =========================================================

    public bool IsBossDefeated()
    {
        return bossDefeated;
    }
}