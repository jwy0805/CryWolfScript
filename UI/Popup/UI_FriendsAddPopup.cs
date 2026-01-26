using System;
using System.Collections.Generic;
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
    private List<FriendUserInfo> _sendingFriends;
    private readonly Dictionary<string, GameObject> _textDict = new();

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
    
    private enum Texts
    {
        FriendsAddTitleText,
        FriendsAddUsernameText,
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

    protected override async void Init()
    {
        try
        {
            base.Init();
        
            await BindObjectsAsync();
            InitButtonEvents();
            await InitUIAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override async Task BindObjectsAsync()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<TMP_InputField>(typeof(TextInputs));
        Bind<Image>(typeof(Images));
        
        await Managers.Localization.UpdateTextAndFont(_textDict);
        await Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.UsernameInput));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopupUI);
        GetButton((int)Buttons.BackButton).gameObject.BindEvent(OnBackClicked);
        GetButton((int)Buttons.SearchButton).gameObject.BindEvent(OnSearchClicked);
    }

    protected override async Task InitUIAsync()
    {
        await LoadPendingFriends();        
    }

    private async Task LoadPendingFriends()
    {
        var friendTask = await _lobbyVm.LoadPendingFriends();
        _pendingFriends = friendTask.Item1;
        _sendingFriends = friendTask.Item2;
        
        if (_pendingFriends.Count == 0)
        {
            _lobbyVm.OffFriendRequestNotification();
        }
        
        await BindFriendRequestPanel(_pendingFriends, _sendingFriends);
    }

    private async Task BindFriendRequestPanel(List<FriendUserInfo> pendingList, List<FriendUserInfo> sendingList)
    {
        var parent = GetImage((int)Images.FriendsRequestPanel).transform;
        Util.DestroyAllChildren(parent);
        
        foreach (var friendInfo in sendingList)
        {
            var frame = await Managers.Resource.GetFriendRequestFrame(friendInfo, parent);
            var layoutElement = frame.GetOrAddComponent<LayoutElement>();
            layoutElement.preferredHeight = 200f;
            
            var acceptButton = Util.FindChild(frame, "AcceptButton", true).GetComponent<Button>();
            var denyButton = Util.FindChild(frame, "DenyButton", true).GetComponent<Button>();
            acceptButton.gameObject.BindEvent(OnAcceptFriendRequest);
            denyButton.gameObject.BindEvent(OnDenyFriendRequest);
        }

        foreach (var friendInfo in pendingList)
        {
            var frame = await Managers.Resource.GetFriendRequestFrame(friendInfo, parent);
            var layoutElement = frame.GetOrAddComponent<LayoutElement>();
            layoutElement.preferredHeight = 200f;
            Util.FindChild(frame, "AcceptButton", true).SetActive(false);
            
            var denyButton = Util.FindChild(frame, "DenyButton", true).GetComponent<Button>();
            denyButton.gameObject.BindEvent(OnDenyFriendRequest);
        }
    }
    
    private async Task BindSearchResult(List<FriendUserInfo> userInfoList)
    {
        var parent = GetImage((int)Images.SearchResultContent).transform;
        Util.DestroyAllChildren(parent);
        
        foreach (var friendInfo in userInfoList)
        {
            var frame = await Managers.Resource.GetFriendFrame(friendInfo, parent);
            BindFriendRequestButton(frame, friendInfo);
        }
    }

    private void BindFriendRequestButton(GameObject friendFrame, FriendUserInfo friendInfo)
    {
        var requestButton = Util.FindChild(friendFrame, "RequestButton", true, true).GetComponent<Button>();
        var alreadyFriendButton = Util.FindChild(friendFrame, "AlreadyFriendButton", true, true).GetComponent<Button>();
        var pendingButton = Util.FindChild(friendFrame, "PendingButton", true, true).GetComponent<Button>();
        var blockedButton = Util.FindChild(friendFrame, "BlockedButton", true, true).GetComponent<Button>();
        var blockButton = Util.FindChild(friendFrame, "BlockButton", true, true).GetComponent<Button>();
        var deleteButton = Util.FindChild(friendFrame, "DeleteButton", true, true).GetComponent<Button>();
        
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
        try
        {
            if (data.pointerPress.transform.parent.TryGetComponent(out Friend friend) == false) return;

            await _lobbyVm.AcceptFriend(friend.FriendTag, true);
            await LoadPendingFriends();
            _lobbyVm.UpdateFriendList();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Accept friend failed: " + e);
        }
    }

    private async void OnDenyFriendRequest(PointerEventData data)
    {
        try
        {
            if (data.pointerPress.transform.parent.TryGetComponent(out Friend friend) == false) return;

            await _lobbyVm.AcceptFriend(friend.FriendTag, false);
            await LoadPendingFriends();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Deny friend failed: {e}");
        }
    }

    private async void OnSearchClicked(PointerEventData data)
    {
        try
        {
            var username = GetTextInput((int)TextInputs.UsernameInput).text;
            if (username == string.Empty) return;
        
            var friendUserInfoList = await _lobbyVm.SearchUsername(username);
            await BindSearchResult(friendUserInfoList);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Search friend failed: {e}");
        }
    }
    
    private async void OnFriendRequestClicked(PointerEventData data, FriendUserInfo friendInfo)
    {
        try
        {
            var go = data.pointerPress.transform.parent.gameObject;
            var response = await _lobbyVm.SendFriendRequest(friendInfo);
        
            if (response.FriendRequestOk == false) return;
        
            var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
            await Managers.Localization.UpdateWarningPopupText(popup, "warning_friend_request_sent");
            
            var newFriendInfo = new FriendUserInfo
            {
                UserName = friendInfo.UserName,
                UserTag = friendInfo.UserTag,
                Level = friendInfo.Level,
                RankPoint = friendInfo.RankPoint,
                FriendStatus = response.FriendStatus
            };
            
            BindFriendRequestButton(go, newFriendInfo);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Send friend request failed: {e}");
        }
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
        try
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
        catch (Exception e)
        {
            Debug.LogWarning($"Delete friend request failed: {e}");
        }
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
