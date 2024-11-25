using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_MatchMaking : UI_Scene
{
    private IUserService _userService;
    private ITokenService _tokenService;
    private MatchMakingViewModel _matchMakingVm;
    private DeckViewModel _deckVm;
    
    private RectTransform _loadingMark;
    private bool _loadingMarkActive;
    private bool _cancelClicked;
    
    private enum Buttons
    {
        CancelButton,
        TestButton,
    }
    
    private enum Images
    {
        BackgroundPanel,
        LoadingMarkImage,
        InfoPanel,
        Deck,
        EnemyInfoPanel,
        EnemyDeck,
        VSImage,
    }
    
    private enum Texts
    {
        InfoText,
        CountDownText,
        
        UserNameText,
        RankPointText,
        EnemyUserNameText,
        EnemyRankPointText,
    }
    
    [Inject]
    public void Construct(
        IUserService userService,
        ITokenService tokenService,
        MatchMakingViewModel matchMakingVm,
        DeckViewModel deckVm)
    {
        _userService = userService;
        _tokenService = tokenService;
        _matchMakingVm = matchMakingVm;
        _deckVm = deckVm;
    }

    private void Awake()
    {
        _matchMakingVm.OnMatchMakingStarted -= SetUserInfo;
        _matchMakingVm.OnMatchMakingStarted += SetUserInfo;
    }

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
        
        // Connect to the game session in advance
        _matchMakingVm.ConnectSocketServer();
    }

    protected void Update()
    {
        if (_loadingMarkActive) _loadingMark.Rotate(0, 0, 180 * Time.deltaTime);
        if (_cancelClicked) GetButton((int)Buttons.CancelButton).interactable = false;
    }
    
    public void StartMatchMaking(int sessionId)
    {
        _matchMakingVm.SessionId = sessionId;
        _matchMakingVm.StartMatchMaking();
    }

    private void SetUserInfo()
    {
        var faction = Util.Faction;
        var deck = _deckVm.GetDeck(faction);
        var deckImage = GetImage((int)Images.Deck).transform;
        var userNameText = GetText((int)Texts.UserNameText);
        var rankPointText = GetText((int)Texts.RankPointText);
        
        userNameText.text = _userService.UserInfo.UserName;
        rankPointText.text = _userService.UserInfo.RankPoint.ToString();
        
        foreach (var unit in deck.UnitsOnDeck)
        {
            Util.GetCardResources<UnitId>(unit, deckImage.transform);
        }
    }
    
    public void SetEnemyUserInfo(S_MatchMakingSuccess matchPacket)
    {
        _loadingMarkActive = false;
        
        GetButton((int)Buttons.CancelButton).gameObject.SetActive(false);
        GetImage((int)Images.LoadingMarkImage).gameObject.SetActive(false);
        GetImage((int)Images.EnemyInfoPanel).gameObject.SetActive(true);
        GetImage((int)Images.VSImage).gameObject.SetActive(true);
        
        var deckImage = GetImage((int)Images.EnemyDeck).transform;
        var enemyUserNameText = GetText((int)Texts.EnemyUserNameText);
        var enemyRankPointText = GetText((int)Texts.EnemyRankPointText);
        
        enemyUserNameText.text = matchPacket.EnemyUserName;
        enemyRankPointText.text = matchPacket.EnemyRankPoint.ToString();
        
        foreach (var unitId in matchPacket.EnemyUnitIds)
        {
            Managers.Data.UnitInfoDict.TryGetValue(unitId, out var unitInfo);
            if (unitInfo == null) continue;
            Util.GetCardResources<UnitId>(unitInfo, deckImage.transform);
        }

        StartCoroutine(CountDown());
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
        
        EnterGame();
    }
    
    private void EnterGame()
    {
        Managers.Map.MapId = 1;
        Managers.Scene.LoadScene(Define.Scene.Game);
    }
    
    // Button Events
    private void OnCancelClicked(PointerEventData data)
    {
        _cancelClicked = true;
        _matchMakingVm.CancelMatchMaking();
    }
    
    private void OnTestButtonClicked(PointerEventData data)
    {
        _matchMakingVm.TestMatchMaking();
        GetButton((int)Buttons.TestButton).interactable = false;
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
        GetButton((int)Buttons.CancelButton).gameObject.BindEvent(OnCancelClicked);
        GetButton((int)Buttons.TestButton).gameObject.BindEvent(OnTestButtonClicked);
    }
    
    protected override void InitUI()
    {
        GetText((int)Texts.CountDownText).gameObject.SetActive(false);
        GetImage((int)Images.BackgroundPanel).color = Util.ThemeColor;
        GetImage((int)Images.EnemyInfoPanel).gameObject.SetActive(false);
        GetImage((int)Images.VSImage).gameObject.SetActive(false);
        SetObjectSize(GetImage((int)Images.LoadingMarkImage).gameObject, 0.2f);
        
        _loadingMark = GetImage((int)Images.LoadingMarkImage).rectTransform;
        _loadingMarkActive = true;
    }

    private void OnDestroy()
    {
        _matchMakingVm.OnMatchMakingStarted -= SetUserInfo;
    }
}
