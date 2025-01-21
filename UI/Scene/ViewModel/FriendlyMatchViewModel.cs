using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class FriendlyMatchViewModel : IDisposable
{
    private IWebService _webService;
    private ITokenService _tokenService;
    
    public int SessionId { get; set; }
    
    [Inject]
    public FriendlyMatchViewModel(IWebService webService, ITokenService tokenService)
    {
        _webService = webService;
        _tokenService = tokenService;
    }
    
    public void SwitchFaction()
    {
        
    }

    public void Dispose()
    {
        
    }
}
