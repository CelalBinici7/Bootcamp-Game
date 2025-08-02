using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Multiplayer;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class JoinLobby : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyCode;
    string playerName;
    private RelayHostData _HostData;
    private RelayJoinData _JoinData;
    const string k_keyJoinCode = "RelayJoinCode";
    public GameObject networkmanager;
    public UnityTransport transport;
    public GameObject loadingUI;
    private void Start()
    {
        transport = networkmanager.GetComponent<UnityTransport>();
    }
    public async void JoinLobbyWithCode(string lobbycode)
    {
        var code = lobbyCode.text;
        string playerName = PlayerPrefs.GetString("PlayerName", "Player_" + UnityEngine.Random.Range(1000, 9999));

        try
        {
            // Lobiye katýlma
            var options = new JoinLobbyByCodeOptions
            {
                Player = new Player(AuthenticationService.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>()
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                { "PlayerLevel", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "8") },
                { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
            }
                }
            };

            var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);

            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;

            Debug.Log("Joined lobby with code: " + code);
            LobbyStatic.LogPlayersInLObby(lobby);

            loadingUI.SetActive(true);

            //relay
            string relayJoinCode = lobby.Data[k_keyJoinCode].Value;
            JoinAllocation joinAllocation = await Networkmanager.Instance.JoinRelay(relayJoinCode);

            _JoinData = new RelayJoinData()
            {
                IPv4Adress = joinAllocation.RelayServer.IpV4,
                Port = (ushort)joinAllocation.RelayServer.Port,
                AllocationId = joinAllocation.AllocationId,
                AllocationIDBytes = joinAllocation.AllocationIdBytes,
                ConnectionData = joinAllocation.ConnectionData,
                HostConnectionData = joinAllocation.HostConnectionData,
                Key = joinAllocation.Key
            };

            Debug.Log("Join Succes : " + _JoinData.AllocationId);
            
            transport.SetRelayServerData(_JoinData.IPv4Adress, _JoinData.Port, _JoinData.AllocationIDBytes, _JoinData.Key, _JoinData.ConnectionData, _JoinData.HostConnectionData);

            NetworkManager.Singleton.StartClient();
            LobbyStatic.LoadLobbyRoom();
            //end relay

            // Relay kodu kontrolü
            /* if (!lobby.Data.ContainsKey("RelayJoinCode") || string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value))
             {
                 Debug.LogError("RelayJoinCode bulunamadý. Host tarafýndan ayarlanmamýþ olabilir.");
                 return;
             }

             var joinCode = lobby.Data["RelayJoinCode"].Value;

             // Relay'e katýlma
             var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

             var relayData = new RelayJoinData()
             {
                 IPv4Adress = joinAllocation.RelayServer.IpV4,
                 Port = (ushort)joinAllocation.RelayServer.Port,
                 AllocationId = joinAllocation.AllocationId,
                 AllocationIDBytes = joinAllocation.AllocationIdBytes,
                 ConnectionData = joinAllocation.ConnectionData,
                 HostConnectionData = joinAllocation.HostConnectionData,
                 Key = joinAllocation.Key
             };

             var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
             if (transport == null)
             {
                 Debug.LogError("UnityTransport bileþeni bulunamadý!");
                 return;
             }

             transport.SetRelayServerData(
                 relayData.IPv4Adress,
                 relayData.Port,
                 relayData.AllocationIDBytes,
                 relayData.Key,
                 relayData.ConnectionData,
                 relayData.HostConnectionData
             );

             NetworkManager.Singleton.StartClient();*/
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobiye baðlanýrken hata: {e.Message}");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay baðlantýsýnda hata: {e.Message}");
        }
        /* var code = lobbyCode.text;
          playerName = playerName = PlayerPrefs.GetString("PlayerName", "Player_" + UnityEngine.Random.Range(1000, 9999)); ; ;
         try
         {
             JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions();
             options.Player = new Player(AuthenticationService.Instance.PlayerId);
             //sadece string deðerler taþýr

             options.Player.Data = new Dictionary<string, PlayerDataObject>()
             {

                 { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                 { "PlayerLevel",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,"8") },
                 { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
             };

             var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code,options);


             DontDestroyOnLoad(this);
             GetComponent<CurrentLobby>().currentLobby = lobby;

             Debug.Log("Joined lobby with code :" + code);
             LobbyStatic.LogPlayersInLObby(lobby);
             LobbyStatic.LoadLobbyRoom();

             ///
             var joinCode = lobby.Data["RelayJoinCode"].Value;

             // Relay'e katýl
             var joinAllocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);
             _JoinData = new RelayJoinData()
             {
                 IPv4Adress = joinAllocation.RelayServer.IpV4,
                 Port = (ushort)joinAllocation.RelayServer.Port,
                 AllocationId = joinAllocation.AllocationId,
                 AllocationIDBytes = joinAllocation.AllocationIdBytes,
                 ConnectionData = joinAllocation.ConnectionData,
                 HostConnectionData = joinAllocation.HostConnectionData,
                 Key = joinAllocation.Key
             };
             UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
             transport.SetRelayServerData(_JoinData.IPv4Adress, _JoinData.Port, _JoinData.AllocationIDBytes, _JoinData.Key, _JoinData.ConnectionData, _JoinData.HostConnectionData);

             // Network Manager ile oyuna katýl
             NetworkManager.Singleton.StartClient();
         }
         catch (LobbyServiceException e)
         {

             Debug.LogError(e);
         }*/
    }

    public async void JoinLobbyWithID(string lobbyId)
    {
        playerName = PlayerPrefs.GetString("PlayerName", "Player_" + UnityEngine.Random.Range(1000, 9999));
        try
        {
            var options = new JoinLobbyByIdOptions
            {
                Player = new Player(AuthenticationService.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                    { "PlayerLevel", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "8") },
                    { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
                }
                }
            };

            var lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;

            Debug.Log("Joined lobby with ID: " + lobbyId);
            Debug.LogWarning("Lobby Code: " + lobby.LobbyCode);

            LobbyStatic.LogPlayersInLObby(lobby);
         
            //relay
            loadingUI.SetActive(true);
            string relayJoinCode = lobby.Data[k_keyJoinCode].Value;
            JoinAllocation joinAllocation = await Networkmanager.Instance.JoinRelay(relayJoinCode);

            _JoinData = new RelayJoinData()
            {
                IPv4Adress = joinAllocation.RelayServer.IpV4,
                Port = (ushort)joinAllocation.RelayServer.Port,
                AllocationId = joinAllocation.AllocationId,
                AllocationIDBytes = joinAllocation.AllocationIdBytes,
                ConnectionData = joinAllocation.ConnectionData,
                HostConnectionData = joinAllocation.HostConnectionData,
                Key = joinAllocation.Key
            };

            Debug.Log("Join Succes : " + _JoinData.AllocationId);
           
            transport.SetRelayServerData(_JoinData.IPv4Adress, _JoinData.Port, _JoinData.AllocationIDBytes, _JoinData.Key, _JoinData.ConnectionData, _JoinData.HostConnectionData);

            NetworkManager.Singleton.StartClient();
            LobbyStatic.LoadLobbyRoom();
            //end relay


            // await ConnectToRelayAsClient(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobiye katýlýrken hata: {e.Message}");
        }
        /*  playerName = playerName = PlayerPrefs.GetString("PlayerName", "Player_" + UnityEngine.Random.Range(1000, 9999)); ; 
          try
          {
              JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();
              options.Player = new Player(AuthenticationService.Instance.PlayerId);
              //sadece string deðerler taþýr
              options.Player.Data = new Dictionary<string, PlayerDataObject>()
              {
                   { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                  { "PlayerLevel",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,"8") },
                  { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
              };
              var lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId,options);


              DontDestroyOnLoad(this);
              GetComponent<CurrentLobby>().currentLobby = lobby;

              Debug.Log("Joined lobby with ID :" + lobbyId);
              Debug.LogWarning("Lobby Code : " + lobby.LobbyCode);
              LobbyStatic.LogPlayersInLObby(lobby);
              LobbyStatic.LoadLobbyRoom();
          }
          catch (LobbyServiceException e)
          {

              Debug.LogError(e);
          }*/
    }

    public async void QuickJoinMethod()
    {
        playerName = PlayerPrefs.GetString("PlayerName", "Player_" + UnityEngine.Random.Range(1000, 9999));
        try
        {
            var options = new QuickJoinLobbyOptions
            {
                Player = new Player(AuthenticationService.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                    { "PlayerLevel", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "8") },
                    { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
                }
                }
            };

            var lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);

            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;

            Debug.Log("Joined lobby with QuickJoin: " + lobby.Id);
            Debug.LogWarning("Lobby Code: " + lobby.LobbyCode);

            LobbyStatic.LogPlayersInLObby(lobby);
            LobbyStatic.LoadLobbyRoom();

           // await ConnectToRelayAsClient(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"QuickJoin ile katýlýrken hata: {e.Message}");
        }
        /*  try
          {
              QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();
              options.Player = new Player(AuthenticationService.Instance.PlayerId);
              //sadece string deðerler taþýr
              options.Player.Data = new Dictionary<string, PlayerDataObject>()
              {
                  { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                  { "PlayerLevel",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,"8") },
                  { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
              };
              var lobby =await   LobbyService.Instance.QuickJoinLobbyAsync(options);

              Debug.Log("Joined lobby with QuickJoin :" + lobby.Id);
              Debug.LogWarning("Lobby Code : " + lobby.LobbyCode);
              LobbyStatic.LogPlayersInLObby(lobby);
              DontDestroyOnLoad(this);
              GetComponent<CurrentLobby>().currentLobby = lobby;
              LobbyStatic.LoadLobbyRoom();

          }
          catch (LobbyServiceException e)
          {

              Console.WriteLine(e);
          }*/
    }
    private async Task ConnectToRelayAsClient(Lobby lobby)
    {
        try
        {
            if (!lobby.Data.ContainsKey("RelayJoinCode") || string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value))
            {
                Debug.LogError("RelayJoinCode bulunamadý. Host Relay baþlatmamýþ olabilir.");
                return;
            }

            var joinCode = lobby.Data["RelayJoinCode"].Value;
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport bileþeni bulunamadý!");
                return;
            }

            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
            Debug.Log("Relay üzerinden client olarak baðlanýldý.");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay baðlantýsý baþarýsýz: {e.Message}");
        }
    }
}
// burada joinlerde lobby oyuncu listesi gncellenicek