using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Manager")]
    [SerializeField] private TutorialManager tutorialManager;

    [Header("Message")]
    [TextArea(2, 4)]
    [SerializeField] private string message;

    [Header("Complete Tutorial")]
    [SerializeField] private bool completeTutorial;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (tutorialManager == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} : TutorialManager가 연결되지 않았습니다."
            );

            return;
        }

        triggered = true;

        if (completeTutorial)
        {
            tutorialManager.CompleteTutorial(message);
            return;
        }

        tutorialManager.ShowMessage(message);
    }
}
