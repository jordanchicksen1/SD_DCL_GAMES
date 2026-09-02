using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField]
    public List<Transform> Players;
    [SerializeField]
    private List<Transform> SpawnPoints;

    private void Start()
    {
        RespawnPlayers();
    }

    public void RespawnPlayers()
    {
        if (Players.Count != SpawnPoints.Count)
        {
            Debug.LogWarning($"[RespawnManager] Players ({Players.Count}) and Spawn Points ({SpawnPoints.Count}) counts don't match - check the Inspector lists line up.");
        }

        int count = Mathf.Min(Players.Count, SpawnPoints.Count);
        for (int i = 0; i < count; i++)
        {
            if (Players[i] == null || SpawnPoints[i] == null)
            {
                Debug.LogWarning($"[RespawnManager] Players[{i}] or SpawnPoints[{i}] is empty - skipping.");
                continue;
            }

            Players[i].position = SpawnPoints[i].position;
        }

        Debug.Log($"[RespawnManager] Respawned {count} player(s).");
    }

    public void AddPlayer(Transform player)
    {
        Players.Add(player);
    }
}