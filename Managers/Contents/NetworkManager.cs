using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Microsoft.AspNetCore.SignalR.Client;
using ModestTree;
using UnityEngine;
using Zenject;

public class NetworkManager
{
    private ServerSession _session = new();
    private int _sessionId = -1;
    private TaskCompletionSource<int> _sessionIdTcs;
    private bool _awaitingSessionId;
    private const string LocalPort = "7270";
    private const string Address = "hamonstudio.net";

    private float _timer = 0f;
    private const float CheckInterval = 3f;
    private bool _noInternetPopupShowing = false;
    public Action OnInternetRestored;

    public Env Environment => Env.Prod;
    public readonly bool UseAddressables = true;
    public bool IsFriendlyMatchHost { get; set; }

    public int SessionId
    {
        get => _sessionId;
        set
        {
            if (value == -1)
            {
                _sessionId = value;
                return;
            }

            if (_awaitingSessionId == false)
            {
                Debug.LogWarning($"Ignoring SessionId because no pending connect: {value}");
                return;
            }

            _sessionId = value;
            _awaitingSessionId = false;
            _sessionIdTcs?.TrySetResult(_sessionId);
            _sessionIdTcs = null;
            
            var sceneContext = UnityEngine.Object.FindAnyObjectByType<SceneContext>();
            if (sceneContext == null)
            {
                Debug.LogWarning("SceneContext not found. Cannot set SessionId.");
                return;
            }
            
            var tutorialVm = sceneContext.Container.TryResolve<TutorialViewModel>();
            if (tutorialVm == null)
            {
                if (GameObject.FindWithTag("UI").TryGetComponent(out UI_MatchMaking uiMatchMaking))
                {
                    uiMatchMaking.StartMatchMaking(_sessionId);
                    return;
                }
            
                if (GameObject.FindWithTag("UI").TryGetComponent(out UI_SinglePlay uiSinglePlay))
                {
                    uiSinglePlay.StartSinglePlay(_sessionId);
                    return;
                }
            }
            else
            {
                if (tutorialVm.ProcessTutorial)
                {
                    _ = tutorialVm.StartTutorial(tutorialVm.TutorialFaction, _sessionId);
                }
            }
            
            var friendlyMatchVm = sceneContext.Container.TryResolve<FriendlyMatchViewModel>();
            if (friendlyMatchVm != null)
            {
                Debug.Log("Setting SessionId for FriendlyMatchViewModel: " + _sessionId);
                _ = friendlyMatchVm.SendSessionId(_sessionId);
            }
        }
    }
    
    public string BaseUrl
    {
        get
        {
            return Managers.Network.Environment switch
            {
                Env.Dev => $"https://{Address}",
                Env.Prod => $"https://{Address}",
                _ => $"https://localhost:{LocalPort}"
            };
        }
    }
    
    public void Send(IMessage packet)
    {
        _session.Send(packet);
    }

    public void Update()
    {
        List<PacketMessage> list = PacketQueue.Instance.PopAll();
        foreach (var packet in list)
        {
            Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
            handler?.Invoke(_session, packet.Message);
        }
        
        _timer += Time.deltaTime;
        if (_timer >= CheckInterval)
        {
            _timer = 0f;
            _ = CheckInternetConnection();
        }
    }

    private async Task CheckInternetConnection()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (_noInternetPopupShowing == false)
            {
                _noInternetPopupShowing = true;
                await Managers.UI.ShowPopupUI<UI_NetworkErrorPopup>();
            }
        }
        else
        {
            if (_noInternetPopupShowing)
            {
                _noInternetPopupShowing = false;
                Managers.UI.ClosePopupUI<UI_NetworkErrorPopup>();
            }
        }
    }
    
    public async Task<bool> ConnectGameSession(bool test = false)
    {
        // DNS (Domain Name System)
        string host;
        int port;
        IPHostEntry ipHost;
        IPAddress ipAddress;
        
        switch (Environment)
        {
            case Env.Local:
                host = Dns.GetHostName();
                port = 7777;
                ipHost = await Dns.GetHostEntryAsync(host);
                ipAddress = ipHost.AddressList.FirstOrDefault(ip => ip.ToString().Contains("172."));
                foreach (var address in ipHost.AddressList)
                {
                    // Debug.Log(address);
                }
                break;
            
            case Env.Dev:
                host = "tcp.hamonstudio.net";
                port = 7780;
                ipHost = await Dns.GetHostEntryAsync(host);
                ipAddress = ipHost.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                break;
            
            case Env.Stage:
            case Env.Prod:
                host = "tcp.hamonstudio.net";
                port = 7780;
                ipHost = await Dns.GetHostEntryAsync(host);
                ipAddress = ipHost.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                break;
            default:
                return false;
        }
        
        if (ipAddress == null) return false;

        if (_awaitingSessionId)
        {
            return await AwaitSessionIdAsync(_sessionIdTcs?.Task);
        }

        Debug.Log($"Connecting to {ipAddress} with SessionId: {_sessionId}");
        
        var sessionIdTask = WaitForSessionIdAsync();
        var endPoint = new IPEndPoint(ipAddress, port);
        _session = new ServerSession();
        new Connector().Connect(endPoint, () => _session, test);

        const int timeoutMilliseconds = 5000;
        var completedTask = await Task.WhenAny(sessionIdTask, Task.Delay(timeoutMilliseconds));
        if (completedTask != sessionIdTask)
        {
            CancelSessionIdWait();
            Disconnect();
            var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
            await Managers.Localization.UpdateWarningPopupText(popup, "warning_server_error");
            return false;
        }

        return await AwaitSessionIdAsync(sessionIdTask);
    }

    private Task<int> WaitForSessionIdAsync()
    {
        _awaitingSessionId = true;
        if (_sessionIdTcs == null || _sessionIdTcs.Task.IsCompleted)
        {
            _sessionIdTcs = new TaskCompletionSource<int>();
        }

        return _sessionIdTcs.Task;
    }

    private async Task<bool> AwaitSessionIdAsync(Task<int> sessionIdTask)
    {
        if (sessionIdTask == null) return false;

        try
        {
            await sessionIdTask;
            return true;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    private void CancelSessionIdWait()
    {
        _awaitingSessionId = false;
        if (_sessionIdTcs != null && _sessionIdTcs.Task.IsCompleted == false)
        {
            _sessionIdTcs.TrySetCanceled();
        }
        _sessionIdTcs = null;
    }
    
    // Disconnect TCP Connection
    public void Disconnect()
    {
        CancelSessionIdWait();
        _session.Disconnect();
        _sessionId = -1;
    }
}
