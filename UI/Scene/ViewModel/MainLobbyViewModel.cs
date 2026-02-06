using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;
// ReSharper disable ClassNeverInstantiated.Global

public class MainLobbyViewModel : IDisposable
{
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    private readonly ISignalRClient _signalRClient;
    
    // Caching
    private List<NoticeInfo> _noticeCache = new();
    private List<EventInfo> _eventCache = new();

    // 각 페이지 사이의 거리
    private float _valueDistance;                        
    private int _maxPage;
    private Tuple<float, int> _startTouchX;
    private Tuple<float, int> _endTouchX;
    // 페이지 스와이프를 위해 움직여야 하는 최소 거리
    private readonly float _swipeDistance = Screen.width * 0.45f;           
    private int _currentPage;

    public event Func<Task> OnInitUserInfo;
    public event Action OnFriendRequestNotificationReceived;
    public event Action OnFriendRequestNotificationOff;
    public event Action OnMailAlert;
    public event Action OffMailAlert;
    public event Func<Task> OnResetMailUI;
    public event Action OnUpdateFriendList;
    public event Action OnUpdateUsername;
    public event Func<int, Task> OnPageChanged;
    public event Action<int> OnChangeButtonFocus;
    public event Func<Task> OnChangeLanguage;
    
    public bool ChildScrolling { get; set; }
    public float[] ScrollPageValues { get; private set; }
    public bool IsSwipeMode { get; set; }
    public float SwipeTime => 0.2f;
    
    public int CurrentPage 
    { 
        get => _currentPage;
        private set
        {
            _currentPage = value;
            OnChangeButtonFocus?.Invoke(value);
        }
    }

    [Inject]
    public MainLobbyViewModel(
        IUserService userService, 
        IWebService webService, 
        ITokenService tokenService, 
        ISignalRClient signalRClient)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
        _signalRClient = signalRClient;
        
        BindEvents();
    }
    
    private void BindEvents()
    {
        _signalRClient.OnInvitationSent += RefreshMailAlert;
        _signalRClient.OnFriendRequestNotificationReceived += FriendRequestNotificationReceived;
        _signalRClient.OnEnterFriendlyMatch += response =>
        {
            Managers.Scene.LoadScene(Define.Scene.FriendlyMatch);
            Managers.Game.FriendlyMatchResponse = response;
        };
    }

    public async Task InitUserInfo()
    {
        await Util.InvokeAll(OnInitUserInfo);
    }
    
    public async Task ConnectSignalR(string username)
    {
        await _signalRClient.Connect(username);
        Debug.Log($"Connected to SignalR as {username}");
    }
    
    public async Task InitFriendAlert()
    {
        var friendTuple = await LoadPendingFriends();
        if (friendTuple.Item2.Count > 0)
        {
            Debug.Log($"Friend list loaded: {string.Join(", ", friendTuple.Item1)}");
            OnFriendRequestNotificationReceived?.Invoke();
        }
    }
    
    public async Task ClaimMail(MailInfo mailInfo)
    {
        var packet = new ClaimMailPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            MailId = mailInfo.MailId
        };
        
        await _webService.SendWebRequestAsync<ClaimMailPacketResponse>(
            "Mail/ClaimMail", UnityWebRequest.kHttpVerbPUT, packet);
        OnResetMailUI?.Invoke();
        await InitMailAlert();
    }
    
    public async Task ClaimProductFromMailbox(MailInfo mailInfo = null)
    {
        var packet = new ContinueClaimPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            MailId = mailInfo?.MailId ?? 0
        };
        
        var res = await _webService.SendWebRequestAsync<ClaimProductPacketResponse>(
            "Payment/StartClaim", UnityWebRequest.kHttpVerbPUT, packet);

        OnResetMailUI?.Invoke();
        await HandleClaimPacketResponse(res);
        await InitMailAlert();
    }
    
    public async Task SelectProduct(CompositionInfo composition)
    {
        var packet = new SelectProductPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            SelectedCompositionInfo = composition,
        };
        
        var res = await _webService.SendWebRequestAsync<ClaimProductPacketResponse>(
            "Payment/SelectProduct", UnityWebRequest.kHttpVerbPUT, packet);

        await HandleClaimPacketResponse(res);
    }

    public async Task OpenProduct(ProductInfo productInfo, bool openAll)
    {
        var packet = new OpenProductPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            ProductId = productInfo.ProductId,
            OpenAll = openAll,
        };
        
        var res = await _webService.SendWebRequestAsync<ClaimProductPacketResponse>(
            "Payment/OpenProduct", UnityWebRequest.kHttpVerbPUT, packet);
        
        await HandleClaimPacketResponse(res);
    }
    
    public async Task ContinueClaim()
    {
        var packet = new ContinueClaimPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
        };
        
        var res = await _webService.SendWebRequestAsync<ClaimProductPacketResponse>(
            "Payment/ContinueClaim", UnityWebRequest.kHttpVerbPUT, packet);
        
        await HandleClaimPacketResponse(res);
    }

    private async Task HandleClaimPacketResponse(ClaimProductPacketResponse res)
    {
        if (!res.ClaimOk) return;

        Managers.UI.CloseAllPopupUI();

        switch (res.RewardPopupType)
        {
            case RewardPopupType.None:
                break;

            case RewardPopupType.Select:
            {
                if (res.ProductInfos == null || res.ProductInfos.Count == 0) return;
                var selectPopup = await Managers.UI.ShowPopupUI<UI_RewardSelectPopup>();
                selectPopup.ProductInfo = res.ProductInfos[0];
                selectPopup.CompositionInfos = res.CompositionInfos ?? new List<CompositionInfo>();
                break;
            }

            case RewardPopupType.Open:
            {
                // "열기 선택 화면" (open one / open all)
                var openPopup = await Managers.UI.ShowPopupUI<UI_RewardOpenPopup>();
                openPopup.RandomProductInfos = res.RandomProductInfos; // 이 프로퍼티 추가/연결 필요
                break;
            }

            case RewardPopupType.OpenResult:
            {
                // "오픈 결과 화면"
                var openResultPopup = await Managers.UI.ShowPopupUI<UI_RewardOpenResultPopup>();
                openResultPopup.OriginalProductInfos = res.RandomProductInfos;
                openResultPopup.CompositionInfos = res.CompositionInfos;
                break;
            }

            case RewardPopupType.Item:
            {
                var itemPopup = await Managers.UI.ShowPopupUI<UI_RewardItemPopup>();
                itemPopup.CompositionInfos = res.CompositionInfos;
                break;
            }

            case RewardPopupType.Subscription:
                // (선택) 구독 하이라이트 팝업
                break;
        }
    }
    
    public async Task AcceptInvitation(MailInfo mailInfo, bool accept)
    {
        var packet = new AcceptInvitationPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Accept = accept,
            MailId = mailInfo.MailId
        };
        
        await Task.WhenAll(ClaimMail(mailInfo), _signalRClient.SendAcceptInvitation(packet));
    }
    
    public async Task DeleteReadMail()
    {
        var packet = new DeleteReadMailPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
        };
        
        await _webService.SendWebRequestAsync<DeleteReadMailPacketResponse>(
            "Mail/DeleteReadMail", UnityWebRequest.kHttpVerbDELETE, packet);
        
        OnResetMailUI?.Invoke();
        await InitMailAlert();
    }
    
    public async Task InitMailAlert()
    {
        var mailTask = await GetMailList();
        if (mailTask.Any(mail => mail.Claimed == false))
        {
            OnMailAlert?.Invoke();
        }
        else
        {
            OffMailAlert?.Invoke();
        }
    }

    public void RefreshMailAlert()
    {
        OnMailAlert?.Invoke();
    }
    
    public void OnPlayButtonClicked(Define.GameMode mode)
    {
        switch (mode)
        {
            case Define.GameMode.FriendlyMatch:
                Managers.Scene.LoadScene(Define.Scene.FriendlyMatch);
                Managers.Network.IsFriendlyMatchHost = true;
                break;
            
            case Define.GameMode.RankGame:
                Managers.Scene.LoadScene(Define.Scene.MatchMaking);
                break;
            
            case Define.GameMode.SinglePlay:
                Managers.Scene.LoadScene(Define.Scene.SinglePlay);
                break;
        }
    }
    
    public async Task<List<FriendUserInfo>> GetFriendList()
    {
        var packet = new FriendListPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken()
        };
        
        var task = await _webService
            .SendWebRequestAsync<FriendListPacketResponse>("Relation/GetFriendList", "POST", packet);
        
        return task.FriendList.OrderBy(friendInfo => GetFriendOrderPriority(friendInfo.Act)).ToList();
    }
    
    public async Task<Tuple<List<FriendUserInfo>, List<FriendUserInfo>>> LoadPendingFriends()
    {
        var packet = new LoadPendingFriendPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken()
        };
        
        var task = await _webService
            .SendWebRequestAsync<LoadPendingFriendPacketResponse>("Relation/LoadPendingFriends", "POST", packet);
        
        return new Tuple<List<FriendUserInfo>, List<FriendUserInfo>>(task.PendingFriendList, task.SendingFriendList);
    }

    public async Task<List<MailInfo>> GetMailList()
    {
        var packet = new LoadPendingMailPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken()
        };
        var task = _webService
            .SendWebRequestAsync<LoadPendingMailPacketResponse>("Mail/GetMail", "POST", packet);

        await task;

        return task.Result.PendingMailList;
    }
    
    public async Task<List<FriendUserInfo>> SearchUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return new List<FriendUserInfo>();
        }
        
        var packet = new SearchUsernamePacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Username = username
        };
        var task = await _webService
            .SendWebRequestAsync<SearchUsernamePacketResponse>("Relation/SearchUsername", "POST", packet);
        
        return task.FriendUserInfos;
    }

    public async Task<FriendRequestPacketResponse> SendFriendRequest(FriendUserInfo friendInfo)
    {
        var packet = new FriendRequestPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            FriendUserTag = friendInfo.UserTag,
            CurrentFriendStatus = friendInfo.FriendStatus
        };
        
        return await _signalRClient.SendFriendRequest(packet);
    }

    public async Task AcceptFriend(string username, bool accept)
    {
        var packet = new AcceptFriendPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            FriendUserTag = username,
            Accept = accept
        };

        await _webService.SendWebRequestAsync<AcceptFriendPacketResponse>("Relation/AcceptFriend", "PUT", packet);
    }
    
    public async Task<Tuple<FriendRequestPacketResponse, string>> DeleteFriend(FriendUserInfo friendInfo, bool isBlock = false)
    {
        var packet = new FriendRequestPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            FriendUserTag = friendInfo.UserTag,
            CurrentFriendStatus = isBlock ? FriendStatus.Blocked : FriendStatus.None
        };
        var task =  _webService
            .SendWebRequestAsync<FriendRequestPacketResponse>("Relation/DeleteFriend", "PUT", packet);

        await task;
        
        return new Tuple<FriendRequestPacketResponse, string>(task.Result, friendInfo.UserName);
    }
    
    private void FriendRequestNotificationReceived(FriendRequestPacketResponse response)
    {
        OnFriendRequestNotificationReceived?.Invoke();
        Debug.Log($"Friend request notification received.");
    }
    
    public void OffFriendRequestNotification()
    {
        OnFriendRequestNotificationOff?.Invoke();
    }

    public void UpdateFriendList()
    {
        OnUpdateFriendList?.Invoke();
    }

    public void UpdateUsername(string username)
    {
        _userService.User.UserInfo.UserName = username;
        OnUpdateUsername?.Invoke();
    }
    
    private int GetFriendOrderPriority(UserAct act)
    {
        switch (act)
        {
            case UserAct.InLobby:
                return 0;
            case UserAct.InCustomGame:
            case UserAct.InMultiGame:
            case UserAct.InRankGame:
            case UserAct.InSingleGame:
            case UserAct.InTutorial:
            case UserAct.MatchMaking:
                return 1;
            case UserAct.Offline:
            case UserAct.Pending:
            default:
                return 2;
        }
    }
    
    public async Task JoinLobby()
    {
        await _signalRClient.JoinLobby(_tokenService.GetAccessToken());
    }
    
    public async Task LeaveLobby()
    {
        await _signalRClient.LeaveLobby();
    }

    public async Task<List<NoticeInfo>> GetNoticeList()
    {
        var packet = new GetNoticeRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            LanguageCode = Managers.Localization.Language2Letter
        };

        var res = await _webService.SendWebRequestAsync<GetNoticeResponse>(
            "Event/GetNotice", UnityWebRequest.kHttpVerbPOST, packet);

        if (res == null || res.GetNoticeOk == false || res.NoticeInfos == null)
        {
            Debug.LogWarning("GetNoticeList failed or empty.");
            _noticeCache = new List<NoticeInfo>();
            return new List<NoticeInfo>(_noticeCache);
        }

        _noticeCache = res.NoticeInfos
            .Where(n => n != null) // 방어
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.NoticeId)
            .ToList();

        return new List<NoticeInfo>(_noticeCache);
    }

    public async Task<List<EventInfo>> GetEventList()
    {
        var packet = new GetEventRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            LanguageCode = Managers.Localization.Language2Letter
        };

        var res = await _webService.SendWebRequestAsync<GetEventResponse>(
            "Event/GetEvent", UnityWebRequest.kHttpVerbPOST, packet);

        if (res == null || res.GetEventOk == false || res.EventInfos == null)
        {
            Debug.LogWarning("GetEventList failed or empty.");
            _eventCache = new List<EventInfo>();
            return new List<EventInfo>(_eventCache);
        }

        // EventInfo가 정렬키를 직접 가짐
        // 우선순위: Priority -> IsPinned -> StartAtUtc -> EventId
        _eventCache = res.EventInfos
            .Where(e => e != null)
            .OrderByDescending(e => e.Priority)
            .ThenByDescending(e => e.IsPinned)
            .ThenByDescending(e => e.StartAtUtc ?? DateTime.MinValue)
            .ThenByDescending(e => e.EventId)
            .ToList();

        return new List<EventInfo>(_eventCache);
    }
    
    public async Task<GetEventProgressResponse> GetEventProgress(int eventId)
    {
        var packet = new GetEventProgressRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            LanguageCode = Managers.Localization.Language2Letter,
            EventId = eventId
        };

        var res = await _webService.SendWebRequestAsync<GetEventProgressResponse>(
            "Event/GetEventProgress", UnityWebRequest.kHttpVerbPOST, packet);

        return res;
    }

    public async Task<ClaimEventRewardResponse> ClaimEventReward(int eventId, int tier)
    {
        var packet = new ClaimEventRewardRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            EventId = eventId,
            Tier = tier
        };
        
        var res = await _webService.SendWebRequestAsync<ClaimEventRewardResponse>(
            "Event/ClaimEventReward", UnityWebRequest.kHttpVerbPOST, packet);

        return res;
    }
    
    public async Task ChangeLanguage()
    {
        await Util.InvokeAll(OnChangeLanguage);
    }
    
    #region MainLobbyScrollView

    // Logics related to the main lobby scroll view
    public void Initialize(int pageCount)
    {
        ScrollPageValues = new float[pageCount];   // 스크롤되는 페이지의 각 value 값을 저장하는 배열 메모리 할당
        _valueDistance = 1f / (ScrollPageValues.Length - 1); // 스크롤되는 페이지 사이의 거리
        
        for (int i = 0; i < ScrollPageValues.Length; i++)   // 스크롤되는 페이지의 각 value 위치 설정 [0 <= value <= 1]
        {
            ScrollPageValues[i] = i * _valueDistance;
        }

        _maxPage = pageCount;           
    }
    
    public void StartTouch(float startX)
    {
        _startTouchX = new Tuple<float, int>(startX, Managers.UI.PopupList.Count);
    }

    public void EndTouch(float endX)
    {
        _endTouchX = new Tuple<float, int>(endX, Managers.UI.PopupList.Count);

        if (IsSwipeMode || Managers.UI.PopupList.Count > 0) return;
        // 터치 시작/끝 시점에 팝업 개수가 달라졌으면(다른 UI 개입) 무시
        if (_startTouchX.Item2 != _endTouchX.Item2) return;

        var delta = _startTouchX.Item1 - _endTouchX.Item1;

        // 스와이프 길이가 짧으면 "페이지 유지"만
        if (Math.Abs(delta) < _swipeDistance) return;

        bool isLeft = _startTouchX.Item1 < _endTouchX.Item1;
        var targetPage = CurrentPage;

        if (isLeft)
        {
            if (CurrentPage > 0)
                targetPage = CurrentPage - 1;
        }
        else
        {
            if (CurrentPage < _maxPage - 1)
                targetPage = CurrentPage + 1;
        }

        SetCurrentPage(targetPage);
    }
    
    public float GetScrollPageValue(int index)
    {
        return ScrollPageValues[index];
    }
    
    public void SetCurrentPage(int index)
    {
        CurrentPage = index;
        OnPageChanged?.Invoke(index);
    }

    #endregion
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private void Dispose(bool disposing)
    {
        if (!disposing) return;
        _signalRClient.OnInvitationSent -= RefreshMailAlert;
        _signalRClient.OnFriendRequestNotificationReceived -= FriendRequestNotificationReceived;
    }
}
