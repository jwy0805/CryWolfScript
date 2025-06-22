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
    Task LoadTestUserInfo(int userId);
    event Action<Faction> InitDeckButton;
    // bool TutorialSheepEnded { get; set; }
    // bool TutorialWolfEnded { get; set; }
    UserInfo UserInfo { get; set; }
    UserTutorialInfo TutorialInfo { get; set; }
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
    void ClearTokens();
}

public interface ISignalRClient
{
    [CanBeNull] Action OnInvitationSent { get; set; }
    [CanBeNull] Action<AcceptInvitationPacketResponse> OnInvitationSuccess { get; set; }
    [CanBeNull] Action<FriendRequestPacketResponse> OnFriendRequestNotificationReceived { get; set; }
    Task Connect(string username);
    Task JoinLobby();
    Task LeaveLobby();
    Task<InviteFriendlyMatchPacketRequired> SendInvitation(InviteFriendlyMatchPacketRequired required);
    Task<AcceptInvitationPacketResponse> SendAcceptInvitation(AcceptInvitationPacketRequired required);
    Task<FriendRequestPacketResponse> SendFriendRequest(FriendRequestPacketRequired required);
    Task Disconnect();
}

public interface IPaymentService
{
    void Init();
    void BuyCashProduct(string productId);
    Task BuyProductAsync(string productId);
    Task BuyDailyProductAsync(string productId);
    void RestorePurchases();
    event Action OnCashPaymentSuccess;
    event Action OnPaymentSuccess;
    event Func<int, Task> OnDailyPaymentSuccess;
}