using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using Zenject;

public class SignalRClient : ISignalRClient
{
    private HubConnection _connection;
    
    public Action<FriendRequestPacketResponse> OnFriendRequestNotificationReceived { get; set; }

    public async Task Connect()
    {
        if (_connection is { State: HubConnectionState.Connected }) return;
        
        var url = Managers.Network.BaseUrl + "/signalRHub";
        Debug.Log(url);
        
        _connection = new HubConnectionBuilder().WithUrl(url).Build();
        Register();

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception e)
        {
            Debug.Log($"Error: {e.Message}");
        }
    }

    public async Task JoinLobby(string username)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            try
            {
                await _connection.InvokeAsync("JoinLobby", username);
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
        _connection.On<FriendRequestPacketResponse>("FriendRequestNotification", OnFriendRequestNotification);
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
                Console.WriteLine($"Error: {e.Message}");
            }
        }
        
        Console.WriteLine("Connection is not established.");
        return new FriendRequestPacketResponse();
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
}
