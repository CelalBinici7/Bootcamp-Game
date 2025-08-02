using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TestServerRpc :NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            testServerRpc(0);
        }
    }
    [ClientRpc]
    public void TestClientRpc(int value)
    {
        if (IsClient) { 
        Debug.Log(" Client received the RPC " + value);
        testServerRpc(value++);
    }
    }
    [ServerRpc]
    public void testServerRpc(int value)
    {
        Debug.Log(" Server received the RPC " + value);
        TestClientRpc(value);
    }
}
