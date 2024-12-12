using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_FriendsAddPopup : UI_Popup
{
    private ITokenService _tokenService;
    private MainLobbyViewModel _lobbyVm;
    
    private List<FriendUserInfo> _pendingFriends;

    private enum Images
    {
        FriendsRequestPanel,
        SearchResultContent,
    }
    
    private enum Buttons
    {
        BackButton,
        SearchButton,
        ExitButton
    }

    private enum TextInputs
    {
        UsernameInput
    }
    
    [Inject]
    public void Construct(ITokenService tokenService, MainLobbyViewModel lobbyViewModel)
    {
        _tokenService = tokenService;
        _lobbyVm = lobbyViewModel;
    }

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
    }

    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_InputField>(typeof(TextInputs));
        Bind<Image>(typeof(Images));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopupUI);
        GetButton((int)Buttons.BackButton).gameObject.BindEvent(OnBackClicked);
        GetButton((int)Buttons.SearchButton).gameObject.BindEvent(OnSearchClicked);
    }

    protected override async void InitUI()
    {
        await LoadPendingFriends();        
    }

    private async Task LoadPendingFriends()
    {
        var friendList = await _lobbyVm.LoadPendingFriends();
        _pendingFriends = friendList;
        
        if (_pendingFriends.Count == 0)
        {
            _lobbyVm.OffFriendRequestNotification();
        }
        
        BindFriendRequestPanel(_pendingFriends);
    }

    private void BindFriendRequestPanel(List<FriendUserInfo> friendList)
    {
        var parent = GetImage((int)Images.FriendsRequestPanel).transform;
        Util.DestroyAllChildren(parent);
        
        foreach (var friendInfo in friendList)
        {
            var userInfo = new UserInfo
            {
                UserName = friendInfo.UserName,
                Level = friendInfo.Level,
                RankPoint = friendInfo.RankPoint
            };

            var frame = Managers.Resource.GetFriendRequestFrame(userInfo, parent);
            var acceptButton = Util.FindChild(frame, "AcceptButton", true).GetComponent<Button>();
            var denyButton = Util.FindChild(frame, "DenyButton", true).GetComponent<Button>();
            
            acceptButton.gameObject.BindEvent(OnAcceptFriendRequest);
            denyButton.gameObject.BindEvent(OnDenyFriendRequest);
        }
    }
    
    private void BindSearchResult(List<FriendUserInfo> userInfoList)
    {
        var parent = GetImage((int)Images.SearchResultContent).transform;
        Util.DestroyAllChildren(parent);
        
        foreach (var friendInfo in userInfoList)
        {
            var userInfo = new UserInfo
            {
                UserName = friendInfo.UserName,
                Level = friendInfo.Level,
                RankPoint = friendInfo.RankPoint
            };

            var frame = Managers.Resource.GetFriendFrame(userInfo, parent);
            BindFriendRequestButton(frame, friendInfo);
        }
    }

    private void BindFriendRequestButton(GameObject friendFrame, FriendUserInfo friendInfo)
    {
        var requestButton = Util.FindChild(friendFrame, "RequestButton", true).GetComponent<Button>();
        var alreadyFriendButton = Util.FindChild(friendFrame, "AlreadyFriendButton", true).GetComponent<Button>();
        var pendingButton = Util.FindChild(friendFrame, "PendingButton", true).GetComponent<Button>();
        var blockedButton = Util.FindChild(friendFrame, "BlockedButton", true).GetComponent<Button>();
        var blockButton = Util.FindChild(friendFrame, "BlockButton", true).GetComponent<Button>();
        var deleteButton = Util.FindChild(friendFrame, "DeleteButton", true).GetComponent<Button>();
        
        Util.FindChild(friendFrame, "BlockDeletePanel", true, true).SetActive(false);
        
        blockButton.gameObject.BindEvent(data => OnDeleteClicked(data, friendInfo, true));
        deleteButton.gameObject.BindEvent(data => OnDeleteClicked(data, friendInfo));

        switch (friendInfo.FriendStatus)
        {
            case FriendStatus.Accepted:
                requestButton.gameObject.SetActive(false);
                pendingButton.gameObject.SetActive(false);
                blockedButton.gameObject.SetActive(false);
                alreadyFriendButton.gameObject.BindEvent(OnAlreadyFriendClicked);
                break;
            case FriendStatus.Pending:
                requestButton.gameObject.SetActive(false);
                alreadyFriendButton.gameObject.SetActive(false);
                blockedButton.gameObject.SetActive(false);
                requestButton.gameObject.BindEvent(data => OnFriendRequestClicked(data, friendInfo));
                break;
            case FriendStatus.Blocked:
                alreadyFriendButton.gameObject.SetActive(false);
                requestButton.gameObject.SetActive(false);
                pendingButton.gameObject.SetActive(false);
                blockedButton.gameObject.BindEvent(data => OnFriendRequestClicked(data, friendInfo));
                break;
            case FriendStatus.None:
            default:
                alreadyFriendButton.gameObject.SetActive(false);
                pendingButton.gameObject.SetActive(false);
                blockedButton.gameObject.SetActive(false);
                requestButton.gameObject.BindEvent(data => OnFriendRequestClicked(data, friendInfo));
                break;
        }
    }
    
    private async void OnAcceptFriendRequest(PointerEventData data)
    {
        if (data.pointerPress.transform.parent.TryGetComponent(out Friend friend) == false) return;

        await _lobbyVm.AcceptFriend(friend.Name, true);
        await LoadPendingFriends();
        _lobbyVm.UpdateFriendList();
    }

    private async void OnDenyFriendRequest(PointerEventData data)
    {
        if (data.pointerPress.transform.parent.TryGetComponent(out Friend friend) == false) return;

        await _lobbyVm.AcceptFriend(friend.Name, false);
        await LoadPendingFriends();
    }

    private async void OnSearchClicked(PointerEventData data)
    {
        var username = GetTextInput((int)TextInputs.UsernameInput).text;
        if (username == string.Empty) return;
        
        var friendUserInfoList = await _lobbyVm.SearchUsername(username);
        BindSearchResult(friendUserInfoList);
    }
    
    private async void OnFriendRequestClicked(PointerEventData data, FriendUserInfo friendInfo)
    {
        var go = data.pointerPress.transform.parent.gameObject;
        var response = await _lobbyVm.SendFriendRequest(friendInfo);
        
        if (response.FriendRequestOk == false) return;
        
        var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
        var newFriendInfo = new FriendUserInfo
        {
            UserName = friendInfo.UserName,
            Level = friendInfo.Level,
            RankPoint = friendInfo.RankPoint,
            FriendStatus = response.FriendStatus
        };
        
        popup.GetComponentInChildren<TextMeshProUGUI>().text = "Friend request sent.";
        BindFriendRequestButton(go, newFriendInfo);
    }
    
    private void OnAlreadyFriendClicked(PointerEventData data)
    {
        if (data.pointerPress.transform.parent.TryGetComponent(out Friend friend) == false) return;
        var blockDeletePanel = Util.FindChild(
            friend.gameObject, "BlockDeletePanel", true, true);
        blockDeletePanel.SetActive(!blockDeletePanel.activeSelf);
    }
    
    private async void OnDeleteClicked(PointerEventData data, FriendUserInfo friendInfo, bool isBlock = false)
    {
        var go = data.pointerPress.transform.parent.gameObject;
        var response = await _lobbyVm.DeleteFriend(friendInfo, isBlock);
        
        if (response.Item1.FriendRequestOk == false) return;
        
        var newFriendInfo = new FriendUserInfo
        {
            UserName = friendInfo.UserName,
            Level = friendInfo.Level,
            RankPoint = friendInfo.RankPoint,
            FriendStatus = response.Item1.FriendStatus
        };
        
        BindFriendRequestButton(go, newFriendInfo);
    }
    
    private void OnBackClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI<UI_FriendsAddPopup>();   
    }
    
    private void ClosePopupUI(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
