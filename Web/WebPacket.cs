using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global

public class UnitInfo
{
    public UnitId Id { get; set; }
    public UnitClass Class { get; set; }
    public int Level { get; set; }
    public UnitId Species { get; set; }
    public Role Role { get; set; }
    public Camp Camp { get; set; }
}

public class SheepInfo
{
    public SheepId Id { get; set; }
    public UnitClass Class { get; set; }
}

public class EnchantInfo
{
    public EnchantId Id { get; set; }
    public UnitClass Class { get; set; }
}

public class CharacterInfo
{
    public CharacterId Id { get; set; }
    public UnitClass Class { get; set; }
}

public class DeckInfo
{
    public int DeckId { get; set; }
    public UnitInfo[] UnitInfo { get; set; }
    public int DeckNumber { get; set; }
    public int Camp { get; set; }
    public bool LastPicked { get; set; }
}

public class CreateUserAccountPacketRequired
{
    public string UserAccount { get; set; }
    public string Password { get; set; }
}

public class CreateUserAccountPacketResponse
{
    public bool CreateOk { get; set; }
    public string Message { get; set; }
}

public class LoginUserAccountPacketRequired
{
    public string UserAccount { get; set; }
    public string Password { get; set; }
}

public class LoginUserAccountPacketResponse
{
    public bool LoginOk { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class ChangeActPacketRequired
{
    public string AccessToken { get; set; }
    public UserAct Act { get; set; }
    public Camp Camp { get; set; }
    public int MapId { get; set; }
}

public class ChangeActPacketResponse
{
    public bool ChangeOk { get; set; }
}

public class RefreshTokenRequired
{
    public string RefreshToken { get; set; }
}

public class RefreshTokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class GetOwnedCardsPacketRequired
{
    public string AccessToken { get; set; }
    public Env Environment { get; set; }
}

public class GetOwnedCardsPacketResponse
{
    public bool GetCardsOk { get; set; }
    public List<UnitInfo> OwnedCardList { get; set; }
    public List<UnitInfo> NotOwnedCardList { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class GetOwnedSheepPacketRequired
{
    public string AccessToken { get; set; }
    public Env Environment { get; set; }
}

public class GetOwnedSheepPacketResponse
{
    public bool GetSheepOk { get; set; }
    public List<SheepInfo> OwnedSheepList { get; set; }
    public List<SheepInfo> NotOwnedSheepList { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class GetOwnedEnchantPacketRequired
{
    public string AccessToken { get; set; }
    public Env Environment { get; set; }
}

public class GetOwnedEnchantPacketResponse
{
    public bool GetEnchantOk { get; set; }
    public List<EnchantInfo> OwnedEnchantList { get; set; }
    public List<EnchantInfo> NotOwnedEnchantList { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class GetOwnedCharacterPacketRequired
{
    public string AccessToken { get; set; }
    public Env Environment { get; set; }
}

public class GetOwnedCharacterPacketResponse
{
    public bool GetCharacterOk { get; set; }
    public List<CharacterInfo> OwnedCharacterList { get; set; }
    public List<CharacterInfo> NotOwnedCharacterList { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class GetInitDeckPacketRequired
{
    public string AccessToken { get; set; }
    public Env Environment { get; set; }
}

public class GetInitDeckPacketResponse
{
    public bool GetDeckOk { get; set; }
    public List<DeckInfo> DeckList { get; set; }
}

public class GetSelectedDeckRequired
{
    public string AccessToken { get; set; }
    public int Camp { get; set; }
    public int DeckNumber { get; set; }
}

public class GetSelectedDeckResponse
{
    public bool GetSelectedDeckOk { get; set; }
    public DeckInfo Deck { get; set; }
}

public class UpdateDeckPacketRequired
{
    public string AccessToken { get; set; }
    public int DeckId { get; set; }
    public UnitId UnitIdToBeDeleted { get; set; }
    public UnitId UnitIdToBeUpdated { get; set; }
}

public class UpdateDeckPacketResponse
{
    public int UpdateDeckOk { get; set; }
}

public class UpdateLastDeckPacketRequired
{
    public string AccessToken { get; set; }
    public Dictionary<int, bool> LastPickedInfo { get; set; }
}

public class UpdateLastDeckPacketResponse
{
    public bool UpdateLastDeckOk { get; set; }
}