using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance;

    [SerializeField] private Transform[] spawnPoints;
    private int nextSpawnIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    public Transform GetNextSpawnPoint()
    {
        Transform point = spawnPoints[nextSpawnIndex % spawnPoints.Length];
        nextSpawnIndex++;
        return point;
    }
}