using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

public class SignalRClient : ISignalRClient, ITickable, IDisposable
{
    private HubConnection _connection;
    private float _elapsedTime;
    private string _userTag;
    
    private const float HeartbeatInterval = 60f;

    public event Action OnInvitationSent;
    public event Func<DeckInfo, Task> OnEnemyDeckSwitched;
    public event Func<DeckInfo, DeckInfo, bool, Task> OnFactionSwitched;
    public event Action<AcceptInvitationPacketResponse> OnInvitationSuccess;
    public event Action<AcceptInvitationPacketResponse> OnEnterFriendlyMatch;
    public event Action<FriendRequestPacketResponse> OnFriendRequestNotificationReceived;
    public event Action OnGuestLeft;
    public event Func<Task> OnStartFriendlyMatch;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void PreserveProtocol()
    {
        // This is to ensure that the JsonHubProtocol is preserved in the build.
        _ = typeof(JsonHubProtocol);
    }
    
    public async Task Connect(string userTag)
    {
        var url = Managers.Network.BaseUrl + "/signalRHub";

        _connection = new HubConnectionBuilder().WithUrl(url).AddNewtonsoftJsonProtocol().Build();
        Register();
        await _connection.StartAsync();
        
        _userTag = userTag;
        Debug.Log($"{_userTag} {_connection.State} at {url}");
    }

    public void Tick()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= HeartbeatInterval)
        {
            _elapsedTime = 0;
            _ = HeartBeat();
        }
    }

    private async Task HeartBeat()
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            await _connection.InvokeAsync("HeartBeat", _userTag);
        }
    }
    
    public async Task JoinLobby(string token)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            await _connection.InvokeAsync("JoinLobby", token);
        }
    }

    public async Task JoinGame(string token)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            await _connection.InvokeAsync("JoinGame", token, Util.Faction);
        }
    }
    
    public async Task LeaveLobby()
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            await _connection.InvokeAsync("LeaveLobby");
        }
    }

    public async Task LeaveGame()
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            await _connection.InvokeAsync("LeaveGame");
        }
    }
    
    private void Register()
    {
        _connection.On("RefreshMailAlert", () =>
        {
            Managers.Dispatcher.Enqueue( () => OnInvitationSent?.Invoke());
        });
        
        _connection.On("ToastNotification", () =>
        {
            Managers.Dispatcher.Enqueue(() => _ = OnToastNotification());
        });

        _connection.On<DeckInfo>("SwitchDeck", deckInfo =>
        {
            Managers.Dispatcher.Enqueue(() => OnEnemyDeckSwitched?.Invoke(deckInfo));
        });

        _connection.On<DeckInfo, DeckInfo, bool>("SwitchFaction", (myDeckInfo , enemyDeckInfo, isGuest) =>
        {
            Managers.Dispatcher.Enqueue(() => OnFactionSwitched?.Invoke(myDeckInfo, enemyDeckInfo, isGuest));
        });

        _connection.On<AcceptInvitationPacketResponse>("GameRoomJoined", response =>
        {
            Managers.Dispatcher.Enqueue(() => OnInvitationSuccess?.Invoke(response));
        });

        _connection.On<AcceptInvitationPacketResponse>("JoinGameRoom", response =>
        {
            Managers.Dispatcher.Enqueue(() => OnEnterFriendlyMatch?.Invoke(response));
        });

        _connection.On<AcceptInvitationPacketResponse>("RejectInvitation", response =>
        {
            Managers.Dispatcher.Enqueue(() => _ = OnRejectInvitation());
        });

        _connection.On<FriendRequestPacketResponse>("FriendRequestNotification", response =>
        {
            Managers.Dispatcher.Enqueue(() => OnFriendRequestNotification(response));
        });
        
        _connection.On("GameRoomClosedByHost",() =>
        {
            Managers.Dispatcher.Enqueue(() => _ = OnGameRoomClosedByHost());
        });
        
        _connection.On("GameRoomClosed", () =>
        {
            Managers.Dispatcher.Enqueue(() => Managers.Scene.LoadScene(Define.Scene.MainLobby));
        });
        
        _connection.On("GuestLeftGameRoom", () =>
        {
            Managers.Dispatcher.Enqueue(() => OnGuestLeft?.Invoke());
        });
        
        _connection.On("LeftGameRoom", () =>
        {
            Managers.Dispatcher.Enqueue(() => Managers.Scene.LoadScene(Define.Scene.MainLobby));
        });
        
        _connection.On("StartSession", () =>
        {
            Managers.Dispatcher.Enqueue(() => { OnStartFriendlyMatch?.Invoke(); });
        });
        
        _connection.On("MatchFailed", () =>
        {
            Managers.Dispatcher.Enqueue(() => _ = MatchFailed());
        });
    }

    public async Task SwitchDeckOnFriendlyMatch(string token, Faction faction)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            await _connection.InvokeAsync("SwitchDeckOnFriendlyMatch", token, faction);
        }
    }
    
    public async Task SwitchFactionOnFriendlyMatch(Faction faction)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            var switchedFaction = faction == Faction.Sheep ? Faction.Wolf : Faction.Sheep;
            Util.Faction = switchedFaction;
            
            const string methodName = "SwitchFactionOnFriendlyMatch";
            await _connection.InvokeAsync(methodName, switchedFaction);
        }
    }
    
    public async Task<LoadInvitableFriendPacketResponse> LoadFriends(LoadInvitableFriendPacketRequired required)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            const string methodName = "LoadFriends";
            return await _connection.InvokeAsync<LoadInvitableFriendPacketResponse>(methodName, required);
        }

        Debug.Log("Connection is not established.");
        return new LoadInvitableFriendPacketResponse();
    }

    public async Task<InviteFriendlyMatchPacketResponse> SendInvitation(InviteFriendlyMatchPacketRequired required)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            const string methodName = "HandleInviteFriendlyMatch";
            return await _connection.InvokeAsync<InviteFriendlyMatchPacketResponse>(methodName, required);
        }
        
        Debug.Log("Connection is not established.");
        return new InviteFriendlyMatchPacketResponse();
    }
    
    public async Task<AcceptInvitationPacketResponse> SendAcceptInvitation(AcceptInvitationPacketRequired required)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            const string methodName = "HandleAcceptInvitation";
            return await _connection.InvokeAsync<AcceptInvitationPacketResponse>(methodName, required);
        }
        
        Debug.Log("Connection is not established.");
        return new AcceptInvitationPacketResponse();
    }
    
    public async Task<FriendRequestPacketResponse> SendFriendRequest(FriendRequestPacketRequired required)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            const string methodName = "HandleFriendRequest";
            return await _connection.InvokeAsync<FriendRequestPacketResponse>(methodName, required);
        }
        
        Debug.Log("Connection is not established.");
        return new FriendRequestPacketResponse();
    }

    private async Task OnToastNotification()
    {
        var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
        await Managers.Localization.UpdateWarningPopupText(popup, "warning_invitation_sent");
    }
    
    private async Task OnRejectInvitation()
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        await Managers.Localization.UpdateNotifyPopupText(popup, "notify_invitation_rejected");
    }
    
    private void OnFriendRequestNotification(FriendRequestPacketResponse response)
    {
        OnFriendRequestNotificationReceived?.Invoke(response);
    }

    private async Task OnGameRoomClosedByHost()
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        await Managers.Localization.UpdateNotifyPopupText(popup, "notify_game_room_closed_by_host");
        popup.SetYesCallback(() => Managers.Scene.LoadScene(Define.Scene.MainLobby));
        
        Managers.Network.IsFriendlyMatchHost = false;
        Managers.Game.ReEntry = false;
        Managers.Game.ReEntryResponse = null;
    }

    public async Task StartFriendlyMatch(string userTag)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            await _connection.InvokeAsync("StartFriendlyMatch", userTag);
        }
    }

    public async Task SendSessionId(int sessionId)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            await _connection.InvokeAsync("GetSessionId", sessionId);
        }
    }

    private async Task MatchFailed()
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        await Managers.Localization.UpdateNotifyPopupText(popup, "notify_friendly_match_failed");
        popup.SetYesCallback(() => Managers.Scene.LoadScene(Define.Scene.MainLobby));
    }
    
    public async Task<Tuple<bool, AcceptInvitationPacketResponse>> ReEntryFriendlyMatch(string userTag)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            return await _connection
                .InvokeAsync<Tuple<bool, AcceptInvitationPacketResponse>>("ReEntryFriendlyMatch", userTag);
        }       
        
        Debug.LogWarning("Connection is not established. Cannot re-enter friendly match.");
        Managers.Scene.LoadScene(Define.Scene.MainLobby);
        return new Tuple<bool, AcceptInvitationPacketResponse>(false, new AcceptInvitationPacketResponse());
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
