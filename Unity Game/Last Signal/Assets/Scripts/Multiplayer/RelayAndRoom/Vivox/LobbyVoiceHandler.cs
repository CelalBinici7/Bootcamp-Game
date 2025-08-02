using System;
using UnityEngine;

public class LobbyVoiceHandler : MonoBehaviour
{
    [SerializeField] private string voiceChannelName;
     
    private async void Start()
    {
        if (VivoxVoiceManagers.Instance != null)
        {
            if (CurrentLobby.instance != null && CurrentLobby.instance.currentLobby != null)
            {
                voiceChannelName = CurrentLobby.instance.currentLobby.Id;
                try
                {
                    if (!string.IsNullOrEmpty(voiceChannelName))
                        await VivoxVoiceManagers.Instance.JoinVoiceChannel(voiceChannelName);
                  //  await VivoxVoiceManagers.Instance.JoinVoiceChannel(voiceChannelName);
                    Debug.Log($"[Vivox] Voice kanalýna katýldý: {voiceChannelName}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Vivox] Voice kanalýna katýlýrken hata oluþtu: {ex.Message}");
                }
            }
         
        }
        else
        {
            Debug.LogWarning("[Vivox] VivoxManager.Instance null.");
        }
     
      
        
    }

    private async void OnDestroy()
    {
        if (VivoxVoiceManagers.Instance != null)
        {
            try
            {
                await VivoxVoiceManagers.Instance.LeaveVoiceChannel(voiceChannelName);
                Debug.Log($"[Vivox] Voice kanalýndan ayrýldý: {voiceChannelName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Vivox] Voice kanalýndan ayrýlýrken hata oluþtu: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("[Vivox] VivoxManager.Instance null.");
        }
    }
}

