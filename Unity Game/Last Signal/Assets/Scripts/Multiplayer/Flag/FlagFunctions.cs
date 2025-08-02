using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class FlagFunctions : NetworkBehaviour
{
    private bool isCaptured = false;
    public GameObject winpanel;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || isCaptured) return;

        if (other.CompareTag("Player"))
        {
            winpanel.SetActive(true);
            isCaptured = true;
        }
    }
}
