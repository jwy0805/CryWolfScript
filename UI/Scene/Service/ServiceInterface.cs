using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IUserService
{
    void LoadOwnedUnit(List<OwnedUnitInfo> units);
    void LoadOwnedSheep(List<OwnedSheepInfo> sheepInfo);
    void LoadOwnedEnchant(List<OwnedEnchantInfo> enchantInfo);
    void LoadOwnedCharacter(List<OwnedCharacterInfo> characterInfo);
    void LoadOwnedMaterial(List<OwnedMaterialInfo> materialInfo);
    void LoadBattleSetting(BattleSettingInfo battleSettingInfo);
    void LoadDeck(DeckInfo deckInfo);
    void SaveDeck(DeckInfo deckInfo);
    Task LoadUserInfo();
    void BindDeck();
    Task LoadTestUserInfo(int userId);
    event Action<Faction> InitDeckButton;
    User User { get; }
    UserTutorialInfo TutorialInfo { get; set; }
}

public interface IWebService
{ 
    Task<T> SendWebRequestAsync<T>(string url, string method, object obj);
    void SendWebRequest(string url, string method, object obj);
}

public interface ITokenService
{
    void SaveAccessToken(string accessToken);
    void SaveRefreshToken(string refreshToken);
    string GetAccessToken();
    string GetRefreshToken();
    void ClearTokens();
}

public interface ISecretService
{
    /// <summary>
    /// 지정한 키로 값을 저장하는 함수
    /// </summary>
    /// <param name="key">키</param>
    /// <param name="value">값</param>
    /// <returns>저장에 성공했는지 여부</returns>
    bool Put(string key, string value); 
    
    /// <summary>
    /// 지정한 키의 값을 가져오는 함수
    /// </summary>
    /// <param name="key">키</param>
    /// <returns>지정한 키로 설정된 값, 없으면 null</returns>
    string Get(string key);
    
    /// <summary>
    /// 지정한 키의 값을 삭제하는 함수
    /// </summary>
    /// <param name="key">키</param>
    /// <returns>삭제에 성공했는지 여부</returns>
    bool Delete(string key);
}

public interface ISignalRClient
{
    [CanBeNull] event Action OnInvitationSent;
    [CanBeNull] event Func<DeckInfo, Task> OnEnemyDeckSwitched;
    [CanBeNull] event Func<DeckInfo, DeckInfo, bool, Task> OnFactionSwitched;
    [CanBeNull] event Action<AcceptInvitationPacketResponse> OnInvitationSuccess;
    [CanBeNull] event Action<AcceptInvitationPacketResponse> OnEnterFriendlyMatch;
    [CanBeNull] event Action<FriendRequestPacketResponse> OnFriendRequestNotificationReceived;
    [CanBeNull] event Action OnGuestLeft;
    [CanBeNull] event Func<Task> OnStartFriendlyMatch;
    Task Connect(string userTag);
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
    Task StartFriendlyMatch(string userTag);
    Task SendSessionId(int sessionId);
    Task<Tuple<bool, AcceptInvitationPacketResponse>> ReEntryFriendlyMatch(string userTag);
    Task Disconnect();
}

public interface IPaymentService
{
    bool IsInitialized { get; }
    void Init();
    void BuyCashProduct(string productId);
    Task BuyProductAsync(string productId);
    Task BuyDailyProductAsync(string productId);
    void RestorePurchases();
    string GetLocalizedPrice(string productCode);
    event Action OnIapReady;
    event Func<Task> OnCashPaymentSuccess;
    event Func<Task> OnPaymentSuccess;
    event Func<int, Task> OnDailyPaymentSuccess;
}

public interface ITutorialHelper
{
    Task RunTutorialTag(string tag);
}

public interface ICardFactory
{
    Task<GameObject> GetCardResourcesF<TEnum>(
        IAsset asset, 
        Transform parent, 
        Func<PointerEventData, Task> action = null, 
        bool activateText = false) where TEnum : struct, Enum;
    Task<GameObject> GetCardResources<TEnum>(
        IAsset asset, 
        Transform parent, 
        Action<PointerEventData> action = null, 
        bool activateText = false) where TEnum : struct, Enum;
    Task<GameObject> GetMaterialResources(
        IAsset asset, 
        Transform parent, 
        Action<PointerEventData> action = null);
    Task<GameObject> GetItemFrameGold(int count, Transform parent);
    Task<GameObject> GetItemFrameSpinel(int count, Transform parent);
    string GetGoldPrefabPath(int count);
    string GetSpinelPrefabPath(int count);
}

public interface IUIFactory
{
    Image GetFrameFromCardButton(ISkillButton button);
    Task<GameObject> GetFriendFrame(FriendUserInfo friendInfo, Transform parent, Action<PointerEventData> action = null);
    Task<GameObject> GetFriendInviteFrame(FriendUserInfo friendInfo, Transform parent, Action<PointerEventData> action = null);
    Task<GameObject> GetFriendRequestFrame(FriendUserInfo friendInfo, Transform parent, Action<PointerEventData> action = null);
    Task<GameObject> GetProductMailFrame(MailInfo mailInfo, Transform parent);
    Task<GameObject> GetNoticeFrame(NoticeInfo noticeInfo, Transform parent);
    RenderTexture CreateRenderTexture(string textureName);
}