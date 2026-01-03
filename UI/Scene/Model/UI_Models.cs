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
    public UserInfo UserInfo { get; set; } = new();
    public List<OwnedUnitInfo> OwnedUnitList { get; set; } = new();
    public List<UnitInfo> NotOwnedUnitList { get; set; } = new();
    public List<OwnedSheepInfo> OwnedSheepList { get; set; } = new();
    public List<SheepInfo> NotOwnedSheepList { get; set; } = new();
    public List<OwnedEnchantInfo> OwnedEnchantList { get; set; } = new();
    public List<EnchantInfo> NotOwnedEnchantList { get; set; } = new();
    public List<OwnedCharacterInfo> OwnedCharacterList { get; set; } = new();
    public List<CharacterInfo> NotOwnedCharacterList { get; set; } = new();
    public BattleSettingInfo BattleSetting { get; set; } = new();
    public List<Deck> AllDeckSheep { get; set; } = new();
    public List<Deck> AllDeckWolf { get; set; }= new();
    public List<OwnedMaterialInfo> OwnedMaterialList { get; set; } = new();
    public Deck DeckSheep { get; set; } = new();
    public Deck DeckWolf { get; set; } = new();
    public bool SubscribeAdsRemover { get; set; }
    public bool IsGuest { get; set; } = false;
    public bool GuestPopupShown { get; set; } = false;
    public Dictionary<int, int> ExpTable { get; set; } = new();
    public void Clear()
    {
        OwnedUnitList.Clear();
        NotOwnedUnitList.Clear();
        OwnedSheepList.Clear();
        NotOwnedSheepList.Clear();
        OwnedEnchantList.Clear();
        NotOwnedEnchantList.Clear();
        OwnedCharacterList.Clear();
        NotOwnedCharacterList.Clear();
        BattleSetting = new BattleSettingInfo();
        AllDeckSheep.Clear();
        AllDeckWolf.Clear();
        OwnedMaterialList.Clear();
        DeckSheep = new Deck();
        DeckWolf = new Deck();
    }
}

public class Deck
{
    public int DeckId;
    public UnitInfo[] UnitsOnDeck;
    public int DeckNumber;
    public Faction Faction;
    public bool LastPicked;
}
