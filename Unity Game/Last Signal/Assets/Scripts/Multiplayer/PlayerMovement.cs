using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
   public NetworkVariable<Vector3> position =new NetworkVariable<Vector3>();
    void Start()
    {
        
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            move();
        }
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = position.Value;
    }

    public void move()
    {

        if (NetworkManager.IsServer)
        {
            var randomPos = setRandomPointOnPlane();
            transform.position = randomPos;
            position.Value = randomPos;
        }
        else {
            submitPositionRequestOnServerRpc();//fonksyon sobu rpc ve tag olarak rpc eklenmeli
        }
    }
    [ServerRpc]
     void submitPositionRequestOnServerRpc(ServerRpcParams rpcParams = default) {
        position.Value = setRandomPointOnPlane();
    }

    static Vector3 setRandomPointOnPlane()
    {
        return new Vector3(Random.Range(-3,3),1f,Random.Range(-3,3));
    }
}
