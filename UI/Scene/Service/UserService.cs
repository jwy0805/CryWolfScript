using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using Zenject;

public class UserService : IUserService
{
    private readonly ITokenService _tokenService;
    
    [Inject]
    public UserService(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public void LoadOwnedUnit(UnitInfo unitInfo)
    {
        if (unitInfo.Camp == Camp.Sheep)
        {
            User.Instance.OwnedCardListSheep.Add(unitInfo);
        }
        else
        {
            User.Instance.OwnedCardListWolf.Add(unitInfo);
        }
    }

    public void LoadNotOwnedUnit(UnitInfo unitInfo)
    {
        if (unitInfo.Camp == Camp.Sheep)
        {
            User.Instance.NotOwnedCardListSheep.Add(unitInfo);
        }
        else
        {
            User.Instance.NotOwnedCardListWolf.Add(unitInfo);
        }
    }

    public void LoadDeck(DeckInfo deckInfo)
    {
        // Receive the user's deck information from the server and store it.
        var deck = new Deck
        {
            DeckId = deckInfo.DeckId,
            UnitsOnDeck = deckInfo.UnitInfo,
            DeckNumber = deckInfo.DeckNumber,
            Camp = (Camp)deckInfo.Camp,
            LastPicked = deckInfo.LastPicked,
        };
        deck.UnitsOnDeck = deck.UnitsOnDeck.OrderBy(unit => unit.Class).ToArray();
        
        if (deck.Camp == Camp.Sheep)
        {
            User.Instance.AllDeckSheep.Add(deck);
        }
        else User.Instance.AllDeckWolf.Add(deck);
    }
    
    public void SaveDeck(DeckInfo deckInfo)
    {
        // Save the user's deck information to the server.
        var deck = new Deck
        {
            DeckId = deckInfo.DeckId,
            UnitsOnDeck = deckInfo.UnitInfo,
            DeckNumber = deckInfo.DeckNumber,
            Camp = (Camp)deckInfo.Camp,
            LastPicked = deckInfo.LastPicked,
        };
        deck.UnitsOnDeck = deck.UnitsOnDeck.OrderBy(unit => unit.Class).ToArray();

        if (deck.Camp == Camp.Sheep)
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
    }
}
