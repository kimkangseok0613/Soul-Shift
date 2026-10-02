using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TabMenuUI : MonoBehaviour
{
    public static bool IsMenuOpen { get; private set; }

    private const string ResumeSceneKey = "ResumeScene";

    [Header("Menu")]
    [SerializeField] private GameObject menuPanel;

    [Header("Tab Panels")]
    [SerializeField] private GameObject statusPanel;

    [SerializeField] private GameObject inventoryPanel;

    private void Awake()
    {
        IsMenuOpen = false;

        CloseMenu();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (IsMenuOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        IsMenuOpen = true;

        if (menuPanel != null)
            menuPanel.SetActive(true);

        ShowStatus();

        Time.timeScale = 0f;
    }

    public void CloseMenu()
    {
        IsMenuOpen = false;

        if (menuPanel != null)
            menuPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ShowStatus()
    {
        if (statusPanel != null)
            statusPanel.SetActive(true);

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    public void ShowInventory()
    {
        if (statusPanel != null)
            statusPanel.SetActive(false);

        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);
    }

    // =========================================================
    // Title 이동
    // =========================================================

    public void ReturnToTitle()
    {
        // --------------------------------
        // 현재 Player 확인
        // --------------------------------
        PlayerHealth playerHealth =
            FindFirstObjectByType<PlayerHealth>();

        // --------------------------------
        // HP 저장
        // --------------------------------
        if (playerHealth != null)
        {
            playerHealth.SaveHealthForSceneChange();

            Debug.Log(
                $"Title 이동 전 HP 저장 : " +
                $"{playerHealth.GetRemainingHits()} / " +
                $"{playerHealth.GetMaxHitCount()}"
            );
        }

        // --------------------------------
        // 현재 Player 위치 저장
        // --------------------------------
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance
                .SaveCurrentPlayerPosition();
        }
        else
        {
            Debug.LogWarning(
                "RespawnManager를 찾을 수 없습니다."
            );
        }

        // --------------------------------
        // 현재 Scene 저장
        // --------------------------------
        string currentSceneName =
            SceneManager
                .GetActiveScene()
                .name;

        // RespawnManager에서 이미 저장하지만
        // 기존 ResumeScene 기능도 유지
        PlayerPrefs.SetString(
            ResumeSceneKey,
            currentSceneName
        );

        PlayerPrefs.Save();

        // --------------------------------
        // 게임 정지 해제
        // --------------------------------
        Time.timeScale = 1f;

        IsMenuOpen = false;

        Debug.Log(
            $"Title로 이동 - " +
            $"저장된 Scene : {currentSceneName}"
        );

        SceneManager.LoadScene("Title");
    }
}