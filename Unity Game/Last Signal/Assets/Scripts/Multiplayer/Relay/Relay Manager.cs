using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using System;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;

public class RelayManager : MonoBehaviour
{
    [SerializeField]private string PlayerId;
    [SerializeField] private TextMeshProUGUI IdText;
    [SerializeField] private TextMeshProUGUI JoinCodeText;
    [SerializeField] private TMP_Dropdown playerCountDropdown;
    [SerializeField] private TMP_InputField CodeJOinInput;
    private RelayHostData _HostData;
    private RelayJoinData _JoinData;
    async void Start()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("UnityServices Initilaze");
        Signing();
    }

    

    async void Signing()
    {
        Debug.Log("Sign in");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        PlayerId = AuthenticationService.Instance.PlayerId;
          Debug.Log("Signed in : " + PlayerId);
        IdText.text = PlayerId; 
    }


    ///Eðer bir apple steam ile baðlanma gerekiyosa auth ops diye ayarlar var araþtýr buradan yap yeni ekran falan uradan ama þimdilik yeterli
    public async void OnHostClick()
    {
        int MaxPlayerCount = Convert.ToInt32(playerCountDropdown.options[playerCountDropdown.value].text);
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayerCount);

        _HostData = new RelayHostData()
        {
            IPv4Adress = allocation.RelayServer.IpV4,
            Port =(ushort)allocation.RelayServer.Port,
            AllocationId = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            Key = allocation.Key
        };
        Debug.Log("Allocate Complete : " + _HostData.AllocationId);
        _HostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(_HostData.AllocationId);
        JoinCodeText.text = _HostData.JoinCode;

        UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
        transport.SetRelayServerData(_HostData.IPv4Adress,_HostData.Port, _HostData.AllocationIDBytes,_HostData.Key,_HostData.ConnectionData);
        NetworkManager.Singleton.StartHost();
    }

    public async void OnJoinClick()
    {
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(CodeJOinInput.text);

        _JoinData = new RelayJoinData()
        {
            IPv4Adress = allocation.RelayServer.IpV4,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationId = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData,
            Key = allocation.Key
        };

        Debug.Log("Join Succes : " + _JoinData.AllocationId);
        UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
        transport.SetRelayServerData(_JoinData.IPv4Adress, _JoinData.Port, _JoinData.AllocationIDBytes, _JoinData.Key, _JoinData.ConnectionData,_JoinData.HostConnectionData);
        NetworkManager.Singleton.StartClient();
    }
}

public struct RelayHostData
{
    public string JoinCode;
    public string IPv4Adress;
    public ushort Port;
    public Guid AllocationId;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] Key;
}

public struct RelayJoinData
{
    
    public string IPv4Adress;
    public ushort Port;
    public Guid AllocationId;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] HostConnectionData;
    public byte[] Key;
}