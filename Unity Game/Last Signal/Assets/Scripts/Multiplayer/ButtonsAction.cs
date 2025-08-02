using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ButtonsAction : MonoBehaviour
{
   [SerializeField] private NetworkManager networkManager;
    [SerializeField] private TextMeshProUGUI text;
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startHost()
    {
      networkManager.StartHost();
      //  InitilazeText();
    }

    public void startClient()
    {
        networkManager.StartClient();
       // InitilazeText();
    }
    public void SubmitNewPosition()
    {
        var PlayerObject =NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var player =PlayerObject.GetComponent<PlayerMovement>();
        player.move();
    }

    public void InitilazeText()
    {
        if (networkManager.IsServer|| networkManager.IsHost)
        {
            text.text = "move";
        }
        else
        {
            text.text = "RequestMove";
        }
    }
}
