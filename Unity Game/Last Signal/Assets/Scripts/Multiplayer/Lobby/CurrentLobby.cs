using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CurrentLobby : MonoBehaviour
{
    public static CurrentLobby instance; 
    public Lobby currentLobby { get; set; }


    private void Start()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }
    // Update is called once per frame
    void Update()
    {
      
    }

    public void Close()
    {
        SceneManager.LoadScene("MainScene");
    }
}
