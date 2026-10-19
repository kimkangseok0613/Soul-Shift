using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Text")]
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "Map1-1";
    [SerializeField] private float completeDelay = 1.2f;

    private bool isCompleting;

    private void Start()
    {
        ShowMessage(
            "조작법을 알려드릴게요.\n오른쪽으로 이동하세요."
        );
    }

    public void ShowMessage(string message)
    {
        if (tutorialText == null)
            return;

        tutorialText.text = message;
    }

    public void CompleteTutorial(string message)
    {
        if (isCompleting)
            return;

        isCompleting = true;

        if (!string.IsNullOrEmpty(message))
            ShowMessage(message);
        else
            ShowMessage("튜토리얼 완료!\n잠시 후 Map1로 이동합니다.");

        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(completeDelay);

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning(
                "TutorialManager : 이동할 Scene 이름이 설정되지 않았습니다."
            );

            yield break;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }
}
