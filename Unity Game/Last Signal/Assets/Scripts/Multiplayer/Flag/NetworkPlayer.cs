using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            MoveToSpawnPointServerRpc();
        }
    }

    [ServerRpc]
    private void MoveToSpawnPointServerRpc(ServerRpcParams rpcParams = default)
    {
        Transform spawnPoint = PlayerSpawner.Instance.GetNextSpawnPoint();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        // Ýstemcide pozisyonu güncelle
        UpdateClientPositionClientRpc(spawnPoint.position, spawnPoint.rotation);
    }

    [ClientRpc]
    private void UpdateClientPositionClientRpc(Vector3 position, Quaternion rotation)
    {
        if (!IsOwner)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}