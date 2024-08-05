using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class User
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
}

public class Deck
{
    public int DeckId;
    public UnitInfo[] UnitsOnDeck;
    public int DeckNumber;
    public Camp Camp;
    public bool LastPicked;
}
