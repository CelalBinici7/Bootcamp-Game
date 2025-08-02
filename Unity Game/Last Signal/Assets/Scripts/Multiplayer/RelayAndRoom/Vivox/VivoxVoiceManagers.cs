using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using System.Threading.Tasks;

public class VivoxVoiceManagers : MonoBehaviour
{
    public static VivoxVoiceManagers Instance;

    private  void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

      
    }
    public async Task JoinVoiceChannel(string channelName)
    {
        await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.TextAndAudio);
    }

    public async Task LeaveVoiceChannel(string channelName)
    {
        await VivoxService.Instance.LeaveChannelAsync(channelName);
    }


}
