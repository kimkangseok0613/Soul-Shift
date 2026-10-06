using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioClip normalBGM;
    [SerializeField] private AudioClip bossBGM;

    [Header("Boss Scene")]
    [SerializeField] private string bossSceneName = "Boss";

    [Header("Volume")]
    [SerializeField] private float normalVolume = 0.4f;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.4f;

    private AudioSource audioSource;

    private Coroutine fadeCoroutine;

    private AudioClip currentBGM;

    private bool bossDefeated;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        audioSource =
            GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayBGMForCurrentScene(true);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        PlayBGMForCurrentScene(false);
    }

    private void PlayBGMForCurrentScene(
        bool instant)
    {
        string sceneName =
            SceneManager.GetActiveScene().name;

        bool isBossScene =
            sceneName == bossSceneName;

        if (isBossScene &&
            !bossDefeated)
        {
            SwitchBGM(
                bossBGM,
                instant
            );
        }
        else
        {
            SwitchBGM(
                normalBGM,
                instant
            );
        }
    }

    private void SwitchBGM(
        AudioClip newBGM,
        bool instant)
    {
        if (newBGM == null)
        {
            Debug.LogWarning(
                "BGMManager : BGM이 연결되지 않았습니다."
            );

            return;
        }

        // 이미 같은 BGM이 재생 중이면
        // 아무것도 하지 않음
        if (currentBGM == newBGM &&
            audioSource.isPlaying)
        {
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (instant)
        {
            audioSource.Stop();

            audioSource.clip =
                newBGM;

            currentBGM =
                newBGM;

            audioSource.volume =
                normalVolume;

            audioSource.Play();

            return;
        }

        fadeCoroutine =
            StartCoroutine(
                ChangeBGM(newBGM)
            );
    }

    private IEnumerator ChangeBGM(
        AudioClip newBGM)
    {
        // -------------------------
        // Fade Out
        // -------------------------

        float startVolume =
            audioSource.volume;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                timer / fadeDuration;

            audioSource.volume =
                Mathf.Lerp(
                    startVolume,
                    0f,
                    t
                );

            yield return null;
        }

        audioSource.volume = 0f;

        // -------------------------
        // BGM 변경
        // -------------------------

        audioSource.Stop();

        audioSource.clip =
            newBGM;

        currentBGM =
            newBGM;

        audioSource.Play();

        // -------------------------
        // Fade In
        // -------------------------

        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                timer / fadeDuration;

            audioSource.volume =
                Mathf.Lerp(
                    0f,
                    normalVolume,
                    t
                );

            yield return null;
        }

        audioSource.volume =
            normalVolume;

        fadeCoroutine =
            null;
    }

    // =========================================================
    // Boss
    // =========================================================

    public void BossDefeated()
    {
        if (bossDefeated)
            return;

        bossDefeated = true;

        SwitchBGM(
            normalBGM,
            false
        );
    }

    // =========================================================
    // Public
    // =========================================================

    public void PlayNormalBGM()
    {
        bossDefeated = true;

        SwitchBGM(
            normalBGM,
            false
        );
    }

    public void PlayBossBGM()
    {
        bossDefeated = false;

        SwitchBGM(
            bossBGM,
            false
        );
    }
}