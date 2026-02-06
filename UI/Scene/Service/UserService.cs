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
    
    public User User { get; }

    public UserTutorialInfo TutorialInfo { get; set; }
    
    [Inject]
    public UserService(ITokenService tokenService, IWebService webService)
    {
        _tokenService = tokenService;
        _webService = webService;
        User = new User();
    }
    
    public void LoadOwnedUnit(List<OwnedUnitInfo> ownedUnitsFromServer)
    {
        User.OwnedUnitList.Clear();
        User.NotOwnedUnitList.Clear();

        var ownedById = ownedUnitsFromServer
            .Where(x => x?.UnitInfo != null)
            .ToDictionary(x => x.UnitInfo.Id, x => x);

        foreach (var info in Managers.Data.UnitInfoDict.Values)
        {
            if (ownedById.TryGetValue(info.Id, out var owned))
            {
                // 서버가 준 UnitInfo 대신, 클라 UnitInfo로 교체(참조 일관성)
                owned.UnitInfo = info;
                User.OwnedUnitList.Add(owned);
            }
            else
            {
                User.NotOwnedUnitList.Add(info);
            }
        }
    }

    public void LoadOwnedSheep(List<OwnedSheepInfo> ownedSheepFromServer)
    {
        User.OwnedSheepList.Clear();
        User.NotOwnedSheepList.Clear();

        var ownedById = ownedSheepFromServer
            .Where(x => x?.SheepInfo != null)
            .ToDictionary(x => x.SheepInfo.Id, x => x);

        foreach (var master in Managers.Data.SheepInfoDict.Values)
        {
            if (ownedById.TryGetValue(master.Id, out var owned))
            {
                owned.SheepInfo = master;
                User.OwnedSheepList.Add(owned);
            }
            else
            {
                User.NotOwnedSheepList.Add(master);
            }
        }
    }

    public void LoadOwnedEnchant(List<OwnedEnchantInfo> ownedEnchantsFromServer)
    {
        User.OwnedEnchantList.Clear();
        User.NotOwnedEnchantList.Clear();

        var ownedById = ownedEnchantsFromServer
            .Where(x => x?.EnchantInfo != null)
            .ToDictionary(x => x.EnchantInfo.Id, x => x);

        foreach (var master in Managers.Data.EnchantInfoDict.Values)
        {
            if (ownedById.TryGetValue(master.Id, out var owned))
            {
                owned.EnchantInfo = master;
                User.OwnedEnchantList.Add(owned);
            }
            else
            {
                User.NotOwnedEnchantList.Add(master);
            }
        }
    }

    public void LoadOwnedCharacter(List<OwnedCharacterInfo> ownedCharactersFromServer)
    {
        User.OwnedCharacterList.Clear();
        User.NotOwnedCharacterList.Clear();

        var ownedById = ownedCharactersFromServer
            .Where(x => x?.CharacterInfo != null)
            .ToDictionary(x => x.CharacterInfo.Id, x => x);

        foreach (var master in Managers.Data.CharacterInfoDict.Values)
        {
            if (ownedById.TryGetValue(master.Id, out var owned))
            {
                owned.CharacterInfo = master;
                User.OwnedCharacterList.Add(owned);
            }
            else
            {
                User.NotOwnedCharacterList.Add(master);
            }
        }
    }
    
    public void LoadOwnedMaterial(List<OwnedMaterialInfo> materials)
    {
        User.OwnedMaterialList.Clear();

        foreach (var material in materials)
        {
            User.OwnedMaterialList.Add(material);
        }
    }
    
    public void LoadBattleSetting(BattleSettingInfo battleSettingInfo)
    {
        User.BattleSetting = battleSettingInfo;
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
            User.AllDeckSheep.Add(deck);
        }
        else
        {
            User.AllDeckWolf.Add(deck);
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
            User.DeckSheep = deck;
        }
        else
        {
            User.DeckWolf = deck;
        }
    }

    public void BindDeck()
    {
        User.DeckSheep = User.AllDeckSheep.Any(deck => deck.LastPicked) 
            ? User.AllDeckSheep.First(deck => deck.LastPicked) 
            : User.AllDeckSheep.First();
        User.DeckWolf = User.AllDeckWolf.Any(deck => deck.LastPicked) 
            ? User.AllDeckWolf.First(deck => deck.LastPicked) 
            : User.AllDeckWolf.First();
        
        InitDeckButton?.Invoke(Util.Faction);
    }
    
    public async Task LoadUserInfo()
    {
        var loadUserInfoPacket = new LoadUserInfoPacketRequired{ AccessToken = _tokenService.GetAccessToken() };
        var res = await _webService.SendWebRequestAsync<LoadUserInfoPacketResponse>(
            "UserAccount/LoadUserInfo", "POST", loadUserInfoPacket);
        if (res.LoadUserInfoOk == false) return;
        
        TutorialInfo = res.UserTutorialInfo;
        User.ExpTable = res.ExpTable;
        User.UserInfo = res.UserInfo;
        User.SubscribeAdsRemover = res.UserInfo.Subscriptions
            .FirstOrDefault(si => si.SubscriptionType == SubscriptionType.AdsRemover) != null;
    }

    public async Task LoadTestUserInfo(int userId)
    {
        var loadTestUserPacket = new LoadTestUserPacketRequired{ UserId = userId };
        var task = _webService.SendWebRequestAsync<LoadTestUserPacketResponse>(
            "UserAccount/LoadTestUser", "POST", loadTestUserPacket);
        
        await task;
        
        TutorialInfo = task.Result.UserTutorialInfo;
        User.ExpTable = task.Result.ExpTable;
        User.UserInfo = task.Result.UserInfo;
        
        _tokenService.SaveAccessToken(task.Result.AccessToken);
        _tokenService.SaveRefreshToken(task.Result.RefreshToken);
    }
}
