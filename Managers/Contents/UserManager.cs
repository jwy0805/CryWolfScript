
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;

public class UserManager
{ 
    public string UserAccount { get; set; } = "temp";
    public string AccessToken => Managers.Token.GetAccessToken();
    public string RefreshToken => Managers.Token.GetRefreshToken();
    public List<UnitInfo> OwnedCardListSheep { get; set; } = new();
    public List<UnitInfo> OwnedCardListWolf { get; set; } = new();
    public List<UnitInfo> NotOwnedCardListSheep { get; set; } = new();
    public List<UnitInfo> NotOwnedCardListWolf { get; set; } = new();
    public List<Deck> AllDeckSheep { get; set; } = new();
    public List<Deck> AllDeckWolf { get; set; }= new();
    public Deck DeckSheep { get; set; } = new();
    public Deck DeckWolf { get; set; } = new();
    
    public class Deck
    {
        public int DeckId;
        public UnitInfo[] UnitsOnDeck;
        public int DeckNumber;
        public Camp Camp;
        public bool LastPicked;
    }
    
    public void LoadOwnedUnit(UnitInfo unitInfo)
    {
        if (unitInfo.Camp == Camp.Sheep) OwnedCardListSheep.Add(unitInfo);
        else OwnedCardListWolf.Add(unitInfo);
    }
    
    public void LoadNotOwnedUnit(UnitInfo unitInfo)
    {
        if (unitInfo.Camp == Camp.Sheep) NotOwnedCardListSheep.Add(unitInfo);
        else NotOwnedCardListWolf.Add(unitInfo);
    }

    public void LoadDeck(DeckInfo deckInfo)
    {   
        // 유저의 덱 정보를 받아서 Manager에 저장
        var deck = new Deck
        {
            DeckId = deckInfo.DeckId,
            UnitsOnDeck = deckInfo.UnitInfo,
            DeckNumber = deckInfo.DeckNumber,
            Camp = (Camp)deckInfo.Camp,
            LastPicked = deckInfo.LastPicked,
        };
        deck.UnitsOnDeck = deck.UnitsOnDeck.OrderBy(unit => unit.Class).ToArray();
        
        if (deck.Camp == Camp.Sheep) AllDeckSheep.Add(deck);
        else AllDeckWolf.Add(deck);
    }

    public void SaveDeck(DeckInfo deckInfo)
    {   
        // 클라이언트 조작 방지용, 메치메이킹, 게임 시작 시 덱 검증을 위해 사용
        var deck = new Deck
        {
            DeckId = deckInfo.DeckId,
            UnitsOnDeck = deckInfo.UnitInfo,
            DeckNumber = deckInfo.DeckNumber,
            Camp = (Camp)deckInfo.Camp,
            LastPicked = deckInfo.LastPicked,
        };
        deck.UnitsOnDeck = deck.UnitsOnDeck.OrderBy(unit => unit.Class).ToArray();

        if (deck.Camp == Camp.Sheep) DeckSheep = deck;
        else DeckWolf = deck;
    }

    public void BindDeck()
    {
        DeckSheep = AllDeckSheep.Any(deck => deck.LastPicked) 
            ? AllDeckSheep.First(deck => deck.LastPicked) 
            : AllDeckSheep.First();
        DeckWolf = AllDeckWolf.Any(deck => deck.LastPicked) 
            ? AllDeckWolf.First(deck => deck.LastPicked) 
            : AllDeckWolf.First();
    }
}