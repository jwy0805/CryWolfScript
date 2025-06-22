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

public class UI_FriendsListPopup : UI_Popup
{
    private MainLobbyViewModel _lobbyVm;
    private Friend _selectedFriend;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private enum Images
    {
        NoFriendBackground,
        AlertImage,
    }
    
    private enum Buttons
    {
        InviteButton,
        FriendsAddButton,
        ExitButton,
    }

    private enum Texts
    {
        FriendsListTitleText,
        FriendsListNoFriendText,
        FriendsListInviteButtonText,
    }
    
    [Inject]
    public void Construct(MainLobbyViewModel lobbyViewModel)
    {
        _lobbyVm = lobbyViewModel;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitEvents();
        InitButtonEvents();
        InitUI();
    }

    private void InitEvents()
    {
        _lobbyVm.OnFriendRequestNotificationReceived -= OnAlertImage;
        _lobbyVm.OnFriendRequestNotificationReceived += OnAlertImage;
        _lobbyVm.OnFriendRequestNotificationOff -= OffAlertImage;
        _lobbyVm.OnFriendRequestNotificationOff += OffAlertImage;
        _lobbyVm.OnUpdateFriendList -= InitUI;
        _lobbyVm.OnUpdateFriendList += InitUI;
    }
    
    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        
        _ = Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.InviteButton).gameObject.BindEvent(OnInviteClicked);
        GetButton((int)Buttons.FriendsAddButton).gameObject.BindEvent(OnFriendsAddClicked);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
    }

    protected override async void InitUI()
    {
        try
        {
            GetImage((int)Images.AlertImage).gameObject.SetActive(false);
            GetImage((int)Images.NoFriendBackground).gameObject.SetActive(false);
            GetButton((int)Buttons.ExitButton).interactable = false;

            var friendListTask = _lobbyVm.GetFriendList();
            var initAlertsTask = _lobbyVm.InitFriendAlert();

            await Task.WhenAll(friendListTask, initAlertsTask);

            var parent = Util.FindChild(gameObject, "Content", true).transform;
            var friendList = friendListTask.Result;

            Util.DestroyAllChildren(parent);
            if (friendList.Count == 0)
            {
                GetImage((int)Images.NoFriendBackground).gameObject.SetActive(true);
            }
            else
            {
                foreach (var friendInfo in friendList)
                {
                    friendInfo.FriendStatus = FriendStatus.Accepted;
                    var friendFrame = await Managers.Resource.GetFriendFrame(friendInfo, parent, OnSelectFriend);
                    BindFriendRequestButton(friendFrame, friendInfo);
                }
            }
            
            GetButton((int)Buttons.ExitButton).interactable = true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
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

        requestButton.gameObject.SetActive(false);
        pendingButton.gameObject.SetActive(false);
        blockedButton.gameObject.SetActive(false);
        alreadyFriendButton.gameObject.BindEvent(OnAlreadyFriendClicked);
        
        blockButton.gameObject.BindEvent(data => OnDeleteClicked(data, friendInfo, true));
        deleteButton.gameObject.BindEvent(data => OnDeleteClicked(data, friendInfo));
    }
    
    private void OnAlertImage()
    {
        GetImage((int)Images.AlertImage).gameObject.SetActive(true);
    }
    
    private void OffAlertImage()
    {
        GetImage((int)Images.AlertImage).gameObject.SetActive(false);
    }

    private void OnSelectFriend(PointerEventData data)
    {
        data.pointerPress.gameObject.TryGetComponent(out Friend friend);
        if (friend == null) return;
        _selectedFriend = friend;
    }
    
    private void OnInviteClicked(PointerEventData data)
    {
        if (_selectedFriend == null) return;
        // Make a match with the selected friend
    }
    
    private async Task OnFriendsAddClicked(PointerEventData data)
    {
        await Managers.UI.ShowPopupUI<UI_FriendsAddPopup>();
    }
    
    private void OnAlreadyFriendClicked(PointerEventData data)
    {
        if (data.pointerPress.transform.parent.TryGetComponent(out Friend friend) == false) return;
        var blockDeletePanel = Util.FindChild(
            friend.gameObject, "BlockDeletePanel", true, true);
        blockDeletePanel.SetActive(!blockDeletePanel.activeSelf);
    }
    
    private async Task OnDeleteClicked(PointerEventData data, FriendUserInfo friendInfo, bool isBlock = false)
    {
        var response = await _lobbyVm.DeleteFriend(friendInfo, isBlock);
        
        if (response.Item1.FriendRequestOk == false) return;
        
        var newFriendInfo = new FriendUserInfo
        {
            UserName = friendInfo.UserName,
            Level = friendInfo.Level,
            RankPoint = friendInfo.RankPoint,
            FriendStatus = response.Item1.FriendStatus
        };
        
        BindFriendRequestButton(data.pointerPress.gameObject, newFriendInfo);
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }

    private void OnDestroy()
    {
        _lobbyVm.OnFriendRequestNotificationReceived -= OnAlertImage;
        _lobbyVm.OnFriendRequestNotificationOff -= OffAlertImage;
        _lobbyVm.OnUpdateFriendList -= InitUI;
    }
}
