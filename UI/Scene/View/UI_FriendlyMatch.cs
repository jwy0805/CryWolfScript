using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_FriendlyMatch : UI_Scene
{
    private ISignalRClient _signalRClient;
    private FriendlyMatchViewModel _friendlyMatchVm;
    private MatchMakingViewModel _matchMakingVm;
    private DeckViewModel _deckVm;

    private RectTransform _enemyFrame;
    private RectTransform _loadingMark;
    private bool _loadingMarkActive;
    
    public bool IsHost { get; set; }
    
    private enum Buttons
    {
        BackButton,
        SwitchButton,
        InviteButton,
        StartButton,
    }

    private enum Images
    {
        BackgroundPanel,
        LoadingMarkImage,
        Deck,
        InviteFrame,
        EnemyDeckUserInfoPanel,
        EnemyDeck,
    }

    private enum Texts
    {
        CountDownText,
        UserNameText,
        RankPointText,
        EnemyUserNameText,
        EnemyRankPointText,
    }
    
    [Inject]
    public void Construct(
        ISignalRClient signalRClient,
        FriendlyMatchViewModel friendlyMatchVm,
        MatchMakingViewModel matchMakingVm,
        DeckViewModel deckVm)
    {
        _signalRClient = signalRClient;
        _friendlyMatchVm = friendlyMatchVm;
        _matchMakingVm = matchMakingVm;
        _deckVm = deckVm;
    }

    protected override async void Init()
    {
        try
        {
            base.Init();
        
            BindEvents();
            BindObjects();
            InitButtonEvents();
            await InitUIAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected void Update()
    {
        if (_loadingMarkActive) _loadingMark.Rotate(0, 0, 180 * Time.deltaTime);
    }

    private void BindEvents()
    {
        _signalRClient.OnInvitationSuccess -= OnInvitationSuccess;
        _signalRClient.OnInvitationSuccess += OnInvitationSuccess;
    }
    
    // UI Setting
    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.BackButton).gameObject.BindEvent(OnBackClicked);
        GetButton((int)Buttons.SwitchButton).gameObject.BindEvent(OnSwitchClicked);
        GetButton((int)Buttons.InviteButton).gameObject.BindEvent(OnInviteClicked);
    }

    protected override async Task InitUIAsync()
    {
        IsHost = Managers.Network.IsFriendlyMatchHost;
        _loadingMark = GetImage((int)Images.LoadingMarkImage).rectTransform;
        _enemyFrame = GetImage((int)Images.EnemyDeckUserInfoPanel).rectTransform;
        
        if (IsHost)
        {
            GetButton((int)Buttons.StartButton).gameObject.SetActive(true);
            GetButton((int)Buttons.SwitchButton).interactable = true;
            _enemyFrame.gameObject.SetActive(false);
        }
        else
        {
            GetButton((int)Buttons.StartButton).gameObject.SetActive(false);
            GetButton((int)Buttons.SwitchButton).interactable = false;
            _enemyFrame.gameObject.SetActive(true);
        }
        
        GetImage((int)Images.BackgroundPanel).color = Util.ThemeColor;
        SetObjectSize(_loadingMark.gameObject, 0.2f);
        await SetUserInfo();
    }
    
    private async Task SetUserInfo()
    {
        var faction = Util.Faction;
        var deck = _deckVm.GetDeck(faction);
        var deckImage = GetImage((int)Images.Deck).transform;
        var userNameText = GetText((int)Texts.UserNameText);
        var rankPointText = GetText((int)Texts.RankPointText);
        
        userNameText.text = User.Instance.UserInfo.UserName;
        rankPointText.text = User.Instance.UserInfo.RankPoint.ToString();
        
        foreach (var unit in deck.UnitsOnDeck)
        {
            await Managers.Resource.GetCardResources<UnitId>(unit, deckImage.transform);
        }
    }
    
    private async void OnInvitationSuccess(AcceptInvitationPacketResponse response)
    {
        try
        {
            _enemyFrame.gameObject.SetActive(true);
            GetImage((int)Images.InviteFrame).gameObject.SetActive(false);
            await SetEnemyInfo(response);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    private async Task SetEnemyInfo(AcceptInvitationPacketResponse response)
    {
        var enemyDeckImage = GetImage((int)Images.EnemyDeck).transform;
        var enemyUserNameText = GetText((int)Texts.EnemyUserNameText);
        var enemyRankPointText = GetText((int)Texts.EnemyRankPointText);
        var enemyDeckInfo = response.MyFaction == Faction.Sheep ? response.EnemyDeckWolf : response.EnemyDeckSheep;

        enemyUserNameText.text = response.EnemyInfo.UserName;
        enemyRankPointText.text = response.EnemyInfo.RankPoint.ToString();

        foreach (var unit in enemyDeckInfo.UnitInfo)
        {
            await Managers.Resource.GetCardResources<UnitId>(unit, enemyDeckImage.transform);
        }
    }
    
    private IEnumerator CountDown(int seconds = 6)
    {
        var text = GetText((int)Texts.CountDownText);
        text.gameObject.SetActive(true);
        
        for (var i = seconds - 1; i >= 0; i--)
        {
            text.text = $"{i} seconds left to start the game";
            yield return new WaitForSeconds(1);
        }
        
        _matchMakingVm.EnterGame();
    }
    
    // Button Events
    private void OnBackClicked(PointerEventData data)
    {
        Managers.Scene.LoadScene(Define.Scene.MainLobby);
    }
    
    private void OnSwitchClicked(PointerEventData data)
    {
        _friendlyMatchVm.SwitchFaction();
    }
    
    private async Task OnInviteClicked(PointerEventData data)
    {
        await Managers.UI.ShowPopupUI<UI_FriendsInvitePopup>();
    }

    private void OnDestroy()
    {
        Managers.Network.IsFriendlyMatchHost = false;
        _signalRClient.OnInvitationSuccess -= OnInvitationSuccess;
    }
}
