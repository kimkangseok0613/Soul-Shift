using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 0.7f;

    private TextMeshProUGUI damageText;

    private float timer;

    private void Awake()
    {
        damageText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        // 위쪽으로 일정한 속도로 이동
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 생성 후 일정 시간이 지나면 삭제
        timer += Time.deltaTime;

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamage(int damage)
    {
        if (damageText == null)
            return;

        damageText.text = damage.ToString();
    }
}