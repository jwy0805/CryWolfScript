using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;

public interface IUserService
{
    void LoadOwnedUnit(List<OwnedUnitInfo> units);
    void LoadNotOwnedUnit(List<UnitInfo> units);
    void LoadOwnedSheep(List<OwnedSheepInfo> sheepInfo);
    void LoadNotOwnedSheep(List<SheepInfo> sheepInfo);
    void LoadOwnedEnchant(List<OwnedEnchantInfo> enchantInfo);
    void LoadNotOwnedEnchant(List<EnchantInfo> enchantInfo);
    void LoadOwnedCharacter(List<OwnedCharacterInfo> characterInfo);
    void LoadNotOwnedCharacter(List<CharacterInfo> characterInfo);
    void LoadOwnedMaterial(List<OwnedMaterialInfo> materialInfo);
    void LoadBattleSetting(BattleSettingInfo battleSettingInfo);
    void LoadDeck(DeckInfo deckInfo);
    void SaveDeck(DeckInfo deckInfo);
    Task LoadUserInfo();
    void BindDeck();
    Task LoadTestUser(int userId);
    event Action<Faction> InitDeckButton;
    UserInfo UserInfo { get; set; }
}

public interface IWebService
{ 
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

public interface ISignalRClient
{
    [CanBeNull] Action<FriendRequestPacketResponse> OnFriendRequestNotificationReceived { get; set; }
    Task Connect();
    Task JoinLobby(string username);
    Task LeaveLobby();
    Task<FriendRequestPacketResponse> SendFriendRequest(FriendRequestPacketRequired required);
    Task Disconnect();
}

public interface IPaymentService
{
    void Init();
    void BuyCashProduct(string productId);
    void BuyProduct(string productId);
    event Action OnCashPaymentSuccess;
    event Action OnPaymentSuccess;
}