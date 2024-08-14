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
    
    [Inject]
    public MatchMakingViewModel(IWebService webService, ITokenService tokenService)
    {
        _webService = webService;
        _tokenService = tokenService;
    }

    public void StartMatchMaking()
    {
        var changeActPacket = new ChangeActPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Camp = Util.Camp,
            Act = UserAct.MatchMaking,
            MapId = Managers.Map.MapId
        };
        
        _webService.SendWebRequest<ChangeActPacketResponse>(
            "UserAccount/ChangeAct", "PUT", changeActPacket, response =>
            {
                if (response.ChangeOk)
                {
                    Managers.Network.ConnectGameSession();
                }
            });
        
        OnMatchMakingStarted?.Invoke();
    }

    public void CancelMatchMaking()
    {
        var changeActPacket = new ChangeActPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Act = UserAct.InLobby
        };
        
        _webService.SendWebRequest<ChangeActPacketResponse>(
            "UserAccount/ChangeAct", "PUT", changeActPacket, response =>
            {
                if (response.ChangeOk) return;
                Managers.Network.Disconnect();
                Managers.Scene.LoadScene(Define.Scene.MainLobby);
                Managers.Clear();
            });
    }
}
