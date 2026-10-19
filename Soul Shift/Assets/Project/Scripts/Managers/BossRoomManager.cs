using UnityEngine;

public class BossRoomManager : MonoBehaviour
{
    public static BossRoomManager Instance { get; private set; }

    [Header("Boss Room Door")]
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject rightDoor;

    [Header("Boss Save ID")]
    [SerializeField] private string bossId = "Map1_GolemBoss";

    private string BossDeadKey => "BossDefeated_" + bossId;

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
        bossDefeated = PlayerPrefs.GetInt(BossDeadKey, 0) == 1;
    }

    private void Start()
    {
        UpdateDoors();

        if (bossDefeated)
        {
            Debug.Log("Boss Room: 이미 보스 처치 완료 - 양쪽 문 비활성화");
        }
        else
        {
            Debug.Log("Boss Room: 보스 생존 - 양쪽 문 활성화");
        }
    }

    // 문 잠금
    public void LockExit()
    {
        bossDefeated = false;
        UpdateDoors();

        Debug.Log("Boss Room: 보스 미처치 - 양쪽 문 활성화");
    }

    // 보스 처치 후 문 열기
    public void UnlockExit()
    {
        bossDefeated = true;

        PlayerPrefs.SetInt(BossDeadKey, 1);
        PlayerPrefs.Save();

        UpdateDoors();

        Debug.Log("Boss Room: 보스 처치 완료 - 양쪽 문 비활성화");
    }

    // 문 상태 갱신
    private void UpdateDoors()
    {
        if (leftDoor != null)
        {
            leftDoor.SetActive(!bossDefeated);
        }

        if (rightDoor != null)
        {
            rightDoor.SetActive(!bossDefeated);
        }
    }

    // 보스 처치 여부
    public bool IsBossDefeated()
    {
        return bossDefeated;
    }
}