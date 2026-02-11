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
    private ITokenService _tokenService;
    private IUIFactory _uiFactory;
    private ISignalRClient _signalRClient;
    private MainLobbyViewModel _lobbyVm;
    private FriendlyMatchViewModel _friendVm;

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
        ITokenService tokenService, 
        IUIFactory uiFactory,
        ISignalRClient signalRClient, 
        MainLobbyViewModel lobbyViewModel,
        FriendlyMatchViewModel friendViewModel)
    {
        _tokenService = tokenService;
        _uiFactory = uiFactory;
        _signalRClient = signalRClient;
        _lobbyVm = lobbyViewModel;
        _friendVm = friendViewModel;
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
        var packet = new LoadInvitableFriendPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
        };
        
        var response = await _signalRClient.LoadFriends(packet);
        await BindFriendsListPanel(response.InvitableFriends, response.Others);
    }
    
    private async Task BindFriendsListPanel(List<FriendUserInfo> invitable, List<FriendUserInfo> others)
    {
        var parent = GetImage((int)Images.FriendsListPanel).transform;
        Util.DestroyAllChildren(parent);

        foreach (var friend in invitable)
        {
            var friendFrame = await _uiFactory.GetFriendInviteFrame(friend, parent);
            BindFriendInviteButton(friendFrame, friend, true);
        }
        
        foreach (var friend in others)
        {
            var friendFrame = await _uiFactory.GetFriendInviteFrame(friend, parent);
            BindFriendInviteButton(friendFrame, friend, false);
        }
    }

    private void BindFriendInviteButton(GameObject friendFrame, FriendUserInfo friendInfo, bool interactable)
    {
        var inviteButton = Util.FindChild(friendFrame, "InviteButton", true).GetComponent<Button>();
        var friendTag = friendInfo.UserTag;
        inviteButton.interactable = interactable;
        
        if (interactable)
        {
            inviteButton.gameObject.BindEvent(data => OnInviteClicked(data, friendTag));
        }
    }
    
    // Button Events
    private void OnInviteClicked(PointerEventData data, string friendTag)
    {
        var packet = new InviteFriendlyMatchPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            InviteeTag = friendTag,
        };
        
        _signalRClient.SendInvitation(packet);
        Managers.UI.ClosePopupUI();
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
