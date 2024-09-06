using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;

public interface IUserService
{
    void LoadOwnedUnit(UnitInfo unitInfo);
    void LoadNotOwnedUnit(UnitInfo unitInfo);
    void LoadDeck(DeckInfo deckInfo);
    void SaveDeck(DeckInfo deckInfo);
    void BindDeck();
    event Action<Camp> InitDeckButton;
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
