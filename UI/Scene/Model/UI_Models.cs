using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;

public interface IAsset
{
    public int Id { get; set; }
    public UnitClass Class { get; set; }
}

public class User
{
    public static User Instance { get; } = new();
    
    public string UserAccount { get; set; } = "temp";
    public List<UnitInfo> OwnedCardListSheep { get; set; } = new();
    public List<UnitInfo> OwnedCardListWolf { get; set; } = new();
    public List<UnitInfo> NotOwnedCardListSheep { get; set; } = new();
    public List<UnitInfo> NotOwnedCardListWolf { get; set; } = new();
    public List<SheepInfo> OwnedSheepList { get; set; } = new();
    public List<SheepInfo> NotOwnedSheepList { get; set; } = new();
    public List<EnchantInfo> OwnedEnchantList { get; set; } = new();
    public List<EnchantInfo> NotOwnedEnchantList { get; set; } = new();
    public List<CharacterInfo> OwnedCharacterList { get; set; } = new();
    public List<CharacterInfo> NotOwnedCharacterList { get; set; } = new();
    public BattleSettingInfo BattleSetting { get; set; } = new();
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
