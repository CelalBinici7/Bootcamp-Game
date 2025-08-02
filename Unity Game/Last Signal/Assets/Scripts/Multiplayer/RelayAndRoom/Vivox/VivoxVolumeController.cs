using Unity.Services.Vivox;
using UnityEngine;

using UnityEngine.UI;

public class VivoxVolumeController : MonoBehaviour
{
    [SerializeField] private Slider vivoxOutputSlider;

    private void Start()
    {
        vivoxOutputSlider.minValue = 0;
        vivoxOutputSlider.maxValue = 100;
        vivoxOutputSlider.value = 100;

        vivoxOutputSlider.onValueChanged.AddListener(SetVivoxOutputVolume);
    }

    private void SetVivoxOutputVolume(float value)
    {
        int volume = Mathf.RoundToInt(value);
        VivoxService.Instance.SetOutputDeviceVolume(volume);
        Debug.Log($"[Vivox] Dinleme sesi (output) {volume} olarak ayarlandý.");
    }
}
