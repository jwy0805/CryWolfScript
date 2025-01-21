using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

public class SignalRClient : ISignalRClient, ITickable, IDisposable
{
    private HubConnection _connection;
    private float _elapsedTime;
    private string _username;
    
    private const float HeartbeatInterval = 60f;
    
    public Action OnInvitationSent { get; set; }
    public Action<AcceptInvitationPacketResponse> OnInvitationSuccess { get; set; }
    public Action<FriendRequestPacketResponse> OnFriendRequestNotificationReceived { get; set; }

    public async Task Connect(string username)
    {
        if (_connection is { State: HubConnectionState.Connected }) return;
        
        var url = Managers.Network.BaseUrl + "/signalRHub";
        Debug.Log(url);
        
        _connection = new HubConnectionBuilder().WithUrl(url).Build();
        Register();

        try
        {
            await _connection.StartAsync();
            _username = username; 
        }
        catch (Exception e)
        {
            Debug.Log($"Error: {e.Message}");
        }
    }

    public void Tick()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= HeartbeatInterval)
        {
            _elapsedTime = 0;
            HeartBeat();
        }
    }

    private async void HeartBeat()
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            try
            {
                await _connection.InvokeAsync("HeartBeat", _username);
            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e.Message}");
            }
        }
    }
    
    public async Task JoinLobby()
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            try
            {
                await _connection.InvokeAsync("JoinLobby", _username);
            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e.Message}");
            }
        }
    }
    
    public async Task LeaveLobby()
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            try
            {
                await _connection.InvokeAsync("LeaveLobby");
            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e.Message}");
            }
        }
    }
    
    private void Register()
    {
        _connection.On("RefreshMailAlert", OnRefreshMailAlert);
        _connection.On("ToastNotification", OnToastNotification);
        _connection.On<AcceptInvitationPacketResponse>("GameRoomJoined", OnGameRoomJoined);
        _connection.On<AcceptInvitationPacketResponse>("RejectInvitation", OnRejectInvitation);
        _connection.On<FriendRequestPacketResponse>("FriendRequestNotification", OnFriendRequestNotification);
    }

    public async Task<InviteFriendlyMatchPacketRequired> SendInvitation(InviteFriendlyMatchPacketRequired required)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            try
            {
                return await _connection
                    .InvokeAsync<InviteFriendlyMatchPacketRequired>("HandleInviteFriendlyMatch", required);
            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e.Message}");
            }
        }
        
        Debug.Log("Connection is not established.");
        return new InviteFriendlyMatchPacketRequired();
    }
    
    public async Task<AcceptInvitationPacketResponse> SendAcceptInvitation(AcceptInvitationPacketRequired required)
    {
        try
        {
            await _connection
                .InvokeAsync<AcceptInvitationPacketResponse>("HandleAcceptInvitation", required);
        }
        catch (Exception e)
        {
            Debug.Log($"Error: {e.Message}");
        }
        
        Debug.Log("Connection is not established.");
        return new AcceptInvitationPacketResponse();
    }
    
    public async Task<FriendRequestPacketResponse> SendFriendRequest(FriendRequestPacketRequired required)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            try
            {
                return await _connection
                    .InvokeAsync<FriendRequestPacketResponse>("HandleFriendRequest", required);
            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e.Message}");
            }
        }
        
        Debug.Log("Connection is not established.");
        return new FriendRequestPacketResponse();
    }

    private void OnRefreshMailAlert()
    {
        OnInvitationSent?.Invoke();
    }
    
    private void OnToastNotification()
    {
        var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
        popup.SetWarning("Invitation Sent!");
    }
    
    private void OnGameRoomJoined(AcceptInvitationPacketResponse response)
    {
        OnInvitationSuccess?.Invoke(response);
    }
    
    private void OnRejectInvitation(AcceptInvitationPacketResponse response)
    {
        var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
        popup.SetWarning("Invitation Rejected!");
    }
    
    private void OnFriendRequestNotification(FriendRequestPacketResponse response)
    {
        OnFriendRequestNotificationReceived?.Invoke(response);
    }
    
    public async Task Disconnect()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }
    }

    public void Dispose()
    {
        
    }
}
