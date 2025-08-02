using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class GetLobbies : MonoBehaviour
{
    public GameObject buttonsContainer;
    public GameObject buttonPrefab;
     void Start()
    {
      //  await UnityServices.InitializeAsync();
      //  Debug.Log("UnityServices Initilaze");
       // await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //eklendi
        GetLoobiesTest();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void GetLoobiesTest()
    {
        ClearContainer();
        try
        {
            QueryLobbiesOptions options = new();
            Debug.LogWarning("QueryLobbiesTest");
            options.Count = 25;
            //burada ýlaþýlabilir boþ slotlarda 0 dan büyük slotlarýolan lobileri getir sorgusu attýk.
            options.Filters=new List<QueryFilter>()
            {
                new QueryFilter(
                    field :QueryFilter.FieldOptions.AvailableSlots,
                    op:QueryFilter.OpOptions.GT,
                    value:"0"),

              //  new QueryFilter(QueryFilter.FieldOptions.S1,"FireUp",QueryFilter.OpOptions.EQ  )
            };


            options.Order = new List<QueryOrder> { 
            
                new QueryOrder(
                    asc:false,
                    field:QueryOrder.FieldOptions.Created
                    ) 
            };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);
            Debug.LogWarning("Get Lobbies Done COUNT :" + lobbies.Results.Count);

            foreach (var item in lobbies.Results)
            {
                LobbyStatic.LogLooby(item);
                CreateLobbyButton(item);
            }
           // GetComponent<JoinLobby>().JoinLobbyWithID(lobbies.Results[0].Id);
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
    //0 name 1 playerstate 2join button 3 image

    private void CreateLobbyButton(Lobby lobby)
    {
        var button = Instantiate(buttonPrefab,Vector3.zero,Quaternion.identity);
        button.name = lobby.Name + "_button";
        button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = lobby.Name;
        button.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = lobby.MaxPlayers.ToString()+ " / "+lobby.Players.Count.ToString();
        var rectTransform = button.GetComponent<RectTransform>();   
        rectTransform.SetParent(buttonsContainer.transform);
        button.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate () { Lobby_Onclik(lobby); });
    }
    // sadece button yerine iim soy isim kiþþi sayýsý gibi gösterebilrisin.
    //kesin yapýlacak
    public void Lobby_Onclik(Lobby lobby)
    {
        Debug.Log("Clicedk lobby :" + lobby.Name);
        GetComponent<JoinLobby>().JoinLobbyWithID(lobby.Id);
    }

    private void ClearContainer()
    {
        if (buttonsContainer!= null && buttonsContainer.transform.childCount>0)
        {
            foreach (Transform item in buttonsContainer.transform)
            {
                Destroy(item.gameObject);
            }
        }
    }
}
