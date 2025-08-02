using System.Collections.Generic;
using System.Linq; // Gerekli!
using TMPro;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;
public class VoiceDeviceSelector : MonoBehaviour
{

    [Header("Dropdown Referanslarý")]
    [SerializeField] private TMP_Dropdown inputDropdown;
    [SerializeField] private TMP_Dropdown outputDropdown;

    private List<VivoxInputDevice> inputDevices = new List<VivoxInputDevice>();
    private List<VivoxOutputDevice> outputDevices = new List<VivoxOutputDevice>();

    public Slider InputVolumeSlider;
    public Slider OutputVolumeSlider;

    const float MinVolume = 0;
    const float MaxVolume = 100;
    private  void Start()
    {
     

        // Load initial devices
        RefreshInputDevices();
        RefreshOutputDevices();

        SetUpDropdownAndSliders();
        // Device list updates (dynamic hotplugging)
        VivoxService.Instance.AvailableInputDevicesChanged += RefreshInputDevices;
        VivoxService.Instance.AvailableOutputDevicesChanged += RefreshOutputDevices;
        // Varsayýlan cihazý seçili hale getir
        // Listen to changes
       
        var currentInput = VivoxService.Instance.ActiveInputDevice;
        var currentOutput = VivoxService.Instance.ActiveOutputDevice;


        int inputIndex = inputDevices.FindIndex(d => d.DeviceName == currentInput.DeviceName);
        int outputIndex = outputDevices.FindIndex(d => d.DeviceName == currentOutput.DeviceName);

        if (inputIndex >= 0) inputDropdown.value = inputIndex;
        if (outputIndex >= 0) outputDropdown.value = outputIndex;
    }

    /* private async void OnInputDeviceSelected(int index)
     {
         if (index >= 0 && index < inputDevices.Count)
         {
             await VivoxService.Instance.SetActiveInputDeviceAsync(inputDevices[index]);
             Debug.Log($"[Vivox] Mikrofon deðiþtirildi: {inputDevices[index].DeviceName}");
         }
     }*/

    /* private async void OnOutputDeviceSelected(int index)
     {
         if (index >= 0 && index < outputDevices.Count)
         {
             await VivoxService.Instance.SetActiveOutputDeviceAsync(outputDevices[index]);
             Debug.Log($"[Vivox] Hoparlör deðiþtirildi: {outputDevices[index].DeviceName}");
         }
     }*/
    void OnInputDeviceSelected(int index)
    {
        var deviceName = inputDropdown.options[index].text;
        var device = VivoxService.Instance.AvailableInputDevices.First(d => d.DeviceName == deviceName);
        VivoxService.Instance.SetActiveInputDeviceAsync(device);
        Debug.Log($"[Vivox] Mikrofon deðiþtirildi: {inputDevices[index].DeviceName}");
    }

    void OnOutputDeviceSelected(int index)
    {
        var deviceName = outputDropdown.options[index].text;
        var device = VivoxService.Instance.AvailableOutputDevices.First(d => d.DeviceName == deviceName);
        VivoxService.Instance.SetActiveOutputDeviceAsync(device);
        Debug.Log($"[Vivox] Hoparlör deðiþtirildi: {outputDevices[index].DeviceName}");
    }
    void RefreshInputDevices()
    {
        inputDropdown.ClearOptions();
        var devices = VivoxService.Instance.AvailableInputDevices.ToList();
        inputDropdown.AddOptions(devices.Select(d => d.DeviceName).ToList());

        var active = VivoxService.Instance.ActiveInputDevice;
        inputDropdown.SetValueWithoutNotify(devices.FindIndex(d => d.DeviceName == active.DeviceName));
    }

    void RefreshOutputDevices()
    {
        outputDropdown.ClearOptions();
        var devices = VivoxService.Instance.AvailableOutputDevices.ToList();
        outputDropdown.AddOptions(devices.Select(d => d.DeviceName).ToList());

        var active = VivoxService.Instance.ActiveOutputDevice;
        outputDropdown.SetValueWithoutNotify(devices.FindIndex(d => d.DeviceName == active.DeviceName));
    }

    public void SetUpDropdownAndSliders()
    {
        // Volume limits
        InputVolumeSlider.minValue = MinVolume;
        InputVolumeSlider.maxValue = MaxVolume;

        OutputVolumeSlider.minValue = MinVolume;
        OutputVolumeSlider.maxValue = MaxVolume;

        InputVolumeSlider.value = VivoxService.Instance.InputDeviceVolume;
        OutputVolumeSlider.value = VivoxService.Instance.OutputDeviceVolume;
        // Input Devices
        inputDevices = VivoxService.Instance.AvailableInputDevices.ToList();
        inputDropdown.ClearOptions();
        inputDropdown.AddOptions(inputDevices.Select(device => device.DeviceName).ToList());
        inputDropdown.onValueChanged.AddListener(OnInputDeviceSelected);

        // Output Devices
        outputDevices = VivoxService.Instance.AvailableOutputDevices.ToList();
        outputDropdown.ClearOptions();
        outputDropdown.AddOptions(outputDevices.Select(device => device.DeviceName).ToList());
        outputDropdown.onValueChanged.AddListener(OnOutputDeviceSelected);

        inputDropdown.onValueChanged.AddListener(OnInputDeviceSelected);
        outputDropdown.onValueChanged.AddListener(OnOutputDeviceSelected);

       // InputVolumeSlider.onValueChanged.AddListener((val) => VivoxService.Instance.SetInputDeviceVolume((int)val));
       // OutputVolumeSlider.onValueChanged.AddListener((val) => VivoxService.Instance.SetOutputDeviceVolume((int)val));

    }
   
}
