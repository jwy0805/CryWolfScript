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

public class UI_FriendlyMatch : UI_Scene
{
    private ISignalRClient _signalRClient;
    private ITokenService _tokenService;
    private FriendlyMatchViewModel _friendlyMatchVm;
    private MatchMakingViewModel _matchMakingVm;
    private DeckViewModel _deckVm;

    private Dictionary<string, GameObject> _deckButtonDict;
    
    public bool IsHost { get; set; }
    
    private enum Buttons
    {
        BackButton,
        SwitchButton,
        InviteButton,
        StartButton,
        
        DeckButton1,
        DeckButton2,
        DeckButton3,
        DeckButton4,
        DeckButton5,
    }

    private enum Images
    {
        BackgroundPanel,
        Deck,
        EnemyDeckUserInfoPanel,
        EnemyDeck,
        EnemyRankImage,
    }

    private enum Texts
    {
        MatchMakingCountDownText,
        UserNameText,
        RankPointText,
        EnemyUserNameText,
        EnemyRankPointText,
    }
    
    [Inject]
    public void Construct(
        ISignalRClient signalRClient,
        ITokenService tokenService,
        FriendlyMatchViewModel friendlyMatchVm,
        MatchMakingViewModel matchMakingVm,
        DeckViewModel deckVm)
    {
        _signalRClient = signalRClient;
        _tokenService = tokenService;
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

    private void BindEvents()
    {
        _signalRClient.OnInvitationSuccess += OnInvitationSuccess;
        _signalRClient.OnEnemyDeckSwitched += OnEnemyDeckSwitched;
        _signalRClient.OnFactionSwitched += OnFactionSwitched;
        _signalRClient.OnGuestLeft += OnGuestLeft;
        _signalRClient.OnStartFriendlyMatch += OnStartFriendlyMatch;
        _deckVm.OnDeckSwitched += SetDeckUI;
    }
    
    // UI Setting
    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _deckButtonDict = new Dictionary<string, GameObject>
        {
            { "DeckButton1", GetButton((int)Buttons.DeckButton1).gameObject },
            { "DeckButton2", GetButton((int)Buttons.DeckButton2).gameObject },
            { "DeckButton3", GetButton((int)Buttons.DeckButton3).gameObject },
            { "DeckButton4", GetButton((int)Buttons.DeckButton4).gameObject },
            { "DeckButton5", GetButton((int)Buttons.DeckButton5).gameObject }
        };
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.BackButton).onClick.AddListener(OnBackClicked);
        GetButton((int)Buttons.SwitchButton).onClick.AddListener(() => _ = OnSwitchClicked());
        GetButton((int)Buttons.InviteButton).onClick.AddListener(() => _ = OnInviteClicked());
        GetButton((int)Buttons.StartButton).onClick.AddListener(() => _ = OnStartClicked());

        foreach (var go in _deckButtonDict.Values)
        {
            go.BindEvent(OnDeckButtonClicked);
        }
    }

    protected override async Task InitUIAsync()
    {
        IsHost = Managers.Network.IsFriendlyMatchHost;
        GetButton((int)Buttons.StartButton).gameObject.SetActive(false);

        if (IsHost)
        {
            GetButton((int)Buttons.SwitchButton).interactable = true;
            await _friendlyMatchVm.JoinGame();
        }
        else
        {
            GetButton((int)Buttons.SwitchButton).interactable = false;
        }
        
        if (Managers.Game.ReEntry)
        {
            await SetEnemyInfo(Managers.Game.ReEntryResponse);
        }
        else
        {
            if (!IsHost)
            {
                var response = Managers.Game.FriendlyMatchResponse;
                Util.Faction = response.MyFaction;
                await SetEnemyInfo(response);
            }
        }
        
        GetImage((int)Images.BackgroundPanel).color = Util.ThemeColor;
        await SetUserInfo();
    }
    
    private async Task SetUserInfo()
    {
        var faction = Util.Faction;
        var userNameText = GetText((int)Texts.UserNameText);
        var rankPointText = GetText((int)Texts.RankPointText);
        
        userNameText.text = User.Instance.UserInfo.UserName;
        rankPointText.text = User.Instance.UserInfo.RankPoint.ToString();
        
        await SetDeckUI(faction);
    }

    private async Task SetDeckUI(Faction faction)
    {
        try
        {
            var deck = _deckVm.GetDeck(faction);
            var deckImage = GetImage((int)Images.Deck).transform;
            var deckNumber = faction == Faction.Sheep 
                ? User.Instance.DeckSheep.DeckNumber 
                : User.Instance.DeckWolf.DeckNumber;
        
            Util.DestroyAllChildren(deckImage.transform);

            foreach (var unit in deck.UnitsOnDeck)
            {
                await Managers.Resource.GetCardResources<UnitId>(unit, deckImage.transform);
            }

            for (var i = 1; i <= 5; i++)
            {
                _deckButtonDict[$"DeckButton{i}"].GetComponent<DeckButtonInfo>().DeckIndex = i;
                _deckButtonDict[$"DeckButton{i}"].GetComponent<DeckButtonInfo>().IsSelected = false;
            }        
        
            _deckButtonDict[$"DeckButton{deckNumber}"].GetComponent<DeckButtonInfo>().IsSelected = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    private async void OnInvitationSuccess(AcceptInvitationPacketResponse response)
    {
        try
        {
            await SetEnemyInfo(response);
            GetButton((int)Buttons.StartButton).gameObject.SetActive(true);
            GetButton((int)Buttons.StartButton).interactable = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    private async Task SetEnemyInfo(AcceptInvitationPacketResponse response)
    {
        var enemyUserNameText = GetText((int)Texts.EnemyUserNameText);
        var enemyRankPointText = GetText((int)Texts.EnemyRankPointText);
        var enemyDeckInfo = response.MyFaction == Faction.Sheep ? response.EnemyDeckWolf : response.EnemyDeckSheep;

        GetButton((int)Buttons.InviteButton).gameObject.SetActive(false);
        enemyUserNameText.text = response.EnemyInfo.UserName;
        enemyRankPointText.text = response.EnemyInfo.RankPoint.ToString();

        await OnEnemyDeckSwitched(enemyDeckInfo);
    }

    private async Task OnEnemyDeckSwitched(DeckInfo deckInfo)
    {
        var enemyDeck = GetImage((int)Images.EnemyDeck).transform;
        Util.DestroyAllChildren(enemyDeck);

        foreach (var unit in deckInfo.UnitInfo)         
        {
            await Managers.Resource.GetCardResources<UnitId>(unit, enemyDeck);
        }
    }
    
    private async Task OnFactionSwitched(DeckInfo myDeckInfo, DeckInfo enemyDeckInfo, bool isGuest)
    {
        if (isGuest) // Host 는 호출시 이미 팩션 바뀜
        {
            Util.Faction = Util.Faction == Faction.Sheep ? Faction.Wolf : Faction.Sheep;
        }
        
        var deck = new Deck
        {
            DeckId = myDeckInfo.DeckId,
            UnitsOnDeck = myDeckInfo.UnitInfo ?? Array.Empty<UnitInfo>(),
            DeckNumber = myDeckInfo.DeckNumber,
            Faction = Util.Faction,
            LastPicked = true
        };
        
        if (Util.Faction == Faction.Sheep)
        {
            User.Instance.DeckSheep = deck;
        }
        else
        {
            User.Instance.DeckWolf = deck;
        }
        
        var myDeck = GetImage((int)Images.Deck).transform;
        GetImage((int)Images.BackgroundPanel).color = Util.ThemeColor;
        Util.DestroyAllChildren(myDeck);
        
        foreach (var unit in myDeckInfo.UnitInfo ?? Array.Empty<UnitInfo>())
        {
            await Managers.Resource.GetCardResources<UnitId>(unit, myDeck);
        }

        if (enemyDeckInfo?.UnitInfo?.Length > 0)
        {
            await OnEnemyDeckSwitched(enemyDeckInfo);
        }
    }

    private void OnGuestLeft()
    {
        Util.DestroyAllChildren( GetImage((int)Images.EnemyDeck).transform);
        GetButton((int)Buttons.InviteButton).gameObject.SetActive(true);       
    }
    
    // Button Events
    private void OnBackClicked()
    {
        GetButton((int)Buttons.BackButton).interactable = false;
        Managers.Scene.LoadScene(Define.Scene.MainLobby);
        Managers.Network.IsFriendlyMatchHost = false;
        Managers.Game.ReEntry = false;
        Managers.Game.ReEntryResponse = null;
        _signalRClient.LeaveGame();
        GetButton((int)Buttons.BackButton).interactable = true;
    }
    
    private async Task OnSwitchClicked()
    {
        GetButton((int)Buttons.SwitchButton).interactable = false;
        await _friendlyMatchVm.SwitchFaction();
        GetButton((int)Buttons.SwitchButton).interactable = true;
    }
    
    private async Task OnInviteClicked()
    {
        GetButton((int)Buttons.InviteButton).interactable = false;
        await Managers.UI.ShowPopupUI<UI_FriendsInvitePopup>();
        GetButton((int)Buttons.InviteButton).interactable = true;
    }

    private async Task OnDeckButtonClicked(PointerEventData data)
    {
        var buttonNumber = data.pointerPress.GetComponent<DeckButtonInfo>().DeckIndex;
        var token = _tokenService.GetAccessToken();
        await _deckVm.SelectDeck(buttonNumber, Util.Faction);
        await _signalRClient.SwitchDeckOnFriendlyMatch(token, Util.Faction);
    }

    private async Task OnStartClicked()
    {
        GetButton((int)Buttons.StartButton).interactable = false;
        await _signalRClient.StartFriendlyMatch(User.Instance.UserInfo.UserTag);
        GetButton((int)Buttons.StartButton).interactable = true;
    }

    private async Task OnStartFriendlyMatch()
    {
        GetButton((int)Buttons.SwitchButton).gameObject.SetActive(false);
        GetButton((int)Buttons.BackButton).interactable = false;
        GetButton((int)Buttons.StartButton).interactable = false;
        StartCoroutine(CountDown());
        
        await Managers.Network.ConnectGameSession();
    }
    
    private IEnumerator CountDown(int seconds = 6)
    {
        var text = GetText((int)Texts.MatchMakingCountDownText);
        text.gameObject.SetActive(true);
        
        for (var i = seconds - 1; i >= 0; i--)
        {
            text.text = $"{i} seconds left to start the game";
            yield return new WaitForSeconds(1);
        }
        
        _matchMakingVm.EnterGame();
    }
    
    private void OnDestroy()
    {
        _signalRClient.OnInvitationSuccess -= OnInvitationSuccess;
        _signalRClient.OnFactionSwitched -= OnFactionSwitched;
        _signalRClient.OnGuestLeft -= OnGuestLeft;
        _signalRClient.OnGuestLeft -= OnGuestLeft;
        _signalRClient.OnStartFriendlyMatch -= OnStartFriendlyMatch;
        _deckVm.OnDeckSwitched -= SetDeckUI;
    }
}
