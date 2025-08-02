
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;


public static  class LobbyStatic
{
 
   
    public static void LogPlayersInLObby(Lobby lobby){

        foreach (Player player in lobby.Players)
        {
            Debug.Log("Player ID : " + player.Id);
         //   Debug.Log("Player ID : " + player.Data["PlayerLevel"]);
        }
    }

    public static void LogLooby(Lobby lobby)
    {

        Debug.Log("Lobby Id : " + lobby.Id + "\n" + "GAmeMode = " + lobby.Data["GameMode"].Value);

    }
    public static void LoadLobbyRoom()
    {
        
        NetworkManager.Singleton.SceneManager.LoadScene("inLobbyRoom", LoadSceneMode.Single);
       // SceneManager.LoadScene("inLobbyRoom");
       // SceneManager.LoadScene("LobbyRoom");
    }
}
