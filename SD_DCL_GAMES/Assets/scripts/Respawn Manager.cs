using System.Collections.Generic;
using NUnit.Framework;
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
        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].position = SpawnPoints[i].position;
        }
    }

    public void AddPlayer(Transform player)
    {
        Players.Add(player);
    }

}
