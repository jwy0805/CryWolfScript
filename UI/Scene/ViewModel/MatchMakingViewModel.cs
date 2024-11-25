using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using Zenject;

public class MatchMakingViewModel
{
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    
    public event Action OnMatchMakingStarted; 
    
    public int SessionId { get; set; }
    
    [Inject]
    public MatchMakingViewModel(IWebService webService, ITokenService tokenService)
    {
        _webService = webService;
        _tokenService = tokenService;
    }

    public void ConnectSocketServer()
    {
        Managers.Network.ConnectGameSession();
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

    public void TestMatchMaking()
    {
        var packet = new ChangeActTestPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            SessionId = SessionId,
            Faction = Util.Faction,
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
    
    public void CancelMatchMaking()
    {
        var cancelPacket = new CancelMatchPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
        };
        
        _webService.SendWebRequest<CancelMatchPacketResponse>(
            "Match/CancelMatchMaking", "PUT", cancelPacket, response =>
            {
                if (response.CancelOk == false) return;
                
                Managers.Network.Disconnect();
                Managers.Scene.LoadScene(Define.Scene.MainLobby);
            });
    }
}
