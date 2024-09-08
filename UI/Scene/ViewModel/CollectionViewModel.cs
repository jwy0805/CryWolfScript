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
        await Task.WhenAll(InitializeSheep(), InitializeEnchant(), InitializeCharacter());
        
        OnCardInitialized?.Invoke(Util.Camp);
    }

    private async Task InitializeCards()
    {
        var cardPacket = new GetOwnedCardsPacketRequired
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
    }

    private async Task InitializeSheep()
    {
        var sheepPacket = new GetOwnedSheepPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Environment = _webService.Environment
        };
        
        var sheepTask = _webService.SendWebRequestAsync<GetOwnedSheepPacketResponse>(
            "Collection/GetSheep", "POST", sheepPacket);
        
        await sheepTask;
        
        var sheepResponse = sheepTask.Result;
        if (sheepResponse.GetSheepOk == false) return;
        if (sheepResponse.AccessToken != null)
        {
            _tokenService.SaveAccessToken(sheepResponse.AccessToken);
            _tokenService.SaveRefreshToken(sheepResponse.RefreshToken);
        }
        
        foreach (var sheepInfo in sheepResponse.OwnedSheepList)
        {
            _userService.LoadOwnedSheep(sheepInfo);
        }

        foreach (var sheepInfo in sheepResponse.NotOwnedSheepList)
        {
            _userService.LoadNotOwnedSheep(sheepInfo);
        }
    }

    private async Task InitializeEnchant()
    {
        var enchantPacket = new GetOwnedEnchantPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Environment = _webService.Environment
        };
        
        var enchantTask = _webService.SendWebRequestAsync<GetOwnedEnchantPacketResponse>(
            "Collection/GetEnchants", "POST", enchantPacket);
        
        await enchantTask;
        
        var enchantResponse = enchantTask.Result;
        if (enchantResponse.GetEnchantOk == false) return;
        if (enchantResponse.AccessToken != null)
        {
            _tokenService.SaveAccessToken(enchantResponse.AccessToken);
            _tokenService.SaveRefreshToken(enchantResponse.RefreshToken);
        }
        
        foreach (var enchantInfo in enchantResponse.OwnedEnchantList)
        {
            _userService.LoadOwnedEnchant(enchantInfo);
        }
        
        foreach (var enchantInfo in enchantResponse.NotOwnedEnchantList)
        {
            _userService.LoadNotOwnedEnchant(enchantInfo);
        }
    }

    private async Task InitializeCharacter()
    {
        var characterPacket = new GetOwnedCharacterPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Environment = _webService.Environment
        };
        
        var characterTask = _webService.SendWebRequestAsync<GetOwnedCharacterPacketResponse>(
            "Collection/GetCharacters", "POST", characterPacket);
        
        await characterTask;
        
        var characterResponse = characterTask.Result;
        if (characterResponse.GetCharacterOk == false) return;
        if (characterResponse.AccessToken != null)
        {
            _tokenService.SaveAccessToken(characterResponse.AccessToken);
            _tokenService.SaveRefreshToken(characterResponse.RefreshToken);
        }
        
        foreach (var characterInfo in characterResponse.OwnedCharacterList)
        {
            _userService.LoadOwnedCharacter(characterInfo);
        }
        
        foreach (var characterInfo in characterResponse.NotOwnedCharacterList)
        {
            _userService.LoadNotOwnedCharacter(characterInfo);
        }
    }
    
    public void OrderCardsByClass<T>(List<T> ownedAsset, List<T> notOwnedAsset) where T : IAsset
    {
        ownedAsset.Sort((a, b) => a.Class.CompareTo(b.Class));
        notOwnedAsset.Sort((a, b) => a.Class.CompareTo(b.Class));
    }

    public void SwitchCards(Camp camp)
    {
        OnCardSwitched?.Invoke(camp);
    }
}
