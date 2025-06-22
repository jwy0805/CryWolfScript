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

public class UI_MatchMaking : UI_Scene
{
    private IUserService _userService;
    private MatchMakingViewModel _matchMakingVm;
    private DeckViewModel _deckVm;
    
    private RectTransform _loadingMark;
    private bool _loadingMarkActive;
    private bool _cancelClicked;
    private readonly float _waitingNumberRefreshTime = 10f;
    private float _waitingNumberTimer;
    
    private enum Buttons
    {
        CancelButton,
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
        MatchMakingQueueInfoText,
        MatchMakingCountDownText,
        UserNameText,
        RankPointText,
        EnemyUserNameText,
        EnemyRankPointText,
    }
    
    [Inject]
    public void Construct(
        IUserService userService,
        MatchMakingViewModel matchMakingVm,
        DeckViewModel deckVm)
    {
        _userService = userService;
        _matchMakingVm = matchMakingVm;
        _deckVm = deckVm;
    }

    private void Awake()
    {
        _matchMakingVm.OnMatchMakingStarted += SetUserInfo;
        _matchMakingVm.OnRefreshQueueCounts += SetQueueInfo;
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
        
        _waitingNumberTimer += Time.deltaTime;
        if (_waitingNumberTimer >= _waitingNumberRefreshTime)
        {
            _waitingNumberTimer = 0f;
            _ = _matchMakingVm.GetQueueCounts();
        }
    }
    
    public void StartMatchMaking(int sessionId)
    {
        _matchMakingVm.SessionId = sessionId;
        _matchMakingVm.StartMatchMaking();
    }

    private async Task SetUserInfo()
    {
        var faction = Util.Faction;
        var deck = _deckVm.GetDeck(faction);
        var deckImage = GetImage((int)Images.Deck).transform;
        var userNameText = GetText((int)Texts.UserNameText);
        var rankPointText = GetText((int)Texts.RankPointText);
        
        await Managers.Localization.UpdateFont(userNameText);
        userNameText.text = _userService.UserInfo.UserName;
        rankPointText.text = _userService.UserInfo.RankPoint.ToString();
        
        foreach (var unit in deck.UnitsOnDeck)
        {
            await Managers.Resource.GetCardResources<UnitId>(unit, deckImage.transform);
        }
    }
    
    public async Task SetEnemyUserInfo(S_MatchMakingSuccess matchPacket)
    {
        _loadingMarkActive = false;
        
        GetButton((int)Buttons.CancelButton).gameObject.SetActive(false);
        GetImage((int)Images.LoadingMarkImage).gameObject.SetActive(false);
        GetImage((int)Images.EnemyInfoPanel).gameObject.SetActive(true);
        GetImage((int)Images.VSImage).gameObject.SetActive(true);
        
        var deckImage = GetImage((int)Images.EnemyDeck).transform;
        var enemyUserNameText = GetText((int)Texts.EnemyUserNameText);
        var enemyRankPointText = GetText((int)Texts.EnemyRankPointText);
        
        await Managers.Localization.UpdateFont(enemyUserNameText);
        enemyUserNameText.text = matchPacket.EnemyUserName;
        enemyRankPointText.text = matchPacket.EnemyRankPoint.ToString();
        
        foreach (var unitId in matchPacket.EnemyUnitIds)
        {
            Managers.Data.UnitInfoDict.TryGetValue(unitId, out var unitInfo);
            if (unitInfo == null) continue;
            await Managers.Resource.GetCardResources<UnitId>(unitInfo, deckImage.transform);
        }

        StartCoroutine(CountDown());
    }

    private async Task SetQueueInfo(int queueCountsWolf, int queueCountsSheep)
    {
        var text = GetText((int)Texts.MatchMakingQueueInfoText);
        var key = "match_making_queue_info_text";
        var placeholders = new List<string> {"value1", "value2"};
        
        if (Util.Faction == Faction.Sheep)
        {
            queueCountsSheep -= 1;
            if (queueCountsSheep < 0) queueCountsSheep = 0;
        }
        else
        {
            queueCountsWolf -= 1;
            if (queueCountsWolf < 0) queueCountsWolf = 0;
        }
        
        var replaceValues = new List<string> { queueCountsWolf.ToString(), queueCountsSheep.ToString() };
        await Managers.Localization.FormatLocalizedText(text, key, placeholders, replaceValues);
    }
    
    private IEnumerator CountDown(int seconds = 6)
    {
        var text = GetText((int)Texts.MatchMakingCountDownText);
        text.gameObject.SetActive(true);
        
        for (var i = seconds - 1; i >= 0; i--)
        {
            var key = "match_making_count_down_text";
            var placeHolderKeys = new List<string> {"value"};
            var replaceValues = new List<string> {i.ToString()};
            _ = Managers.Localization.FormatLocalizedText(text, key, placeHolderKeys, replaceValues);
            yield return new WaitForSeconds(1);
        }
        
        _matchMakingVm.EnterGame();
    }
    
    // Button Events
    private async Task OnCancelClicked(PointerEventData data)
    {
        if (_cancelClicked) return;
        _cancelClicked = true;

        GetButton((int)Buttons.CancelButton).interactable = false;

        try
        {
            await _matchMakingVm.CancelMatchMaking();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            _cancelClicked = false;
            GetButton((int)Buttons.CancelButton).interactable = true;
        }
    }
    
    private void OnTestButtonClicked(PointerEventData data)
    {
        _matchMakingVm.TestMatchMaking();
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
        #if UNITY_EDITOR
        Util.FindChild(gameObject, "TestButton", true).BindEvent(OnTestButtonClicked);
        #endif
    }
    
    protected override void InitUI()
    {
        _ = _matchMakingVm.GetQueueCounts();
            
        GetText((int)Texts.MatchMakingCountDownText).gameObject.SetActive(false);
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
        _matchMakingVm.OnRefreshQueueCounts -= SetQueueInfo;
    }
}
