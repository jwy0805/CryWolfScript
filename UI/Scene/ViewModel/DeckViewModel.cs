using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using Zenject;

public class DeckViewModel
{
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;

    public event Action<Faction> OnDeckInitialized;
    public event Action<Faction> OnDeckSwitched;
    
    [Inject]
    public DeckViewModel(IUserService userService, IWebService webService, ITokenService tokenService)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
    }
    
    public async Task Initialize()
    {
        await InitializeDecks();
    }
    
    private async Task InitializeDecks()
    {
        var deckPacket = new GetInitDeckPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Environment = _webService.Environment
        };
        
        var deckTask = _webService.SendWebRequestAsync<GetInitDeckPacketResponse>(
            "Collection/GetDecks", "POST", deckPacket);
        await deckTask;
        
        var deckResponse = deckTask.Result;
        if (deckResponse.GetDeckOk == false) return;

        foreach (var deckInfo in deckResponse.DeckList)
        {
            _userService.LoadDeck(deckInfo);
        }
        
        _userService.LoadBattleSetting(deckResponse.BattleSetting);
        
        _userService.BindDeck();
        OnDeckInitialized?.Invoke(Util.Faction);
    }

    public void ResetDeckUI(Faction faction)
    {
        OnDeckSwitched?.Invoke(faction);
    }

    public void UpdateBattleSetting(Card oldCard, Card newCard)
    {
        var battleSetting = User.Instance.BattleSetting;
        var type = oldCard.AssetType;
        
        switch (type)
        {
            case Asset.Sheep:
                battleSetting.SheepInfo.Id = newCard.Id;
                battleSetting.SheepInfo.Class = newCard.Class;
                break;
            case Asset.Enchant:
                battleSetting.EnchantInfo.Id = newCard.Id;
                battleSetting.EnchantInfo.Class = newCard.Class;
                break;
            case Asset.Character:
                battleSetting.CharacterInfo.Id = newCard.Id;
                battleSetting.CharacterInfo.Class = newCard.Class;
                break;
        }
        
        var updatePacket = new UpdateBattleSettingPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            BattleSettingInfo = battleSetting
        };
        
        _webService.SendWebRequest<UpdateBattleSettingPacketResponse>(
            "Collection/UpdateBattleSetting", "PUT", updatePacket, _ => { });
    }
    
    public Deck GetDeck(Faction faction)
    {
        return faction == Faction.Sheep ? User.Instance.DeckSheep : User.Instance.DeckWolf;
    }
    
    public void UpdateDeck(Card oldCard, Card newCard)
    {
        var deck = GetDeck(Util.Faction);
        var index = Array.FindIndex(deck.UnitsOnDeck, unitInfo => unitInfo.Id == oldCard.Id);
        if (index == -1) return;
        
        var unitIdToBeDeleted = deck.UnitsOnDeck[index].Id;
        var unitIdToBeUpdated = newCard.Id;

        deck.UnitsOnDeck[index] = User.Instance.OwnedUnitList
            .Select(info => info.UnitInfo)
            .FirstOrDefault(info => info.Id == newCard.Id);
        if (Util.Faction == Faction.Sheep)
        {
            User.Instance.DeckSheep = deck;
        }
        else
        {
            User.Instance.DeckWolf = deck;
        }
        
        deck.UnitsOnDeck = deck.UnitsOnDeck.OrderBy(unit => unit.Class).ToArray();

        var updateDeckPacket = new UpdateDeckPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            DeckId = deck.DeckId,
            UnitIdToBeDeleted = (UnitId)unitIdToBeDeleted,
            UnitIdToBeUpdated = (UnitId)unitIdToBeUpdated
        };
        
        _webService.SendWebRequest<UpdateDeckPacketResponse>(
            "Collection/UpdateDeck", "PUT", updateDeckPacket, _ => { });
    }

    public void SelectDeck(int buttonNum, Faction faction)
    {
        var deckList = Util.Faction == Faction.Sheep ? User.Instance.AllDeckSheep : User.Instance.AllDeckWolf;
        var deck = deckList.First(deck => deck.DeckNumber == buttonNum);

        foreach (var d in deckList) d.LastPicked = false;
        deck.LastPicked = true;
        
        var packetDict = deckList.ToDictionary(d => d.DeckId, d => d.LastPicked);
        var updateLastDeckPacket = new UpdateLastDeckPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            LastPickedInfo = packetDict
        };
        
        _webService.SendWebRequest<UpdateLastDeckPacketResponse>(
            "Collection/UpdateLastDeck", "PUT", updateLastDeckPacket, _ => { });
        
        if (Util.Faction == Faction.Sheep)
        {
            User.Instance.DeckSheep = deck;
        }
        else
        {
            User.Instance.DeckWolf = deck;
        }
        
        OnDeckSwitched?.Invoke(faction);
    }
    
    public void SwitchDeck(Faction faction)
    {
        var deckList = faction == Faction.Sheep ? User.Instance.AllDeckSheep : User.Instance.AllDeckWolf;
        var deck = deckList.FirstOrDefault(d => d.LastPicked) 
                   ?? deckList.First(d => d.DeckNumber == 1);
        
        if (faction == Faction.Sheep)
        {
            User.Instance.DeckSheep = deck;
        }
        else
        {
            User.Instance.DeckWolf = deck;
        }
        
        OnDeckSwitched?.Invoke(faction);
    }
}
