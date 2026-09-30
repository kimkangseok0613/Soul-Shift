using UnityEngine;

public class BossRoomManager : MonoBehaviour
{
    public static BossRoomManager Instance { get; private set; }

    [Header("Boss Room Door")]
    [SerializeField] private GameObject leftDoor;

    private bool bossDefeated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
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

        Debug.Log("보스방 : 왼쪽 문 잠금");
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

        Debug.Log("보스방 : 보스 처치 완료 - 왼쪽 문 개방");
    }

    public bool IsBossDefeated()
    {
        return bossDefeated;
    }
}