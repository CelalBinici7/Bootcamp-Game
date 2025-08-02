using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;
public class VoiceToggleHandler : MonoBehaviour
{

    [SerializeField] private Button toggleIncomingButton;
    [SerializeField] private Button toggleMicrophoneButton;

    private bool isHearingOthers = true;
    private bool isMicMuted = false;

    private void Start()
    {
        toggleIncomingButton.onClick.AddListener(ToggleIncomingAudio);
        toggleMicrophoneButton.onClick.AddListener(ToggleMicrophone);
    }

    private void ToggleIncomingAudio()
    {
        isHearingOthers = !isHearingOthers;

        foreach (var kv in VivoxService.Instance.ActiveChannels)
        {
            var participants = kv.Value; // ReadOnlyCollection<VivoxParticipant>
            foreach (var participant in participants)
            {
                if (!participant.IsSelf)
                {
                    if (isHearingOthers)
                        participant.UnmutePlayerLocally();
                    else
                        participant.MutePlayerLocally();
                }
            }
        }

        Debug.Log($"[Vivox] Gelen ses {(isHearingOthers ? "açýldý" : "kapandý")}");
    }

    private void ToggleMicrophone()
    {
        isMicMuted = !isMicMuted;

        if (isMicMuted)
            VivoxService.Instance.MuteInputDevice();
        else
            VivoxService.Instance.UnmuteInputDevice();

        Debug.Log($"[Vivox] Mikrofon {(isMicMuted ? "kapalý" : "açýk")}");
    }
}
