using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Zenject;

/* Last Modified : 25. 04. 22
 * Version : 1.02
 */

public class UserService : IUserService
{
    private readonly ITokenService _tokenService;
    private readonly IWebService _webService;

    public event Action<Faction> InitDeckButton;
    
    public UserInfo UserInfo { get; set; }
    public UserTutorialInfo TutorialInfo { get; set; }
    public bool TutorialSheepEnded { get; set; }
    public bool TutorialWolfEnded { get; set; }
    
    [Inject]
    public UserService(ITokenService tokenService, IWebService webService)
    {
        _tokenService = tokenService;
        _webService = webService;
    }
    
    public void LoadOwnedUnit(List<OwnedUnitInfo> units)
    {
        User.Instance.OwnedUnitList.Clear();
        
        foreach (var unit in units)
        {
            User.Instance.OwnedUnitList.Add(unit);
        }
    }

    public void LoadNotOwnedUnit(List<UnitInfo> units)
    {
        User.Instance.NotOwnedUnitList.Clear();

        foreach (var unit in units)
        {
            User.Instance.NotOwnedUnitList.Add(unit);
        }
    }

    public void LoadOwnedSheep(List<OwnedSheepInfo> sheep)
    {
        User.Instance.OwnedSheepList.Clear();

        foreach (var eachSheep in sheep)
        {
            User.Instance.OwnedSheepList.Add(eachSheep);
        }
    }

    public void LoadNotOwnedSheep(List<SheepInfo> sheep)
    {
        User.Instance.NotOwnedSheepList.Clear();

        foreach (var eachSheep in sheep)
        {
            User.Instance.NotOwnedSheepList.Add(eachSheep);
        }
    }
    
    public void LoadOwnedEnchant(List<OwnedEnchantInfo> enchants)
    {
        User.Instance.OwnedEnchantList.Clear();

        foreach (var enchant in enchants)
        {
            User.Instance.OwnedEnchantList.Add(enchant);
        }
    }
    
    public void LoadNotOwnedEnchant(List<EnchantInfo> enchants)
    {
        User.Instance.NotOwnedEnchantList.Clear();

        foreach (var enchant in enchants)
        {
            User.Instance.NotOwnedEnchantList.Add(enchant);
        }
    }
    
    public void LoadOwnedCharacter(List<OwnedCharacterInfo> characters)
    {
        User.Instance.OwnedCharacterList.Clear();

        foreach (var character in characters)
        {
            User.Instance.OwnedCharacterList.Add(character);
        }
    }
    
    public void LoadNotOwnedCharacter(List<CharacterInfo> characters)
    {
        User.Instance.NotOwnedCharacterList.Clear();

        foreach (var character in characters)
        {
            User.Instance.NotOwnedCharacterList.Add(character);
        }
    }    
    
    public void LoadOwnedMaterial(List<OwnedMaterialInfo> materials)
    {
        User.Instance.OwnedMaterialList.Clear();

        foreach (var material in materials)
        {
            User.Instance.OwnedMaterialList.Add(material);
        }
    }
    
    public void LoadBattleSetting(BattleSettingInfo battleSettingInfo)
    {
        User.Instance.BattleSetting = battleSettingInfo;
    }
    
    public void LoadDeck(DeckInfo deckInfo)
    {
        // Receive the user's deck information from the server and store it.
        var deck = new Deck
        {
            DeckId = deckInfo.DeckId,
            UnitsOnDeck = deckInfo.UnitInfo,
            DeckNumber = deckInfo.DeckNumber,
            Faction = (Faction)deckInfo.Faction,
            LastPicked = deckInfo.LastPicked,
        };
        
        deck.UnitsOnDeck = deck.UnitsOnDeck.OrderBy(unit => unit.Class).ToArray();
        
        if (deck.Faction == Faction.Sheep)
        {
            User.Instance.AllDeckSheep.Add(deck);
        }
        else
        {
            User.Instance.AllDeckWolf.Add(deck);
        }
    }
    
    public void SaveDeck(DeckInfo deckInfo)
    {
        // Save the user's deck information to the server.
        var deck = new Deck
        {
            DeckId = deckInfo.DeckId,
            UnitsOnDeck = deckInfo.UnitInfo,
            DeckNumber = deckInfo.DeckNumber,
            Faction = (Faction)deckInfo.Faction,
            LastPicked = deckInfo.LastPicked,
        };
        deck.UnitsOnDeck = deck.UnitsOnDeck.OrderBy(unit => unit.Class).ToArray();

        if (deck.Faction == Faction.Sheep)
        {
            User.Instance.DeckSheep = deck;
        }
        else
        {
            User.Instance.DeckWolf = deck;
        }
    }

    public void BindDeck()
    {
        User.Instance.DeckSheep = User.Instance.AllDeckSheep.Any(deck => deck.LastPicked) 
            ? User.Instance.AllDeckSheep.First(deck => deck.LastPicked) 
            : User.Instance.AllDeckSheep.First();
        User.Instance.DeckWolf = User.Instance.AllDeckWolf.Any(deck => deck.LastPicked) 
            ? User.Instance.AllDeckWolf.First(deck => deck.LastPicked) 
            : User.Instance.AllDeckWolf.First();
        
        InitDeckButton?.Invoke(Util.Faction);
    }
    
    public async Task LoadUserInfo()
    {
        var loadUserInfoPacket = new LoadUserInfoPacketRequired{ AccessToken = _tokenService.GetAccessToken() };
        var loadUserInfoTask = _webService.SendWebRequestAsync<LoadUserInfoPacketResponse>(
            "UserAccount/LoadUserInfo", "POST", loadUserInfoPacket);
        
        await loadUserInfoTask;
        
        var loadUserInfoResponse = loadUserInfoTask.Result;
        if (loadUserInfoResponse.LoadUserInfoOk == false) return;
        
        UserInfo = loadUserInfoResponse.UserInfo;
        TutorialInfo = loadUserInfoResponse.UserTutorialInfo;
        
        User.Instance.UserAccount = loadUserInfoResponse.UserInfo.UserAccount;
        User.Instance.NameInitialized = loadUserInfoResponse.UserInfo.NameInitialized;
        User.Instance.SubscribeAdsRemover = loadUserInfoResponse.UserInfo.Subscriptions
            .FirstOrDefault(si => si.SubscriptionType == SubscriptionType.AdsRemover) != null;
    }

    public async Task LoadTestUserInfo(int userId)
    {
        var loadTestUserPacket = new LoadTestUserPacketRequired{ UserId = userId };
        var task = _webService.SendWebRequestAsync<LoadTestUserPacketResponse>(
            "UserAccount/LoadTestUser", "POST", loadTestUserPacket);
        
        await task;
        
        UserInfo = task.Result.UserInfo;
        TutorialInfo = task.Result.UserTutorialInfo;
        
        _tokenService.SaveAccessToken(task.Result.AccessToken);
        _tokenService.SaveRefreshToken(task.Result.RefreshToken);
    }
}
