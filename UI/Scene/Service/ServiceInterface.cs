using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;

public interface IUserService
{
    void LoadOwnedUnit(OwnedUnitInfo units);
    void LoadNotOwnedUnit(UnitInfo unitInfo);
    void LoadOwnedSheep(OwnedSheepInfo sheepInfo);
    void LoadNotOwnedSheep(SheepInfo sheepInfo);
    void LoadOwnedEnchant(OwnedEnchantInfo enchantInfo);
    void LoadNotOwnedEnchant(EnchantInfo enchantInfo);
    void LoadOwnedCharacter(OwnedCharacterInfo characterInfo);
    void LoadNotOwnedCharacter(CharacterInfo characterInfo);
    void LoadOwnedMaterial(OwnedMaterialInfo materialInfo);
    void LoadBattleSetting(BattleSettingInfo battleSettingInfo);
    void LoadDeck(DeckInfo deckInfo);
    void SaveDeck(DeckInfo deckInfo);
    void BindDeck();
    event Action<Faction> InitDeckButton;
}

public interface IWebService
{
    Env Environment { get; set; }
    Task<T> SendWebRequestAsync<T>(string url, string method, object obj);
    Task SendWebRequest<T>(string url, string method, object obj, Action<T> responseAction);
}

public interface ITokenService
{
    void SaveAccessToken(string accessToken);
    void SaveRefreshToken(string refreshToken);
    string GetAccessToken();
    string GetRefreshToken();
}

public interface ICraftingService
{
    
}
