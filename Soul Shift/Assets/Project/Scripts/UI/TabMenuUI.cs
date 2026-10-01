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

    public void ReturnToTitle()
    {
        // 현재 플레이어의 HP 저장
        PlayerHealth playerHealth =
            FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.SaveHealthForSceneChange();

            Debug.Log(
                $"타이틀 이동 전 HP 저장 : " +
                $"{playerHealth.GetRemainingHits()} / " +
                $"{playerHealth.GetMaxHitCount()}"
            );
        }

        // 현재 씬 저장
        string currentSceneName =
            SceneManager.GetActiveScene().name;

        PlayerPrefs.SetString(
            ResumeSceneKey,
            currentSceneName
        );

        PlayerPrefs.Save();

        // 게임 정지 해제
        Time.timeScale = 1f;

        IsMenuOpen = false;

        Debug.Log(
            $"타이틀로 이동 - 저장된 씬 : {currentSceneName}"
        );

        SceneManager.LoadScene("Title");
    }
}