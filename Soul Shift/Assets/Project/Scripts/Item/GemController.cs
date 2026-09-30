using UnityEngine;

public class GemController : MonoBehaviour
{
    private bool hasLanded;

    private void Awake()
    {
        hasLanded = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLanded)
            return;

        if (collision.collider.CompareTag("Ground"))
        {
            hasLanded = true;

            Debug.Log(
                $"{gameObject.name} 바닥에 착지했습니다. 이제 획득 가능합니다."
            );
        }
    }

    public bool HasLanded()
    {
        return hasLanded;
    }
}