using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReadyManagment : NetworkBehaviour
{
    public Button readyButton;
    public Button startButton;
    public bool isReady = false;
    private bool isWaiting = false;

    private float updateCooldown = 2f; // Spam korumasý
    bool allReady;
    public GameObject networkmanager;
    public Sprite readySprite;
    public Sprite unreadySprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  public static   PlayerReadyManagment instance;

    public GameObject closePanels1;
    public GameObject closePanels12;
    public Camera maincam;
    string state;
    [SerializeField] private Transform[] spawnPoints;
    private int nextSpawnIndex = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        readyButton.onClick.AddListener(OnReadyClicked);
        if (IsHost())
        {
            readyButton.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
            startButton.onClick.AddListener(() => {
                StartGame();
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnReadyClicked()
    {
        if (isWaiting) return;
      
        isReady = !isReady;
        readyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = isReady ? "Ready" : "Unready";
        OnClick_ReadyButton();
    }

     async void OnClick_ReadyButton()
    {
        isWaiting = true;

        string readyValue = isReady ? "true" : "false";
        var playerId = AuthenticationService.Instance.PlayerId;
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(
                CurrentLobby.instance.currentLobby.Id,
                AuthenticationService.Instance.PlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                    {
                        "IsReady",
                        new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Member,
                            value: readyValue
                        )
                    }
                    }
                });
            //UpdatePlayerReadyUI(playerId,isReady);

            //UIManager.instance.UpdateReadyIcon(isReady); // Buton simgesini güncelle
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Ready durumu güncellenirken hata: {e.Message}");
        }
    }
    public void UpdatePlayerReadyUI(string playerId, bool isReady)
    {
        if (!RoomManaggment.instance.playerItems.ContainsKey(playerId))
            return;

        GameObject item = RoomManaggment.instance.playerItems[playerId];
        Image readyImage = item.transform.GetChild(1).GetComponent<Image>();

        readyImage.sprite = isReady ? readySprite : unreadySprite;
    }

    private void CheckAllPlayersReady(Lobby lobby)
    {
         allReady = true;

        foreach (Player player in lobby.Players)
        {
            if (player.Data != null && player.Data.TryGetValue("IsReady", out var playerData))
            {
                if (playerData.Value != "true")
                {
                    allReady = false;
                    break;
                }
            }
            else
            {
                allReady = false;
                break;
            }
        }

        // Baþlatma butonunu duruma göre ayarla
        startButton.gameObject.SetActive(allReady);
        startButton.interactable = allReady;
    }
    public async void PollLobbyData()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.instance.currentLobby.Id);
            CurrentLobby.instance.currentLobby = lobby; // Lobby güncellendi

            // Tüm oyuncularýn ready durumunu güncelle
            foreach (Player player in lobby.Players)
            {
                if (player.Data != null && player.Data.TryGetValue("IsReady", out var playerData))
                {
                    bool readyStatus = playerData.Value == "true";
                    UpdatePlayerReadyUI(player.Id, readyStatus);
                }
            }

            // Host ise oyunu baþlatma butonunu kontrol et
            if (IsHost())
            {
                CheckAllPlayersReady(lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Lobby verileri çekilirken hata: " + e.Message);
        }
    }
    private bool IsHost()
    {
        if (CurrentLobby.instance.currentLobby == null) return false;
        return AuthenticationService.Instance.PlayerId == CurrentLobby.instance.currentLobby.HostId;
    }


    public void startGame()
    {
    }

    public void putAllPlayer()
    {

    }
    [SerializeField] private GameObject lobbyPanel;

    public void StartGame()
    {
        if (IsServer)
        {
            CloseLobbyUIClientRpc();
            // Oyun sahnesi geçiþi vs. burada yapýlabilir
        }

        closePanels1.SetActive(false);
        closePanels12.SetActive(false);
      //  maincam.gameObject.SetActive(false);
    }

    [ClientRpc]
    private void CloseLobbyUIClientRpc()
    {
        if (!IsHost())
        {
            closePanels1.SetActive(false);
            closePanels12.SetActive(false);
          //  maincam.gameObject.SetActive(false);
        }
            
    }
  
     
}
