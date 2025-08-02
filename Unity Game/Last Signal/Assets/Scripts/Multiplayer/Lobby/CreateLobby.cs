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

public class CreateLobby : MonoBehaviour
{
    private RelayHostData _HostData;
    private RelayJoinData _JoinData;
    public TMP_Dropdown maxPlayer;
    public TMP_Dropdown gameMode;
    public TMP_InputField lobbyName;
    public TextMeshProUGUI lobbyCode; // nu deðiþken lobby kodunu gösterdiðimiz yer lobi sahnesinde de olabilir þimdilik main sahnede olucak
    public Toggle isPrivate;
    string playerName;
    Coroutine heartbeatCoroutine;
    const string k_keyJoinCode = "RelayJoinCode";
    CreateLobby instance;
    public GameObject networkmanager;
    public UnityTransport transport;
    public GameObject loadingUI;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("Extra LobbyManager destroyed");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        transport = networkmanager.GetComponent<UnityTransport>();
    }
    public async void CreateLobbyMethod()
    {

        string lobbyname = lobbyName.text;
        int maxPlayers = Convert.ToInt32(maxPlayer.options[maxPlayer.value].text);
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = isPrivate.isOn;

        //Player Creato
        //Lobby açýldýðýnda lob,i sahibi için bir player options açtýk
        options.Player = new Player(AuthenticationService.Instance.PlayerId);
        //sadece string deðerler taþýr
        playerName = PlayerPrefs.GetString("PlayerName", "Player_" + Random.Range(1000, 9999));
        options.Player.Data = new Dictionary<string, PlayerDataObject>()
       {
           { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
           { "PlayerLevel",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,"5") },
           { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
       };

        //Lobby Data
        options.Data = new Dictionary<string, DataObject>()
       {
           {"GameMode",new DataObject(DataObject.VisibilityOptions.Public,gameMode.options[gameMode.value].text,DataObject.IndexOptions.S1) }
       };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyname, maxPlayers, options);


        DontDestroyOnLoad(this);
        GetComponent<CurrentLobby>().currentLobby = lobby;

        Debug.Log("Cerate lobby Done!");

        LobbyStatic.LogLooby(lobby);
        LobbyStatic.LogPlayersInLObby(lobby);
        // lobbyCode.text = lobby.LobbyCode;
        //  StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15f));
        if (heartbeatCoroutine != null)
            StopCoroutine(heartbeatCoroutine);

        heartbeatCoroutine = StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
        
        loadingUI.SetActive(true);
      
        JoinrelayFunctions(lobby);
        // await Task.Delay(200);



        /* string lobbyname = lobbyName.text;
         int maxPlayers = Convert.ToInt32(maxPlayer.options[maxPlayer.value].text);
         playerName = PlayerPrefs.GetString("PlayerName", "Player_" + Random.Range(1000, 9999));

         try
         {
             // Relay Host oluþtur
             Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
             string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

             // Unity Transport ayarlarý
             UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
             if (transport == null)
             {
                 Debug.LogError("UnityTransport bileþeni eksik!");
                 return;
             }

             transport.SetRelayServerData(
                 allocation.RelayServer.IpV4,
                 (ushort)allocation.RelayServer.Port,
                 allocation.AllocationIdBytes,
                 allocation.Key,
                 allocation.ConnectionData,
                 allocation.ConnectionData // HostConnectionData = ConnectionData çünkü bu host
             );

             // Lobi oluþturma seçenekleri
             CreateLobbyOptions options = new CreateLobbyOptions
             {
                 IsPrivate = isPrivate.isOn,
                 Player = new Player(AuthenticationService.Instance.PlayerId)
                 {
                     Data = new Dictionary<string, PlayerDataObject>
                 {
                     { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                     { "PlayerLevel", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "5") },
                     { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
                 }
                 },
                 Data = new Dictionary<string, DataObject>
             {
                 { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode.options[gameMode.value].text, DataObject.IndexOptions.S1) },
                 { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
             }
             };

             // Lobi oluþtur
             Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyname, maxPlayers, options);
             DontDestroyOnLoad(this);
             GetComponent<CurrentLobby>().currentLobby = lobby;

             Debug.Log("Lobi oluþturuldu!");
             Debug.Log("Relay Join Code: " + joinCode);

             LobbyStatic.LogLooby(lobby);
             LobbyStatic.LogPlayersInLObby(lobby);

             StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15f));
             LobbyStatic.LoadLobbyRoom();

             // Host baþlat
             NetworkManager.Singleton.StartHost();
         }
         catch (RelayServiceException re)
         {
             Debug.LogError("Relay hatasý: " + re.Message);
         }
         catch (LobbyServiceException le)
         {
             Debug.LogError("Lobby hatasý: " + le.Message);
         }*/

    }
    private async Task<(Allocation, string)> InitializeRelayAsync()
    {
        Allocation allocation = await Networkmanager.Instance.AllocateRealy();
        string relayJoinCode = await Networkmanager.Instance.GetRealyJoinCode(allocation);
        return (allocation, relayJoinCode);
    }

    private void SetupTransport(Allocation allocation)
    {
        var hostData = new RelayHostData
        {
            IPv4Adress = allocation.RelayServer.IpV4,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationIDBytes = allocation.AllocationIdBytes,
            Key = allocation.Key,
            ConnectionData = allocation.ConnectionData
        };

        transport.SetRelayServerData(
            hostData.IPv4Adress,
            hostData.Port,
            hostData.AllocationIDBytes,
            hostData.Key,
            hostData.ConnectionData
        );
    }
    public async void JoinrelayFunctions(Lobby lobby)
    {
            
        Allocation allocation = await Networkmanager.Instance.AllocateRealy();
        string relayJoinCode = await Networkmanager.Instance.GetRealyJoinCode(allocation);

        _HostData = new RelayHostData()
        {
            IPv4Adress = allocation.RelayServer.IpV4,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationId = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            Key = allocation.Key
        };
        await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                {k_keyJoinCode , new DataObject(DataObject.VisibilityOptions.Member,relayJoinCode)}
            }
        });
        transport.SetRelayServerData(_HostData.IPv4Adress, _HostData.Port, _HostData.AllocationIDBytes, _HostData.Key, _HostData.ConnectionData);

        NetworkManager.Singleton.StartHost();
        LobbyStatic.LoadLobbyRoom();
    }
    public void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
            StopCoroutine(heartbeatCoroutine);
        heartbeatCoroutine = null;
    }

    IEnumerator HeartbeatLobbyCoroutine(string lobbyID, float waitTimeSeconds)
    {
        var delay = new WaitForSeconds(waitTimeSeconds);
        while (true) {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyID);
            yield return delay;
        }
    }
    public void LogPlayersOnLobby(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            Debug.Log("PlayerId : " + player.Id);
        }
    }

    void OnEnable()
    {
       SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
       var objs = GameObject.FindGameObjectsWithTag("LobbyManager");
        if (objs.Length > 1)
        {
            for (int i = 0; i < objs.Length - 1; i++)
                Destroy(objs[i].gameObject);
        }
    }
}

/* string lobbyname= lobbyName.text;
       int maxPlayers = Convert.ToInt32(maxPlayer.options[maxPlayer.value].text );
       CreateLobbyOptions options = new CreateLobbyOptions();
       options.IsPrivate = isPrivate.isOn;

       //Player Creato
       //Lobby açýldýðýnda lob,i sahibi için bir player options açtýk
       options.Player = new Player(AuthenticationService.Instance.PlayerId);
       //sadece string deðerler taþýr
       playerName = PlayerPrefs.GetString("PlayerName", "Player_" + Random.Range(1000, 9999)); 
       options.Player.Data = new Dictionary<string, PlayerDataObject>()
       {
           { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
           { "PlayerLevel",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,"5") },
           { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
       };

       //Lobby Data
       options.Data = new Dictionary<string, DataObject>()
       {
           {"GameMode",new DataObject(DataObject.VisibilityOptions.Public,gameMode.options[gameMode.value].text,DataObject.IndexOptions.S1) }
       };
       Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyname,maxPlayers,options);
       DontDestroyOnLoad(this);
       GetComponent<CurrentLobby>().currentLobby = lobby;

       Debug.Log("Cerate lobby Done!"); 

       LobbyStatic.LogLooby(lobby);
       LobbyStatic.LogPlayersInLObby(lobby);
      // lobbyCode.text = lobby.LobbyCode;
       StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id,15f));
       LobbyStatic.LoadLobbyRoom();*/


/**options.Player = new Player(AuthenticationService.Instance.PlayerId)
{
    Data = new Dictionary<string, PlayerDataObject>
    {
        { "playerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "Celal") },
        { "characterSkin", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "warrior01") },
        { "level", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "5") },
        { "isReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "true") }
    }
};*/ //buradaki bilgiler ile ready iþlemleri falan yapýlabilir // normalde veri iþlemleri veri tabanýndan yapýlýr upoad edilir sahnelere