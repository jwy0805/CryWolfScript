using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global

public class CreateUserAccountPacketRequired
{
    public string UserAccount;
    public string Password;
}

public class CreateUserAccountPacketResponse
{
    public bool CreateOk;
    public string UserAccount;
}

public class CreateInitDeckPacketRequired
{
    public string UserAccount;
}

public class CreateInitDeckPacketResponse
{
    public bool CreateDeckOk;
}

public class LoginUserAccountPacketRequired
{
    public string UserAccount;
    public string Password;
}

public class ServerInfo
{
    public string Name;
    public string IP;
    public int CrowdedLevel;
}

public class LoginUserAccountPacketResponse
{
    public bool LoginOk;
}

public class GetOwnedCardsPacketRequired
{
    public string UserAccount;
}

public class UnitInfo
{
    public UnitId Id;
    public UnitClass Class;
    public int Level;
    public UnitId Species;
    public Role Role;
    public Camp Camp;
}

public class GetOwnedCardsPacketResponse
{
    public bool GetCardsOk;
    public List<UnitInfo> OwnedCardList;
    public List<UnitInfo> NotOwnedCardList;
}

public class GetInitDeckPacketRequired
{
    public string UserAccount;
}

public class DeckInfo
{
    public int DeckId;
    public UnitInfo[] UnitInfo;
    public int DeckNumber;
    public int Camp;
    public bool LastPicked;
}

public class GetInitDeckPacketResponse
{
    public bool GetDeckOk;
    public List<DeckInfo> DeckList;
}

public class UpdateDeckPacketRequired
{
    public string UserAccount;
    public int DeckId;
    public UnitId UnitIdToBeDeleted;
    public UnitId UnitIdToBeUpdated;
}

public class UpdateDeckPacketResponse
{
    public int UpdateDeckOk;
}

public class UpdateLastDeckPacketRequired
{
    public string UserAccount;
    public Dictionary<int, bool> LastPickedInfo;
}

public class UpdateLastDeckPacketResponse
{
    public bool UpdateLastDeckOk;
}