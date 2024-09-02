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

    public event Action<Camp> OnDeckInitialized;
    public event Action<Camp> OnDeckSelected;
    public event Action<Camp> OnDeckSwitched;
    
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
        
        _userService.BindDeck();
        OnDeckInitialized?.Invoke(Util.Camp);
    }

    public Deck GetDeck(Camp camp)
    {
        return camp == Camp.Sheep ? User.Instance.DeckSheep : User.Instance.DeckWolf;
    }
    
    public void UpdateDeck(Card oldCard, Card newCard)
    {
        var deck = GetDeck(Util.Camp);
        var index = Array.FindIndex(deck.UnitsOnDeck, unitInfo => unitInfo.Id == oldCard.UnitInfo.Id);
        if (index == -1) return;
        
        var unitIdToBeDeleted = deck.UnitsOnDeck[index].Id;
        var unitIdToBeUpdated = newCard.UnitInfo.Id;
        deck.UnitsOnDeck[index] = newCard.UnitInfo;
        deck.UnitsOnDeck = deck.UnitsOnDeck.OrderBy(unit => unit.Class).ToArray();

        if (Util.Camp == Camp.Sheep)
        {
            User.Instance.DeckSheep = deck;
        }
        else
        {
            User.Instance.DeckWolf = deck;
        }
        
        var updateDeckPacket = new UpdateDeckPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            DeckId = deck.DeckId,
            UnitIdToBeDeleted = unitIdToBeDeleted,
            UnitIdToBeUpdated = unitIdToBeUpdated
        };
        
        _webService.SendWebRequest<UpdateDeckPacketResponse>(
            "Collection/UpdateDeck", "PUT", updateDeckPacket, _ => { });
    }

    public void SelectDeck(int buttonNum, Camp camp)
    {
        var deckList = Util.Camp == Camp.Sheep ? User.Instance.AllDeckSheep : User.Instance.AllDeckWolf;
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
        
        if (Util.Camp == Camp.Sheep)
        {
            User.Instance.DeckSheep = deck;
        }
        else
        {
            User.Instance.DeckWolf = deck;
        }
        
        OnDeckSelected?.Invoke(camp);
    }
    
    public void SwitchDeck(Camp camp)
    {
        var deckList = camp == Camp.Sheep ? User.Instance.AllDeckSheep : User.Instance.AllDeckWolf;
        var deck = deckList.FirstOrDefault(d => d.LastPicked) 
                   ?? deckList.First(d => d.DeckNumber == 1);
        
        if (camp == Camp.Sheep)
        {
            User.Instance.DeckSheep = deck;
        }
        else
        {
            User.Instance.DeckWolf = deck;
        }
        
        OnDeckSwitched?.Invoke(camp);
    }
}
