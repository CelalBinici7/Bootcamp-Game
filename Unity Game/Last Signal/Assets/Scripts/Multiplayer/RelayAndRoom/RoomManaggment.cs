using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomManaggment : MonoBehaviour
{
    public GameObject playerItemPrefab;
    public Transform playerListParent;

    private Lobby currentLobby;
    private CurrentLobby _currentLobby;
    public Dictionary<string, GameObject> playerItems = new();

    private LobbyEventCallbacks callbacks;
    private Task lobbyEventTask;

    private RelayHostData _HostData;
    private RelayJoinData _JoinData;
    //private bool isSubscribed = false;
   /// private bool isKickHandled = false;
    private HashSet<string> kickedPlayerIds = new HashSet<string>();
    public static RoomManaggment instance;
    
    private void Start()
    {
        if (instance ==null)
        {
            instance = this;
        }
        
            callbacks = new LobbyEventCallbacks();

            callbacks.LobbyChanged += OnLobbyChanged;
            callbacks.PlayerJoined += OnPlayerJoined;
            callbacks.PlayerLeft += OnPlayerLeft;
            callbacks.KickedFromLobby += OnKickedFromLobby;

            _currentLobby = GameObject.Find("Lobbymanager").GetComponent<CurrentLobby>();
        // Event dinleme başlatılıyor
        if (_currentLobby.currentLobby ==null)
        {
            print("asda");
        }
            lobbyEventTask = LobbyService.Instance.SubscribeToLobbyEventsAsync(_currentLobby.currentLobby.Id, callbacks);
        
     
           UpdatePlayerList();
        if (true)
        {
            currentLobby = _currentLobby.currentLobby;
            print(currentLobby.Name);
        }

    }

    private async void OnKickedFromLobby()
    {
        if (_currentLobby.currentLobby != null)
        {
            try
            {
             
                //_currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.currentLobby.Id);
                //UpdatePlayerList();
                string myPlayerId = AuthenticationService.Instance.PlayerId;
                bool isPlayerInLobby = _currentLobby.currentLobby.Players.Any(p => p.Id == myPlayerId);
                Debug.Log(isPlayerInLobby);
                if (isPlayerInLobby) 
                {
                    Debug.Log("Kicklendin! Ana menüye yönlendiriliyor...");
                  //  _currentLobby.currentLobby = null;
                    //OnDestroy();
                    RelayDisconnect.instance.DisconnectRelay();
                    SceneManager.LoadScene("MainScene");
                    return;
                }

                _currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.currentLobby.Id);
                UpdatePlayerList();
            }
            catch (LobbyServiceException e)
            {
                if (e.ErrorCode == 404)
                {
                    Debug.LogWarning("Lobby silinmiş veya bulunamadı. İşlem iptal edildi.");
                    // İstersen burada sahne geçişi yapabilirsin
                    // _currentLobby.currentLobby = null; // ÖNEMLİ: Lobby referansını sıfırla
                    RelayDisconnect.instance.DisconnectRelay();
                    SceneManager.LoadScene("MainScene");
                }
                else
                {
                    Debug.LogWarning($"Lobby güncellenirken hata: {e.Message}");
                }
            }
        }

    }

    private async void OnPlayerLeft(List<int> list)
    {
        try
        {
            // 1. Null kontrolü ve temel kontroller
            if (_currentLobby?.currentLobby == null) return;

            // 2. Lobby'yi güncelle
            _currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.currentLobby.Id);

            // 3. Lobby artık yoksa ana menüye dön
            if (_currentLobby.currentLobby == null)
            {
                RelayDisconnect.instance.DisconnectRelay();
                SceneManager.LoadScene("MainScene");
                return;
            }

            // 4. Kendi oyuncu ID'mizi al
            string myPlayerId = AuthenticationService.Instance.PlayerId;
            bool isMeWhoLeft = false;
            var players = _currentLobby.currentLobby.Players;
            // 5. Ayrılan oyuncuları kontrol et
            foreach (var playerIndex in list)
            {
                if (playerIndex >= 0 && playerIndex < _currentLobby.currentLobby.Players.Count)
                {
                    var leftPlayer = _currentLobby.currentLobby.Players[playerIndex];
                    if (kickedPlayerIds.Contains(leftPlayer.Id))
                    {
                        kickedPlayerIds.Remove(leftPlayer.Id);
                        return;
                    }
                    // 6. Ayrılan oyuncu host mu?
                    if (leftPlayer.Id == _currentLobby.currentLobby.HostId)
                    {
                        Debug.Log("Host ayrıldı! Ana menüye yönlendiriliyor...");
                        RelayDisconnect.instance.DisconnectRelay();
                        SceneManager.LoadScene("MainScene");
                        return;
                    }

                    // 7. Ayrılan oyuncu ben miyim?
                    if (leftPlayer.Id == myPlayerId)
                    {
                        isMeWhoLeft = true;
                    }
                }
            }

            // 8. Eğer ayrılan oyuncu ben değilsem güncelleme yap
            if (!isMeWhoLeft)
            {
                UpdatePlayerList();
            }
            else
            {
                Debug.Log("Oyuncu (ben) lobby'den ayrıldı");
                RelayDisconnect.instance.DisconnectRelay();
                SceneManager.LoadScene("MainScene");
            }
        }
        catch (LobbyServiceException e) when (e.ErrorCode == 404)
        {
            Debug.Log("Lobby bulunamadı (silinmiş olabilir). Ana menüye dönülüyor...");
            RelayDisconnect.instance.DisconnectRelay();
            SceneManager.LoadScene("MainScene");
        }
        catch (Exception e)
        {
            Debug.LogWarning("OnPlayerLeft - Lobby erişim hatası: " + e.Message);
        }
        /*try
        {
            if (_currentLobby.currentLobby == null) return;
           
            // Lobby'yi tekrar getir (hala var mı diye)
            _currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.currentLobby.Id);

            if (_currentLobby.currentLobby == null)
            {
                SceneManager.LoadScene("MainScene");
                return;
            }

          
                UpdatePlayerList();

            var players = _currentLobby.currentLobby.Players;

            foreach (var playerIndex in list)
            {
                if (playerIndex >= 0 && playerIndex < players.Count)
                {
                    var leftPlayer = players[playerIndex];

                    // Eğer ayrılan kişi host id'siyle eşleşiyorsa, lobby silinmiştir
                    if (leftPlayer.Id == _currentLobby.currentLobby.HostId)
                    {
                        Debug.Log("Host ayrıldı! Ana menüye yönlendiriliyor...");

                        // Artık lobby'yi silmeye çalışma, zaten silinmiş
                     //   _currentLobby.currentLobby = null; // ÖNEMLİ: Lobby referansını sıfırla
                        SceneManager.LoadScene("MainMenu");
                       

                        return;
                    }
                }
            }
        }
        catch (LobbyServiceException e)
        {
            // Eğer hata 404 ise, lobby silinmiş demektir — direkt menüye dön
            if (e.ErrorCode == 404)
            {
                Debug.Log("Lobby bulunamadı (silinmiş olabilir). Ana menüye dönülüyor...");
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                Debug.LogWarning("OnPlayerLeft - Lobby erişim hatası: " + e.Message);
            }
        }*/
        /*_currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.currentLobby.Id);
        UpdatePlayerList();
        var players = _currentLobby.currentLobby.Players;

        // Ayrılan oyuncuları kontrol et
        foreach (var playerIndex in list)
        {
            // Geçerli index kontrolü
            if (playerIndex >= 0 && playerIndex < players.Count)
            {
                var leftPlayer = players[playerIndex];

                // Eğer ayrılan oyuncu host ise
                if (leftPlayer.Id == _currentLobby.currentLobby.HostId)
                {
                    Debug.Log("Host ayrıldı! Ana menüye yönlendiriliyor...");

                    // Oyuncuyu lobiden çıkar
                    await LobbyService.Instance.RemovePlayerAsync(
                        _currentLobby.currentLobby.Id,
                        AuthenticationService.Instance.PlayerId
                    );

                    // Ana menüye yönlendir
                    SceneManager.LoadScene("MainMenu");
                    return;
                }
            }
        }*/
    }

    private async void OnPlayerJoined(List<LobbyPlayerJoined> list)
    {
        _currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.currentLobby.Id);
        UpdatePlayerList();
    }
    public async Task SafeLeaveLobby(Lobby currentLobby, string playerId)
    {
        try
        {
            if (currentLobby.HostId == playerId)
            {
                await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
            }
            else
            {
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
            }
        }
        catch (LobbyServiceException e)
        {
            if (e.ErrorCode != 404)
            {
                Debug.LogWarning("SafeLeaveLobby hatası: " + e.Message);
            }
        }
    }
    //lobbyden çıkma kodu 
    public async void LeaveLobbyAndReturnToMenu()
    {
        try
        {
            var lobby = _currentLobby?.currentLobby;

            if (lobby != null && !string.IsNullOrEmpty(lobby.Id))
            {
                string playerId = AuthenticationService.Instance.PlayerId;

                if (!string.IsNullOrEmpty(playerId))
                {
                    if (lobby.HostId == playerId)
                    {
                        // Host ise lobby silinir
                        await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);

                        Debug.Log("Lobi silindi (host olarak).");

                        var lobbyManager = GameObject.Find("Lobbymanager")?.GetComponent<CreateLobby>();
                        if (lobbyManager != null)
                        {
                            lobbyManager.StopHeartbeat();
                            //_currentLobby.currentLobby = null;
                        }                     // ÖNEMLİ: Lobby referansını sıfırla

                    }
                    else
                    {
                        // Client ise sadece çıkılır
                        await LobbyService.Instance.RemovePlayerAsync(lobby.Id, playerId);
                        Debug.Log("Lobby'den çıkıldı.");
                        //_currentLobby.currentLobby = null; // ÖNEMLİ: Lobby referansını sıfırla
                    }
                }
                else
                {
                    Debug.LogWarning("Geçersiz Player ID.");
                }
            }
            else
            {
                Debug.LogWarning("Geçerli bir lobi bulunamadı.");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobby'den çıkma hatası: {e.Message}");
        }
        finally
        {
            RelayDisconnect.instance.DisconnectRelay();
            // _currentLobby.currentLobby = null;
            SceneManager.LoadScene("MainScene");
        }
    }
    public  void Initialize(Lobby lobby)
    {
        currentLobby = lobby;
       

        // Callback fonksiyonlarını tanımlıyoruz
        callbacks = new LobbyEventCallbacks();
        
     
        callbacks.LobbyChanged += OnLobbyChanged;

        // Event dinleme başlatılıyor
        lobbyEventTask = LobbyService.Instance.SubscribeToLobbyEventsAsync(currentLobby.Id, callbacks);
    }

    private string GetLocalPlayerId()
    {
        return AuthenticationService.Instance.PlayerId; // Gerçek oyuncu ID'si ile değiştir
    }

    private  void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            Debug.Log("Host lobiyi sildi. Ana menüye dönülüyor.");
            SceneManager.LoadScene("MainScene");
        }
        Debug.Log("Lobby güncellendi!");
        if (_currentLobby.currentLobby != null)
            UpdatePlayerList(); // İsim değişmiş olabilir
                                // Oyun başladı mı kontrol et
        var lobbyManager = GameObject.Find("Lobbymanager")?.GetComponent<CreateLobby>();
        if (lobbyManager != null)
        {
           // lobbyManager.StopHeartbeat();
        }
        if (_currentLobby.currentLobby != null&& changes.PlayerData.Changed)
        {
            PlayerReadyManagment.instance.PollLobbyData();
        }

        /* if (_currentLobby.currentLobby.Data["GameStarted"].Value == "true")
        {
            // Relay join code'unu al
            var joinCode = _currentLobby.currentLobby.Data["RelayJoinCode"].Value;

            // Relay'e katıl
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
       
            // Network Manager ile oyuna katıl
            NetworkManager.Singleton.StartClient();
            SceneManager.LoadScene("GameScene");
        }*/
    }
   

    private void UpdatePlayerList2()
    {
        // Listeyi temizle
        foreach (Transform child in playerListParent)
            Destroy(child.gameObject);
        playerItems.Clear();

        foreach (var player in currentLobby.Players)
        {
            var item = Instantiate(playerItemPrefab, playerListParent);
            var name = player.Data.ContainsKey("playerName") ? player.Data["playerName"].Value : player.Id;
            item.transform.Find("NameText").GetComponent<Text>().text = name;

            playerItems[player.Id] = item;
        }
    }
    public void UpdatePlayerList()
    {
        if (_currentLobby == null || _currentLobby.currentLobby == null || playerListParent == null)
        {
            Debug.LogWarning("UpdatePlayerList: Gerekli referanslar null!");
            return;
        }
      
        foreach (Transform child in playerListParent)
            Destroy(child.gameObject);

        playerItems.Clear();

        bool isHost = _currentLobby.currentLobby.HostId == AuthenticationService.Instance.PlayerId;
        if (_currentLobby.currentLobby==null)
        {
            Debug.Log("currentlobby is null");
        }
        foreach (var player in _currentLobby.currentLobby.Players)
        {
            GameObject item = Instantiate(playerItemPrefab, playerListParent);
            string name = (player.Data != null && player.Data.ContainsKey("PlayerName"))
            ? player.Data["PlayerName"].Value
            : player.Id;
            item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;

            // KICK BUTTON sadece host için görünür ve kendisi hariç
            var kickBtn = item.transform.transform.GetChild(2).GetComponent<Button>();
            if (isHost && player.Id != AuthenticationService.Instance.PlayerId)
            {
                kickBtn.gameObject.SetActive(true);
                string targetPlayerId = player.Id;

                kickBtn.onClick.AddListener(() => KickPlayer(targetPlayerId));
            }
            else
            {
                kickBtn.gameObject.SetActive(false);
            }

            playerItems[player.Id] = item;
        }
    }
    /*    private async void OnLobbyChanged(ILobbyChanges changes)
    {
        Debug.Log("🎯 Lobi güncellendi.");

        // Değişen lobby bilgisini tekrar çek
        try
        {
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            UpdatePlayerList();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Lobi güncelleme hatası: " + ex.Message);
        }
    }*/

    private async void KickPlayer(string playerId)
    {
        if (_currentLobby?.currentLobby == null || string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning("KickPlayer: Geçersiz lobby veya playerId");
            return;
        }
        try
        {
            kickedPlayerIds.Add(playerId);
            await LobbyService.Instance.RemovePlayerAsync(_currentLobby.currentLobby.Id, playerId);
            Debug.Log($"Oyuncu atıldı: {playerId}");
            // Lobby'yi güncelle
            _currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.currentLobby.Id);
            UpdatePlayerList();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Oyuncu atılamadı: " + ex.Message);
        }
    }
    private bool IsHost()
    {
        return AuthenticationService.Instance.PlayerId == currentLobby.HostId;
    }

    // Lobi hazır mı kontrolü
    public bool IsLobbyReadyToStart(Lobby lobby)
        {
            // Minimum oyuncu sayısı kontrolü (örneğin 2 oyuncu)
            if (lobby.Players.Count < 2) return false;

            // Tüm oyuncular "Ready" durumunda mı kontrolü
            foreach (var player in lobby.Players)
            {
                if (player.Data["Ready"].Value != "true")
                    return false;
            }

            return true;
        }
    public async void StartGame()
    {
        try
        {
            // Lobby bilgilerini güncelle
            var lobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.currentLobby.Id);

            // Lobi hazır mı kontrol et
            if (!IsLobbyReadyToStart(lobby))
            {
                Debug.Log("Oyun başlatma için hazır değil!");
                return;
            }

            // Relay Allocation oluştur
            var allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(lobby.Players.Count);
            var joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Lobby'ye join code'u ekle
            var updateOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
                { "GameStarted", new DataObject(DataObject.VisibilityOptions.Member, "true") }
            }
            };

            await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateOptions);

            // Network Manager üzerinden oyunu başlat
            //  NetworkManager.Singleton.StartHost(allocation);
            _HostData = new RelayHostData()
            {
                IPv4Adress = allocation.RelayServer.IpV4,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationId = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                Key = allocation.Key
            };
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(_HostData.IPv4Adress, _HostData.Port, _HostData.AllocationIDBytes, _HostData.Key, _HostData.ConnectionData);

            NetworkManager.Singleton.StartHost();
            SceneManager.LoadScene("GameScene");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Oyun başlatma hatası: {e.Message}");
        }
    }

    public async void SetPlayerReady()
    {
        try
        {
            var playerUpdateOptions = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
            }
            };

            await LobbyService.Instance.UpdatePlayerAsync(
                _currentLobby.currentLobby.Id,
                AuthenticationService.Instance.PlayerId,
                playerUpdateOptions
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Hazır durumu güncelleme hatası: {e.Message}");
        }
    }
    private void OnDestroy()
    {
        // Eventleri temizle
        if (callbacks != null)
        {
            callbacks.LobbyChanged -= OnLobbyChanged;
            callbacks.PlayerJoined -= OnPlayerJoined;
            callbacks.PlayerLeft -= OnPlayerLeft;
            callbacks.KickedFromLobby -= OnKickedFromLobby;
        }

        // Async task'ı iptal et
        if (lobbyEventTask != null && !lobbyEventTask.IsCompleted)
        {
            // Task iptal mekanizması eklenmeli
        }
    }


   


}


/*Unity Lobby Sistemi ile Oyun Başlatma Rehberi
Unity'nin kendi Lobby ve Relay servislerini kullanarak tüm oyuncular aynı lobideyken oyunu başlatmak için aşağıdaki adımları izleyebilirsiniz:

1. Lobby Hazırlık Kontrolü
csharp
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class LobbyManager : MonoBehaviour
{
    // Lobi hazır mı kontrolü
    public bool IsLobbyReadyToStart(Lobby lobby)
    {
        // Minimum oyuncu sayısı kontrolü (örneğin 2 oyuncu)
        if (lobby.Players.Count < 2) return false;
        
        // Tüm oyuncular "Ready" durumunda mı kontrolü
        foreach (var player in lobby.Players)
        {
            if (player.Data["Ready"].Value != "true")
                return false;
        }
        
        return true;
    }
}

 ----

 2. Oyun Başlatma Mekanizması (Host Tarafı)
csharp
public async void StartGame()
{
    try
    {
        // Lobby bilgilerini güncelle
        var lobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.currentLobby.Id);
        
        // Lobi hazır mı kontrol et
        if (!IsLobbyReadyToStart(lobby))
        {
            Debug.Log("Oyun başlatma için hazır değil!");
            return;
        }

        // Relay Allocation oluştur
        var allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(lobby.Players.Count);
        var joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // Lobby'ye join code'u ekle
        var updateOptions = new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
                { "GameStarted", new DataObject(DataObject.VisibilityOptions.Member, "true") }
            }
        };

        await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateOptions);

        // Network Manager üzerinden oyunu başlat
        NetworkManager.Singleton.StartHost(allocation);
        SceneManager.LoadScene("GameScene");
    }
    catch (LobbyServiceException e)
    {
        Debug.LogError($"Oyun başlatma hatası: {e.Message}");
    }
}

 
 
 3. Client Tarafında Oyun Başlatma Dinleme
csharp
private async void OnLobbyChanged(Lobby lobby)
{
    // Oyun başladı mı kontrol et
    if (lobby.Data["GameStarted"].Value == "true")
    {
        // Relay join code'unu al
        var joinCode = lobby.Data["RelayJoinCode"].Value;
        
        // Relay'e katıl
        var joinAllocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);
        
        // Network Manager ile oyuna katıl
        NetworkManager.Singleton.StartClient(joinAllocation);
        SceneManager.LoadScene("GameScene");
    }
}

 -----
4. Oyuncu Hazırlık Sistemi
csharp
public async void SetPlayerReady()
{
    try
    {
        var playerUpdateOptions = new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "true") }
            }
        };
        
        await LobbyService.Instance.UpdatePlayerAsync(
            _currentLobby.currentLobby.Id,
            AuthenticationService.Instance.PlayerId,
            playerUpdateOptions
        );
    }
    catch (LobbyServiceException e)
    {
        Debug.LogError($"Hazır durumu güncelleme hatası: {e.Message}");
    }
}

 
 
 . UI Entegrasyonu (Örnek)
csharp
public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Text lobbyStatusText;

    private void Update()
    {
        // Sadece host görsün
        startGameButton.gameObject.SetActive(IsHost());
        
        // Lobi durumunu güncelle
        lobbyStatusText.text = $"Oyuncular: {currentPlayerCount}/{maxPlayers} | Hazır: {readyPlayerCount}";
    }

    public void OnReadyClicked()
    {
        LobbyManager.Instance.SetPlayerReady();
    }

    public void OnStartGameClicked()
    {
        LobbyManager.Instance.StartGame();
    }
}*/

public struct RelayHostDatas
{
    public string IPv4Address;
    public ushort Port;
    public string AllocationId;
    public byte[] AllocationIdBytes;
    public byte[] ConnectionData;
    public byte[] Key;

    public RelayHostDatas(Allocation allocation)
    {
        IPv4Address = allocation.RelayServer.IpV4;
        Port = (ushort)allocation.RelayServer.Port;
        AllocationId = allocation.AllocationId.ToString();
        AllocationIdBytes = allocation.AllocationIdBytes;
        ConnectionData = allocation.ConnectionData;
        Key = allocation.Key;
    }
}

public struct RelayClientDatas
{
    public string IPv4Address;
    public ushort Port;
    public string AllocationId;
    public byte[] AllocationIdBytes;
    public byte[] ConnectionData;
    public byte[] HostConnectionData;
    public byte[] Key;

    public RelayClientDatas(JoinAllocation allocation)
    {
        IPv4Address = allocation.RelayServer.IpV4;
        Port = (ushort)allocation.RelayServer.Port;
        AllocationId = allocation.AllocationId.ToString();
        AllocationIdBytes = allocation.AllocationIdBytes;
        ConnectionData = allocation.ConnectionData;
        HostConnectionData = allocation.HostConnectionData;
        Key = allocation.Key;
    }
}