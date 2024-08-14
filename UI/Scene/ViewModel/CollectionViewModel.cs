using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using Zenject;

public class CollectionViewModel
{
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    
    public event Action<Camp> OnCardInitialized;
    public event Action<Camp> OnCardSwitched;
    
    [Inject]
    public CollectionViewModel(IUserService userService, IWebService webService, ITokenService tokenService)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
    }
    
    public async Task Initialize()
    {
        await InitializeCards();
    }

    private async Task InitializeCards()
    {
        var cardPacket = new GetOwnedCardsPacketRequired()
        {
            AccessToken = _tokenService.GetAccessToken(),
            Environment = _webService.Environment
        };
        
        var cardTask = _webService.SendWebRequestAsync<GetOwnedCardsPacketResponse>(
            "Collection/GetCards", "POST", cardPacket);
        
        await cardTask;
        
        var cardResponse = cardTask.Result;
        if (cardResponse.GetCardsOk == false) return;
        if (cardResponse.AccessToken != null)
        {
            _tokenService.SaveAccessToken(cardResponse.AccessToken);
            _tokenService.SaveRefreshToken(cardResponse.RefreshToken);
        }
        
        foreach (var unitInfo in cardResponse.OwnedCardList)
        {
            _userService.LoadOwnedUnit(unitInfo);
        }

        foreach (var unitInfo in cardResponse.NotOwnedCardList)
        {
            _userService.LoadNotOwnedUnit(unitInfo);
        }
        
        OnCardInitialized?.Invoke(Util.Camp);
    }

    public void OrderCardsByClass()
    {
        User.Instance.OwnedCardListSheep.Sort((a, b) => a.Class.CompareTo(b.Class));
        User.Instance.NotOwnedCardListSheep.Sort((a, b) => a.Class.CompareTo(b.Class));
        
        User.Instance.OwnedCardListWolf.Sort((a, b) => a.Class.CompareTo(b.Class));
        User.Instance.NotOwnedCardListWolf.Sort((a, b) => a.Class.CompareTo(b.Class));
    }

    public void SwitchCards(Camp camp)
    {
        OnCardSwitched?.Invoke(camp);
    }
}
