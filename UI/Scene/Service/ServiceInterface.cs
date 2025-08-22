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
    [CanBeNull] event Action OnInvitationSent;
    [CanBeNull] event Func<DeckInfo, Task> OnEnemyDeckSwitched;
    [CanBeNull] event Func<DeckInfo, DeckInfo, Task> OnFactionSwitched;
    [CanBeNull] event Action<AcceptInvitationPacketResponse> OnInvitationSuccess;
    [CanBeNull] event Action<AcceptInvitationPacketResponse> OnEnterFriendlyMatch;
    [CanBeNull] event Action<FriendRequestPacketResponse> OnFriendRequestNotificationReceived;
    [CanBeNull] event Action OnGuestLeft;
    [CanBeNull] event Func<Task> OnStartFriendlyMatch;
    Task Connect(string username);
    Task JoinLobby(string token);
    Task LeaveLobby();
    Task JoinGame(string token);
    Task LeaveGame();
    Task SwitchFactionOnFriendlyMatch(Faction faction);
    Task SwitchDeckOnFriendlyMatch(string token, Faction faction);
    Task<LoadInvitableFriendPacketResponse> LoadFriends(LoadInvitableFriendPacketRequired required);
    Task<InviteFriendlyMatchPacketResponse> SendInvitation(InviteFriendlyMatchPacketRequired required);
    Task<AcceptInvitationPacketResponse> SendAcceptInvitation(AcceptInvitationPacketRequired required);
    Task<FriendRequestPacketResponse> SendFriendRequest(FriendRequestPacketRequired required);
    Task StartFriendlyMatch(string username);
    Task SendSessionId(int sessionId);
    Task<Tuple<bool, AcceptInvitationPacketResponse>> ReEntryFriendlyMatch(string username);
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