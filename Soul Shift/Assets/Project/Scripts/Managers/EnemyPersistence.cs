using System.Collections.Generic;
using UnityEngine;

public static class EnemyPersistence
{
    private static readonly HashSet<string> deadEnemies = new();

    public static void MarkDead(string enemyId)
    {
        if (string.IsNullOrEmpty(enemyId))
            return;

        deadEnemies.Add(enemyId);
    }

    public static bool IsDead(string enemyId)
    {
        if (string.IsNullOrEmpty(enemyId))
            return false;

        return deadEnemies.Contains(enemyId);
    }

    public static void ResetAll()
    {
        deadEnemies.Clear();

        Debug.Log("모든 적 처치 상태 초기화");
    }

    public static string CreateEnemyId(GameObject enemy)
    {
        if (enemy == null)
            return string.Empty;

        string sceneName = enemy.scene.name;
        string hierarchyPath = GetHierarchyPath(enemy.transform);

        return $"{sceneName}|{hierarchyPath}";
    }

    private static string GetHierarchyPath(Transform target)
    {
        string path = target.name;

        Transform current = target.parent;

        while (current != null)
        {
            path =
                $"{current.name}/{path}";

            current = current.parent;
        }

        return path;
    }
}