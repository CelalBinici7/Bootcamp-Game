using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
public class PopulateUI : MonoBehaviour
{
    private CurrentLobby _currentLobby;

    public GameObject playerInfoContainer;
    public GameObject playerInfoPrefab;

    public TMP_InputField newName;
    public TMP_InputField newPlayerName;


    [Header("LobbyInfoPanel")]
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI lobbyCode;
    [SerializeField] private TextMeshProUGUI gameMode;

    private string lobbyId;
    void Start()
    {
        _currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        PopulateUiElements();
        lobbyId = _currentLobby.currentLobby.Id;
       // InvokeRepeating(nameof(PollForLobbyUpdate), 1.1f, 4f);//burada güncelleme kýsmýný eventler ile yapýlmalý sende öyle yap bu yöntem çok maliyetli
        LobbyStatic.LogLooby(_currentLobby.currentLobby);
        
    }
    void PopulateUiElements()
    {
        lobbyName.text = _currentLobby.currentLobby.Name;
        lobbyCode.text = _currentLobby.currentLobby.LobbyCode;
        gameMode.text = _currentLobby.currentLobby.Data["GameMode"].Value;

        ClearContainer();
        foreach (Player item in _currentLobby.currentLobby.Players)
        {
            CreatePlayerInfoCard(item);
        }
    }

    public void CreatePlayerInfoCard(Player player)
    {
        var text = Instantiate(playerInfoPrefab , Vector3.zero ,Quaternion.identity);
        text.name = player.Joined.ToShortTimeString();
        text.GetComponent<TextMeshProUGUI>().text = player.Id + " : " + player.Data["PlayerLevel"].Value;
        var rectTransform = text.GetComponent<RectTransform>();
        rectTransform.SetParent(playerInfoContainer.transform);

    }
    async void PollForLobbyUpdate()
    {
        _currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
        PopulateUiElements();
    }
    private void ClearContainer()
    {
        if (playerInfoContainer != null && playerInfoContainer.transform.childCount > 0)
        {
            foreach (Transform item in playerInfoContainer.transform)
            {
                Destroy(item.gameObject);
            }
        }
    }


    ///BUtton Events 
    
    public async  void ChangeLobbyName()
    {
        var newLobbyname = newName.text;

        try
        {
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.Name = newLobbyname;
           _currentLobby.currentLobby= await LobbyService.Instance.UpdateLobbyAsync(lobbyId,options);
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }

    public async void ChangePlayerName()
    {
        var newPlayernames = newPlayerName.text;

        try
        {
            UpdatePlayerOptions options = new UpdatePlayerOptions();
            options.Data = new Dictionary<string, PlayerDataObject>()
        {
            { "PlayerLevel",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,newPlayernames) }
        };
           await LobbyService.Instance.UpdatePlayerAsync(lobbyId, AuthenticationService.Instance.PlayerId,options);
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
}
