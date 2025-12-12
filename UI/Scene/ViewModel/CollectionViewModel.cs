using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

public class CollectionViewModel
{
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    
    public event Func<Faction, Task> OnCardInitialized;
    public event Func<Faction, Task> OnCardSwitched;
    
    [Inject]
    public CollectionViewModel(IUserService userService, IWebService webService, ITokenService tokenService)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
    }
    
    public async Task Initialize()
    {
        // Load unit & item data
        await LoadInfoAsync();
        await Task.WhenAll(
            InitializeCards(), InitializeSheep(), InitializeEnchants(), InitializeCharacters(), InitializeMaterials());
        
        OnCardInitialized?.Invoke(Util.Faction);
    }
    
    // DB caching only before introducing REDIS
    private async Task LoadInfoAsync()
    {
        var loadInfoTask = _webService.SendWebRequestAsync<LoadInfoPacketResponse>(
            "Collection/LoadInfo", UnityWebRequest.kHttpVerbPOST, new LoadInfoPacketRequired
            {
                LoadInfo = true
            });
        
        await loadInfoTask;
        var loadInfoResponse = loadInfoTask.Result;
        if (loadInfoResponse.LoadInfoOk == false) return;
        Managers.Data.UnitInfoDict = loadInfoResponse.UnitInfos.ToDictionary(info => info.Id, info => info);
        Managers.Data.EnchantInfoDict = loadInfoResponse.EnchantInfos.ToDictionary(info => info.Id, info => info);
        Managers.Data.SheepInfoDict = loadInfoResponse.SheepInfos.ToDictionary(info => info.Id, info => info);
        Managers.Data.CharacterInfoDict = loadInfoResponse.CharacterInfos.ToDictionary(info => info.Id, info => info);
        Managers.Data.MaterialInfoDict = loadInfoResponse.MaterialInfos.ToDictionary(info => info.Id, info => info);
        Managers.Data.ReinforcePointDict = loadInfoResponse.ReinforcePoints
            .ToDictionary(info => Tuple.Create(info.Class, info.Level), info => info);
        Managers.Data.CraftingMaterialDict = loadInfoResponse.CraftingMaterials
            .ToDictionary(info => info.UnitId, info => info);
    }

    private async Task InitializeCards()
    {
        var cardPacket = new InitCardsPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Environment = Managers.Network.Environment
        };
        
        var cardTask = _webService.SendWebRequestAsync<InitCardsPacketResponse>(
            "Collection/InitCards", "POST", cardPacket);
        
        await cardTask;
        
        var cardResponse = cardTask.Result;
        if (cardResponse.GetCardsOk == false) return;
        if (cardResponse.AccessToken != null)
        {
            _tokenService.SaveAccessToken(cardResponse.AccessToken);
            _tokenService.SaveRefreshToken(cardResponse.RefreshToken);
        }

        _userService.LoadOwnedUnit(cardResponse.OwnedCardList);
        _userService.LoadNotOwnedUnit(cardResponse.NotOwnedCardList);
    }

    private async Task InitializeSheep()
    {
        var sheepPacket = new InitSheepPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Environment = Managers.Network.Environment
        };
        
        var sheepTask = _webService.SendWebRequestAsync<InitSheepPacketResponse>(
            "Collection/InitSheep", "POST", sheepPacket);
        
        await sheepTask;
        
        var sheepResponse = sheepTask.Result;
        if (sheepResponse.GetSheepOk == false) return;
        if (sheepResponse.AccessToken != null)
        {
            _tokenService.SaveAccessToken(sheepResponse.AccessToken);
            _tokenService.SaveRefreshToken(sheepResponse.RefreshToken);
        }

        _userService.LoadOwnedSheep(sheepResponse.OwnedSheepList);
        _userService.LoadNotOwnedSheep(sheepResponse.NotOwnedSheepList);
    }

    private async Task InitializeEnchants()
    {
        var enchantPacket = new InitEnchantPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Environment = Managers.Network.Environment
        };
        
        var enchantTask = _webService.SendWebRequestAsync<InitEnchantPacketResponse>(
            "Collection/InitEnchants", "POST", enchantPacket);
        
        await enchantTask;
        
        var enchantResponse = enchantTask.Result;
        if (enchantResponse.GetEnchantOk == false) return;
        if (enchantResponse.AccessToken != null)
        {
            _tokenService.SaveAccessToken(enchantResponse.AccessToken);
            _tokenService.SaveRefreshToken(enchantResponse.RefreshToken);
        }

        _userService.LoadOwnedEnchant(enchantResponse.OwnedEnchantList);
        _userService.LoadNotOwnedEnchant(enchantResponse.NotOwnedEnchantList);
    }

    private async Task InitializeCharacters()
    {
        var characterPacket = new InitCharacterPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Environment = Managers.Network.Environment
        };
        
        var characterTask = _webService.SendWebRequestAsync<InitCharacterPacketResponse>(
            "Collection/InitCharacters", "POST", characterPacket);
        
        await characterTask;
        
        var characterResponse = characterTask.Result;
        if (characterResponse.GetCharacterOk == false) return;
        if (characterResponse.AccessToken != null)
        {
            _tokenService.SaveAccessToken(characterResponse.AccessToken);
            _tokenService.SaveRefreshToken(characterResponse.RefreshToken);
        }

        _userService.LoadOwnedCharacter(characterResponse.OwnedCharacterList);
        _userService.LoadNotOwnedCharacter(characterResponse.NotOwnedCharacterList);
    }
    
    private async Task InitializeMaterials()
    {
        var materialPacket = new InitMaterialPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Environment = Managers.Network.Environment
        };
        
        var materialTask = _webService.SendWebRequestAsync<InitMaterialPacketResponse>(
            "Collection/InitMaterials", "POST", materialPacket);
        
        await materialTask;
        
        var materialResponse = materialTask.Result;
        if (materialResponse.GetMaterialOk == false) return;
        if (materialResponse.AccessToken != null)
        {
            _tokenService.SaveAccessToken(materialResponse.AccessToken);
            _tokenService.SaveRefreshToken(materialResponse.RefreshToken);
        }
        
        _userService.LoadOwnedMaterial(materialResponse.OwnedMaterialList);
    }
    
    public int GetLevelFromUiObject(UnitId unitId)
    {
        var level = (int)unitId % 100 % 3;
        if (level == 0) { level = 3; }

        return level;
    }

    public void SwitchCards(Faction faction)
    {
        OnCardSwitched?.Invoke(faction);
    }
}
