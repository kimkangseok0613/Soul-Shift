using UnityEngine;

public class BossRoomManager2 : MonoBehaviour
{
    public static BossRoomManager2 Instance { get; private set; }

    [Header("Boss Room Door")]
    [SerializeField] private GameObject leftDoor;

    [Header("Boss Save ID")]
    [SerializeField] private string bossId = "Map2_SlimeBoss";

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

        bossDefeated = PlayerPrefs.GetInt(BossDeadKey, 0) == 1;

        UpdateDoor();
    }

    private void Start()
    {
        UpdateDoor();

        if (bossDefeated)
        {
            Debug.Log("슬라임 보스 처치 완료: 문 비활성화");
        }
        else
        {
            Debug.Log("슬라임 보스 생존: 문 활성화");
        }
    }

    public void LockExit()
    {
        bossDefeated = false;
        UpdateDoor();
    }

    public void UnlockExit()
    {
        bossDefeated = true;

        PlayerPrefs.SetInt(BossDeadKey, 1);
        PlayerPrefs.Save();

        UpdateDoor();

        Debug.Log("슬라임 보스 처치 완료: 문 비활성화");
    }

    private void UpdateDoor()
    {
        if (leftDoor == null)
        {
            Debug.LogWarning("BossRoomManager2: Left Door가 연결되지 않았습니다.");
            return;
        }

        leftDoor.SetActive(!bossDefeated);
    }

    public bool IsBossDefeated()
    {
        return bossDefeated;
    }
}