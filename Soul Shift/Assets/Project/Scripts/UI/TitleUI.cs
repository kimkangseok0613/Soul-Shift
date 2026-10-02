using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    [Header("Game Start")]
    [SerializeField] private string startSceneName = "Map1";

    [Header("Setting")]
    [SerializeField] private GameObject settingPanel;

    [SerializeField] private Slider volumeSlider;

    private const string VolumeKey = "MasterVolume";
    private const string ResumeSceneKey = "ResumeScene";

    private static bool resetHealthOnNextSceneLoad;
    private static bool resetGameOnNextStart;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (settingPanel != null)
            settingPanel.SetActive(false);

        LoadVolume();
    }

    // =========================================================
    // Game Start
    // =========================================================

    public void StartGame()
    {
        Time.timeScale = 1f;

        string sceneToLoad;

        // --------------------------------
        // Reset 직후
        // --------------------------------
        if (resetGameOnNextStart)
        {
            sceneToLoad =
                startSceneName;

            resetGameOnNextStart =
                false;

            // 새 게임이므로 저장 위치 사용 안 함
            RespawnManager
                .ClearResumePositionFlag();

            RespawnManager.NextSpawnSide =
                RespawnManager.SpawnSide.Default;

            Debug.Log(
                $"게임 리셋 상태 : " +
                $"{sceneToLoad}부터 새로 시작"
            );
        }
        else
        {
            // --------------------------------
            // 저장된 Scene 확인
            // --------------------------------
            string savedScene =
                PlayerPrefs.GetString(
                    ResumeSceneKey,
                    string.Empty
                );

            if (!string.IsNullOrEmpty(savedScene))
            {
                sceneToLoad =
                    savedScene;

                // 저장된 위치를 이번 씬 생성에서 사용
                RespawnManager
                    .PrepareResumePosition();

                // Title에서 게임 재개할 때는
                // SpawnSide 사용하지 않음
                RespawnManager.NextSpawnSide =
                    RespawnManager.SpawnSide.Default;

                Debug.Log(
                    $"저장된 게임 불러오기 : " +
                    $"{savedScene}"
                );
            }
            else
            {
                sceneToLoad =
                    startSceneName;

                // 저장 위치 없음
                RespawnManager
                    .ClearResumePositionFlag();

                RespawnManager.NextSpawnSide =
                    RespawnManager.SpawnSide.Default;

                Debug.Log(
                    $"저장된 게임 없음 - " +
                    $"새 게임 시작 : {startSceneName}"
                );
            }
        }

        // --------------------------------
        // 새 게임이면 HP 초기화
        // --------------------------------
        if (resetHealthOnNextSceneLoad)
        {
            SceneManager.sceneLoaded +=
                ResetPlayerHealthAfterSceneLoad;
        }

        SceneManager.LoadScene(
            sceneToLoad
        );
    }

    // =========================================================
    // Reset
    // =========================================================

    public void ResetGameProgress()
    {
        Debug.Log(
            "게임 진행 데이터 초기화 시작"
        );

        // 현재 설정값인 볼륨은 유지
        float currentVolume =
            PlayerPrefs.GetFloat(
                VolumeKey,
                1f
            );

        // 모든 저장 데이터 삭제
        PlayerPrefs.DeleteAll();

        // 볼륨 복구
        PlayerPrefs.SetFloat(
            VolumeKey,
            currentVolume
        );

        // 저장된 Scene 제거
        PlayerPrefs.DeleteKey(
            ResumeSceneKey
        );

        PlayerPrefs.Save();

        // 저장 위치 복귀 플래그 초기화
        RespawnManager
            .ClearResumePositionFlag();

        RespawnManager.NextSpawnSide =
            RespawnManager.SpawnSide.Default;

        // 다음 Start에서는 Map1부터 시작
        resetGameOnNextStart = true;

        // 다음 씬 로드 후 HP도 기본값으로 초기화
        resetHealthOnNextSceneLoad = true;

        // DontDestroyOnLoad 매니저 초기화
        ResetPersistentManagers();

        Time.timeScale = 1f;

        Debug.Log(
            "게임 진행 데이터 초기화 완료 - " +
            "다음 시작 시 Map1부터 다시 시작합니다."
        );
    }

    private void ResetPersistentManagers()
    {
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.ResetGold();
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ResetInventory();
        }

        if (PlayerUpgradeManager.Instance != null)
        {
            PlayerUpgradeManager.Instance.ResetUpgrades();
        }
    }

    // =========================================================
    // 새 게임 HP 초기화
    // =========================================================

    private static void ResetPlayerHealthAfterSceneLoad(
        Scene scene,
        LoadSceneMode mode
    )
    {
        SceneManager.sceneLoaded -=
            ResetPlayerHealthAfterSceneLoad;

        PlayerHealth playerHealth =
            Object.FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.ResetHealth();

            Debug.Log(
                "새 게임 시작 : " +
                "HP 기본값 + 풀피 적용"
            );
        }
        else
        {
            Debug.LogWarning(
                "새 게임 시작 후 " +
                "PlayerHealth를 찾지 못했습니다."
            );
        }

        resetHealthOnNextSceneLoad =
            false;
    }

    // =========================================================
    // Setting
    // =========================================================

    public void OpenSetting()
    {
        if (settingPanel == null)
            return;

        settingPanel.SetActive(true);

        if (volumeSlider != null)
        {
            volumeSlider.value =
                AudioListener.volume;
        }
    }

    public void CloseSetting()
    {
        if (settingPanel == null)
            return;

        settingPanel.SetActive(false);
    }

    private void LoadVolume()
    {
        float savedVolume =
            PlayerPrefs.GetFloat(
                VolumeKey,
                1f
            );

        AudioListener.volume =
            savedVolume;

        if (volumeSlider != null)
        {
            volumeSlider.value =
                savedVolume;

            volumeSlider.onValueChanged.AddListener(
                SetVolume
            );
        }
    }

    public void SetVolume(float value)
    {
        value =
            Mathf.Clamp01(value);

        AudioListener.volume =
            value;

        PlayerPrefs.SetFloat(
            VolumeKey,
            value
        );

        PlayerPrefs.Save();
    }

    // =========================================================
    // Quit
    // =========================================================

    public void QuitGame()
    {
        Debug.Log(
            "게임 종료"
        );

        Application.Quit();
    }
}