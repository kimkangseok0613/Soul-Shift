using System.Collections.Generic;
using UnityEngine;

public static class ChestPersistence
{
    private static HashSet<string> openedChests =
        new HashSet<string>();

    // 플레이 시작 시 static 데이터 초기화
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void Initialize()
    {
        openedChests.Clear();
    }

    public static void MarkOpened(string chestId)
    {
        if (string.IsNullOrEmpty(chestId))
            return;

        openedChests.Add(chestId);

        Debug.Log(
            $"Chest Opened : {chestId}"
        );
    }

    public static bool IsOpened(string chestId)
    {
        if (string.IsNullOrEmpty(chestId))
            return false;

        return openedChests.Contains(chestId);
    }

    public static void ResetAll()
    {
        openedChests.Clear();

        Debug.Log(
            "Chest Persistence Reset"
        );
    }
}