using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI goldText;

    private GoldManager goldManager;

    private void Start()
    {
        goldManager = GoldManager.Instance;

        if (goldManager == null)
        {
            Debug.LogWarning(
                "GoldManager를 찾을 수 없습니다."
            );

            return;
        }

        UpdateGoldText();
    }

    private void Update()
    {
        if (goldManager == null)
        {
            goldManager = GoldManager.Instance;

            if (goldManager == null)
                return;
        }

        UpdateGoldText();
    }

    private void UpdateGoldText()
    {
        if (goldText == null)
            return;

        goldText.text = goldManager.GetGold().ToString();
    }
}