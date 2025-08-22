using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class FriendlyMatchViewModel : IDisposable
{
    private IWebService _webService;
    private ITokenService _tokenService;
    private ISignalRClient _signalRClient;
    
    public int SessionId { get; set; }
    
    [Inject]
    public FriendlyMatchViewModel(IWebService webService, ITokenService tokenService, ISignalRClient signalRClient)
    {
        _webService = webService;
        _tokenService = tokenService;
        _signalRClient = signalRClient;
    }

    public async Task JoinGame()
    {
        await _signalRClient.JoinGame(_tokenService.GetAccessToken());
    }
    
    public async Task SwitchFaction()
    {
        await _signalRClient.SwitchFactionOnFriendlyMatch(Util.Faction);
    }

    public async Task SendSessionId(int sessionId)
    {
        await _signalRClient.SendSessionId(sessionId);
    }
    
    public void Dispose()
    {
        
    }
}
