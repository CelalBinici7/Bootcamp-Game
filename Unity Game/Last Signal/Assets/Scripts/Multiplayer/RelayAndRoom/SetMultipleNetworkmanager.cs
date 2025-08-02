using UnityEngine;
using UnityEngine.SceneManagement;

public class SetMultipleNetworkmanager : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var objs = GameObject.FindGameObjectsWithTag("NetworkManager");
        if (objs.Length > 1)
        {
            for (int i = 0; i < objs.Length - 1; i++)
                Destroy(objs[i].gameObject);
        }
    }
}
