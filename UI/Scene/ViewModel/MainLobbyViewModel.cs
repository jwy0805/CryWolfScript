using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using Zenject;
// ReSharper disable ClassNeverInstantiated.Global

public class MainLobbyViewModel : IDisposable
{
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    private readonly ISignalRClient _signalRClient;

    // 각 페이지 사이의 거리
    private float _valueDistance;                        
    private int _maxPage;
    private Tuple<float, int> _startTouchX;
    private Tuple<float, int> _endTouchX;
    // 페이지 스와이프를 위해 움직여야 하는 최소 거리
    private readonly float _swipeDistance = 150f;           
    private int _currentPage;

    public event Action OnFriendRequestNotificationReceived;
    public event Action OnFriendRequestNotificationOff;
    public event Action OnMailAlert;
    public event Action OffMailAlert;
    public event Action OnUpdateFriendList;
    public event Action<int> OnPageChanged;
    public event Action<int> ChangeButtonFocus;
    
    public float[] ScrollPageValues { get; private set; }
    public bool IsSwipeMode { get; set; } = false;
    public float SwipeTime => 0.2f;
    
    public int CurrentPage 
    { 
        get => _currentPage;
        private set
        {
            _currentPage = value;
            ChangeButtonFocus?.Invoke(value);
        }
    }

    [Inject]
    public MainLobbyViewModel(IWebService webService, ITokenService tokenService, ISignalRClient signalRClient)
    {
        _webService = webService;
        _tokenService = tokenService;
        _signalRClient = signalRClient;
        _signalRClient.Connect();
        
        BindEvents();
    }
    
    private void BindEvents()
    {
        _signalRClient.OnFriendRequestNotificationReceived -= FriendRequestNotificationReceived;
        _signalRClient.OnFriendRequestNotificationReceived += FriendRequestNotificationReceived;
    }

    public async Task InitFriendAlert()
    {
        var friendTask = await LoadPendingFriends();
        if (friendTask.Count > 0)
        {
            OnFriendRequestNotificationReceived?.Invoke();
        }
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
    
    public void OnPlayButtonClicked(UI_MainLobby.GameModeEnums mode)
    {
        switch (mode)
        {
            case UI_MainLobby.GameModeEnums.FriendlyMatch:
                break;
            case UI_MainLobby.GameModeEnums.RankGame:
                LoadMatchMakingScene();
                break;
            case UI_MainLobby.GameModeEnums.SinglePlay:
                break;
        }
    }
    
    private void LoadMatchMakingScene()
    {
        Managers.Scene.LoadScene(Define.Scene.MatchMaking);
    }
    
    public async Task<List<UserInfo>> GetFriendList()
    {
        var packet = new FriendListPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken()
        };
        var task = _webService
            .SendWebRequestAsync<FriendListPacketResponse>("Relation/GetFriendList", "POST", packet);

        await task;

        return task.Result.FriendList;
    }
    
    public async Task<List<FriendUserInfo>> LoadPendingFriends()
    {
        var packet = new LoadPendingFriendPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken()
        };
        var task = _webService
            .SendWebRequestAsync<LoadPendingFriendPacketResponse>("Relation/LoadPendingFriends", "POST", packet);

        await task;

        return task.Result.PendingFriendList;
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
        var task = _webService
            .SendWebRequestAsync<SearchUsernamePacketResponse>("Relation/SearchUsername", "POST", packet);

        await task;

        return task.Result.FriendUserInfos;
    }

    public async Task<FriendRequestPacketResponse> SendFriendRequest(FriendUserInfo friendInfo)
    {
        var packet = new FriendRequestPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            FriendUsername = friendInfo.UserName,
            CurrentFriendStatus = friendInfo.FriendStatus
        };
        
        return await _signalRClient.SendFriendRequest(packet);
    }

    public async Task AcceptFriend(string username, bool accept)
    {
        var packet = new AcceptFriendPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            FriendUsername = username,
            Accept = accept
        };

        await _webService.SendWebRequestAsync<AcceptFriendPacketResponse>("Relation/AcceptFriend", "PUT", packet);
    }
    
    public async Task<Tuple<FriendRequestPacketResponse, string>> DeleteFriend(FriendUserInfo friendInfo, bool isBlock = false)
    {
        var packet = new FriendRequestPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            FriendUsername = friendInfo.UserName,
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
    
    public async Task JoinLobby(string username)
    {
        await _signalRClient.JoinLobby(username);
    }
    
    public async Task LeaveLobby()
    {
        await _signalRClient.LeaveLobby();
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
        if (_startTouchX.Item2 != _endTouchX.Item2) return;
        if (Math.Abs(_startTouchX.Item1 - _endTouchX.Item1) < _swipeDistance)
        {
            OnPageChanged?.Invoke(CurrentPage);
            return;
        }
        
        bool isLeft = _startTouchX.Item1 < _endTouchX.Item1;
        if (isLeft)
        {
            if (CurrentPage == 0) return;
            CurrentPage--;
        }
        else
        {
            if (CurrentPage == _maxPage - 1) return;
            CurrentPage++;
        }
        
        OnPageChanged?.Invoke(CurrentPage);
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
        _signalRClient.OnFriendRequestNotificationReceived -= FriendRequestNotificationReceived;
    }
}
