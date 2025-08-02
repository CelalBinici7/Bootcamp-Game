using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Vivox;
using UnityEngine;
public class Networkmanager : MonoBehaviour
{

    public static Networkmanager Instance;
    public Player LocalPlayer;
    public string playerName;
    public int maxPlayer =16;
    async void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await VivoxService.Instance.InitializeAsync();

            VivoxService.Instance.LoggedIn += () => Debug.Log("Vivox Giriþ Baþarýlý");
            VivoxService.Instance.LoggedOut += () => Debug.Log("Vivox Çýkýþ Yapýldý");

            await VivoxService.Instance.LoginAsync(new LoginOptions { DisplayName = PlayerPrefs.GetString("PlayerName") });
        }
        catch (Exception e)
        {
            Debug.LogError("Giriþ baþarýsýz: " + e.Message);
            // Offline moda geçebilirsiniz
        }

    }
    public void PreparePlayer()
    {
         playerName = PlayerPrefs.GetString("PlayerName", "Player_" + UnityEngine.Random.Range(1000, 9999));
     
    }
    private bool isQuitting = false;

    private void OnApplicationQuit()
    {
        isQuitting = true;
        ForceLeaveLobby();
    }

    private async void ForceLeaveLobby()
    {
        if (CurrentLobby.instance?.currentLobby != null &&
            AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                // Lobby'den oyuncuyu kaldýr
                await LobbyService.Instance.RemovePlayerAsync(
                    CurrentLobby.instance.currentLobby.Id,
                    AuthenticationService.Instance.PlayerId
                );

               
              

                Debug.Log("Lobby ve Relay baðlantýlarý kapatýldý");
            }
            catch (Exception e)
            {
                Debug.Log("Çýkýþ sýrasýnda hata: " + e.Message);
            }
        }
    }

   public async Task<Allocation> AllocateRealy()
    {
        try
        {
            
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayer-1);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("failed to allocate relay : " + e.Message);
            return default ;
        }
    }

   public async Task<string> GetRealyJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("failed to get relay join code : " + e.Message);
            return default;
        }
    }

   public async Task<JoinAllocation>  JoinRelay(string relayJoinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("failed to join relay : " + e.Message);
            return default;
        }
    }

   

}
