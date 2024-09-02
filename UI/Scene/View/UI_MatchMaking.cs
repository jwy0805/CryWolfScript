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
    
    private Transform _myDeck;
    private Transform _enemyDeck;
    private RectTransform _loadingMark;
    private bool _loadingMarkActive;
    private bool _cancelClicked;

    private enum Buttons
    {
        CancelButton,
    }
    
    private enum Images
    {
        LoadingMarkImage,
        MyDeckPanel,
        EnemyDeckPanel,
        VSImage,
    }
    
    private enum Texts
    {
        InfoText,
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
        _matchMakingVm.OnMatchMakingStarted -= SetDeckInfo;
        _matchMakingVm.OnMatchMakingStarted += SetDeckInfo;
    }

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
        
        _matchMakingVm.StartMatchMaking();
    }

    protected void Update()
    {
        if (_loadingMarkActive) _loadingMark.Rotate(0, 0, 180 * Time.deltaTime);
        if (_cancelClicked) GetButton((int)Buttons.CancelButton).interactable = false;
    }

    private void SetDeckInfo()
    {
        _myDeck = GetImage((int)Images.MyDeckPanel).transform;
        var deck = _deckVm.GetDeck(Util.Camp);
        foreach (var unit in deck.UnitsOnDeck)
        {
            Util.GetCardResources(unit, _myDeck, 150);
        }
    }
    
    public void SetEnemyInfo()
    {
        _loadingMarkActive = false;
        
        GetButton((int)Buttons.CancelButton).gameObject.SetActive(false);
        GetImage((int)Images.LoadingMarkImage).gameObject.SetActive(false);
        GetImage((int)Images.EnemyDeckPanel).gameObject.SetActive(true);
        GetImage((int)Images.VSImage).gameObject.SetActive(true);
    }
    
    private void SomeMethod()
    {
        Managers.Map.MapId = 1;
        Managers.Scene.LoadScene(Define.Scene.Game);
        Managers.Clear();
    }
    
    // Button Events
    private void OnCancelClicked(PointerEventData data)
    {
        _cancelClicked = true;
        _matchMakingVm.CancelMatchMaking();
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
    }
    
    protected override void InitUI()
    {
        GetImage((int)Images.EnemyDeckPanel).gameObject.SetActive(false);
        GetImage((int)Images.VSImage).gameObject.SetActive(false);
        SetObjectSize(GetImage((int)Images.LoadingMarkImage).gameObject, 0.2f);
        
        _loadingMark = GetImage((int)Images.LoadingMarkImage).rectTransform;
        _loadingMarkActive = true;
    }
}
