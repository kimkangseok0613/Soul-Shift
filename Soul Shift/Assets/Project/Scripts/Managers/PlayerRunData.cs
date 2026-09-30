using UnityEngine;

public static class PlayerRunData
{
    private static int savedHitCount;

    private static bool hasSavedHealth;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void Initialize()
    {
        savedHitCount = 0;
        hasSavedHealth = false;
    }

    public static void SaveHealth(
        int hitCount
    )
    {
        savedHitCount = hitCount;
        hasSavedHealth = true;

        Debug.Log(
            $"Player HP 저장 : " +
            $"HitCount = {savedHitCount}"
        );
    }

    public static bool HasSavedHealth()
    {
        return hasSavedHealth;
    }

    public static int GetSavedHitCount()
    {
        return savedHitCount;
    }

    public static void ResetHealth()
    {
        savedHitCount = 0;
        hasSavedHealth = false;

        Debug.Log(
            "Player HP 저장 데이터 초기화"
        );
    }
}