using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class RelayDisconnect : MonoBehaviour
{
    public static RelayDisconnect instance;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("Extra LobbyManager destroyed");
            Destroy(gameObject);
            return;
        }

        instance = this;
       
    }
    public void DisconnectRelay()
    {
        
        
        // 1. Relay baðlantýsý yoksa çýk
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Relay baðlantýsý yok, çýkýlýyor.");
            return;
        }

        // 2. Eðer host isek (host == server + client)
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Host: baðlantýyý kapatýyor.");
            NetworkManager.Singleton.Shutdown();
        }
        // 3. Eðer sadece client isek
        else if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Client: baðlantýyý kapatýyor.");
            NetworkManager.Singleton.Shutdown();
        }
    }
}
