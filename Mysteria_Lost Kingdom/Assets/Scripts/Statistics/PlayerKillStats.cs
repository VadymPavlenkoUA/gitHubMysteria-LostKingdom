using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKillStats : MonoBehaviour
{
    public static PlayerKillStats Instance;

    private Dictionary<string, int> kills = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterKill(string enemyId)
    {
        if (!kills.ContainsKey(enemyId))
            kills[enemyId] = 0;

        kills[enemyId]++;
        Debug.Log($"Kill registered: {enemyId} ({kills[enemyId]})");
    }

    public int GetKills(string enemyId)
    {
        return kills.TryGetValue(enemyId, out int count) ? count : 0;
    }

    public bool HasKilled(string enemyId)
    {
        return GetKills(enemyId) > 0;
    }

    public bool HasKilledUnique(string enemyId)
    {
        return kills.ContainsKey(enemyId);
    }
}
