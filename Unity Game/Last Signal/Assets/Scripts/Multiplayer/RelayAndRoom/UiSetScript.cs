using UnityEngine;
using UnityEngine.SceneManagement;

public class UiSetScript : MonoBehaviour
{
    public void LoadNextScene()
    {
        SceneManager.LoadScene("LobbyBrowserScene");
    }

    public void ApplicationQuitMehod()
    {
        Application.Quit();
    }
}
