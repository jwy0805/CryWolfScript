using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

public class MatchMakingViewModel
{
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    
    public event Action OnMatchMakingStarted; 
    public event Action<int, int> OnRefreshQueueCounts; 
    
    public int SessionId { get; set; }
    
    [Inject]
    public MatchMakingViewModel(IWebService webService, ITokenService tokenService)
    {
        _webService = webService;
        _tokenService = tokenService;
    }

    public void ConnectSocketServer()
    {
        _ = Managers.Network.ConnectGameSession();
    }
    
    public void StartMatchMaking()
    {
        // Change the act to match making
        var packet = new ChangeActPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            SessionId = SessionId,
            Faction = Util.Faction,
            MapId = Managers.Map.MapId
        };

        _webService
            .SendWebRequest<ChangeActPacketResponse>("Match/ChangeActByMatchMaking", "PUT", packet, _ => { });
        
        OnMatchMakingStarted?.Invoke();
    }

    public void EnterGame(int mapId = 1)
    {
        Managers.Map.MapId = mapId;
        Managers.Scene.LoadScene(Define.Scene.Game);
    }

    public async Task GetQueueCounts()
    {
        var packet = new GetQueueCountsPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
        };
        
        var task = await _webService.SendWebRequestAsync<GetQueueCountsPacketResponse>(
            "Match/GetQueueCounts", UnityWebRequest.kHttpVerbPOST, packet);

        if (task.GetQueueCountsOk)
        {
            OnRefreshQueueCounts?.Invoke(task.QueueCountsSheep, task.QueueCountsSheep);
        }
    }
    
    public void TestMatchMaking()
    {
        Debug.Log("Start Test");
        var packet = new ChangeActTestPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            SessionId = SessionId,
            Faction = Util.Faction == Faction.Wolf ? Faction.Sheep : Faction.Wolf,
            MapId = Managers.Map.MapId
        };
        
        var cancelPacket = new CancelMatchPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
        };        
        
        _webService
            .SendWebRequest<ChangeActTestPacketResponse>("Match/TestMatchMaking", "PUT", packet, _ => { });
        _webService.SendWebRequest<CancelMatchPacketResponse>(
            "Match/CancelMatchMaking", "PUT", cancelPacket, _ => { });
    }
    
    public async Task CancelMatchMaking()
    {
        var cancelPacket = new CancelMatchPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
        };
        
        var task = await _webService.SendWebRequestAsync<CancelMatchPacketResponse>(
            "Match/CancelMatchMaking", UnityWebRequest.kHttpVerbPUT, cancelPacket);

        if (task.CancelOk)
        {
            Managers.Network.Disconnect();
            Managers.Scene.LoadScene(Define.Scene.MainLobby);
        }
    }
}
