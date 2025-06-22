using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_FriendsInvitePopup : UI_Popup
{
    private IUserService _userService;
    private ITokenService _tokenService;
    private ISignalRClient _signalRClient;
    private MainLobbyViewModel _lobbyVm;

    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private enum Images
    {
        FriendsListPanel,
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
        FriendsInviteTitleText,
        FriendsInviteUsernameText,
    }
    
    private enum TextInputs
    {
        UsernameInput
    }
    
    [Inject]
    public void Construct(
        IUserService userService,
        ITokenService tokenService, 
        ISignalRClient signalRClient, 
        MainLobbyViewModel lobbyViewModel)
    {
        _userService = userService;
        _tokenService = tokenService;
        _signalRClient = signalRClient;
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
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.BackButton).gameObject.BindEvent(ClosePopup);
    }

    protected override async Task InitUIAsync()
    {
        await LoadFriends();
    }
    
    private async Task LoadFriends()
    {
        var friendList = await _lobbyVm.GetFriendList();
        await BindFriendsListPanel(friendList);
    }

    private async Task BindFriendsListPanel(List<FriendUserInfo> friendList)
    {
        var parent = GetImage((int)Images.FriendsListPanel).transform;
        Util.DestroyAllChildren(parent);
        foreach (var friend in friendList)
        {
            var friendUserInfo = new FriendUserInfo
            {
                UserName = friend.UserName,
                Level = friend.Level,
                RankPoint = friend.RankPoint
            };
            
            var friendFrame = await Managers.Resource.GetFriendInviteFrame(friendUserInfo, parent);
            BindFriendInviteButton(friendFrame, friendUserInfo);
        }
    }

    private void BindFriendInviteButton(GameObject friendFrame, FriendUserInfo friendInfo)
    {
        var inviteButton = Util.FindChild(friendFrame, "InviteButton", true).GetComponent<Button>();
        var friendName = friendFrame.GetComponent<Friend>().FriendName;
        
        if (friendInfo.Act != UserAct.InLobby)
        {
            inviteButton.interactable = false;
        }
        else
        {
            inviteButton.gameObject.BindEvent(data => OnInviteClicked(data, friendName));
        }
    }
    
    // Button Events
    private void OnInviteClicked(PointerEventData data, string friendName)
    {
        var packet = new InviteFriendlyMatchPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            InviterName = _userService.UserInfo.UserName,
            InviteeName = friendName,
        };
        
        _signalRClient.SendInvitation(packet);
        Managers.UI.ClosePopupUI();
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
