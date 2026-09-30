using UnityEngine;
using UnityEngine.InputSystem;

public class TabMenuUI : MonoBehaviour
{
    public static bool IsMenuOpen { get; private set; }

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
        {
            menuPanel.SetActive(true);
        }

        ShowStatus();

        // 게임 정지
        Time.timeScale = 0f;
    }

    public void CloseMenu()
    {
        IsMenuOpen = false;

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        // 게임 재개
        Time.timeScale = 1f;
    }

    public void ShowStatus()
    {
        if (statusPanel != null)
        {
            statusPanel.SetActive(true);
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void ShowInventory()
    {
        if (statusPanel != null)
        {
            statusPanel.SetActive(false);
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
    }
}