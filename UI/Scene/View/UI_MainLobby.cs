using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Google.Protobuf.Protocol;
using NUnit.Framework.Internal;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

/* Last Modified : 25. 04. 22
 * Version : 1.02
 */

// This class includes the binding, initialization and core logics for the main lobby UI.
public partial class UI_MainLobby : UI_Scene, IPointerClickHandler
{
    [SerializeField] private Scrollbar scrollbar;
    
    private const string DiscordInviteUrl = "https://discord.gg/m6FZGpJqXg";
    
    private IUserService _userService;
    private ITokenService _tokenService;
    private IPaymentService _paymentService;
    private MainLobbyViewModel _lobbyVm;
    private DeckViewModel _deckVm;
    private CollectionViewModel _collectionVm;
    private CraftingViewModel _craftingVm;
    private TutorialViewModel _tutorialVm;
    private ShopViewModel _shopVm;

    private LobbyUtilWidget _utilWidget;
    private LobbyTutorialWidget _tutorialWidget;
    private LobbyHeaderWidget _headerWidget;
    private LobbyModeWidget _modeWidget;
    private LobbyDeckWidget _deckWidget;
    private LobbyCollectionWidget _collectionWidget;
    private LobbyCraftingWidget _craftingWidget;
    private LobbyShopWidget _shopWidget;
    
    private Slider _expSlider;
    private int _currentModeIndex;
    private Card _selectedCard;
    private UI_CardClickPopup _cardPopup;
    private RectTransform _loadingMark;
    private RectTransform _craftingPanel;
    private ScrollRect _craftingScrollRect;
    private List<GameObject> _modes;
    private Dictionary<string, GameObject> _craftingUiDict;
    private Dictionary<string, GameObject> _collectionUiDict;
    private Dictionary<string, GameObject> _arrangeButtonDict;
    private Dictionary<string, GameObject> _bottomButtonDict;
    private Dictionary<string, GameObject> _bottomButtonFocusDict;
    private Dictionary<string, GameObject> _tabButtonDict;
    private Dictionary<string, GameObject> _deckButtonDict;
    private Dictionary<string, GameObject> _lobbyDeckButtonDict;
    private Dictionary<string, GameObject> _textDict = new();
    private Define.SelectMode _selectMode;
    private Define.GameMode _gameMode;
    private Define.ArrangeMode _arrangeMode = Define.ArrangeMode.All;
    private Coroutine _shakeButtonRoutine;
    private Vector2 _leftButtonOriginalPos;
    private Vector2 _rightButtonOriginalPos;
    
    private Define.SelectMode SelectMode
    {
        get => _selectMode;
        set
        {
            if (_selectMode is Define.SelectMode.Reinforce or Define.SelectMode.Recycle)
            {
                _ = _collectionWidget.SetCollectionUIDetails(Util.Faction);
            }
            _selectMode = value;
            Debug.Log($"Select Mode changed to: {_selectMode}");
        }
    }
    
    private Define.ArrangeMode ArrangeMode
    {
        get => _arrangeMode;
        set
        {
            _arrangeMode = value;
            if (_collectionWidget != null) _collectionWidget.ArrangeMode = value;
            
            if (SelectMode == Define.SelectMode.Reinforce)
            {
                _ = _craftingWidget.ResetCollectionUIForReinforce();
            }
            else
            {
                if (_collectionWidget != null)
                {
                    _ = _collectionWidget.SetCollectionUIDetails(Util.Faction);
                }
            }
        }
    }
    
    private UI_CardClickPopup CardPopup
    {
        get => _cardPopup;
        set
        {
            if (_cardPopup != null) Managers.UI.ClosePopupUI(_cardPopup);
            _cardPopup = value;
        }
    }

    [Inject]
    public void Construct(
        IUserService userService,
        ITokenService tokenService,
        IPaymentService paymentService,
        MainLobbyViewModel viewModel,
        DeckViewModel deckViewModel,
        CollectionViewModel collectionViewModel,
        CraftingViewModel craftingViewModel,
        TutorialViewModel tutorialViewModel,
        ShopViewModel shopViewModel)
    {
        _userService = userService;
        _tokenService = tokenService;
        _paymentService = paymentService;
        _lobbyVm = viewModel;
        _deckVm = deckViewModel;
        _collectionVm = collectionViewModel;
        _craftingVm = craftingViewModel;
        _tutorialVm = tutorialViewModel;
        _shopVm = shopViewModel;
    }

    private void Awake()
    {
        InitWidgets();

        _lobbyVm.Initialize(Util.FindChild(gameObject, "HorizontalContents", true)
            .transform.childCount);
    }

    private void OnEnable()
    {
        ReleaseEvents();
        BindEvents();
    }

    private void InitWidgets()
    {
        _utilWidget = new LobbyUtilWidget();

        _deckWidget = new LobbyDeckWidget(_deckVm, OnCardClicked, OnDeckTabClicked);
        _collectionWidget = new LobbyCollectionWidget(
            _collectionVm, _utilWidget, OnCardClicked, mode => SelectMode = mode);
        _craftingWidget = new LobbyCraftingWidget(
            _craftingVm,
            _collectionVm,
            _utilWidget,
            () => _selectedCard = null,
            () => _selectedCard,
            () => ArrangeMode,
            mode => SelectMode = mode,
            routine => StartCoroutine(routine)
            ,OnCardClicked);
        _shopWidget = new LobbyShopWidget(_shopVm, _paymentService, InitUserInfo);
        _tutorialWidget = new LobbyTutorialWidget(_tutorialVm, GameObject.FindGameObjectsWithTag("Camera"));
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();

            await BindObjectsAsync();
            
            InitWidgetProperties();
            InitButtonEvents();
            
            _lobbyVm.SetCurrentPage(2);
            await InitMainLobby();
            await _lobbyVm.JoinLobby();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void Update()
    {
        if (_loadingMark != null)
        {
            if (_loadingMark.gameObject.activeSelf)
            {
                _loadingMark.Rotate(0, 0, 180 * Time.deltaTime);
            }
        }
        
        if (_lobbyVm.ChildScrolling) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            _lobbyVm.StartTouch(Input.mousePosition.x);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _lobbyVm.EndTouch(Input.mousePosition.x);
            StartCoroutine(nameof(OnSwipeOneStep), _lobbyVm.CurrentPage);
        }
    }

    private Task ShakeModeSelectButtons(int pageIndex)
    {
        var left = GetImage((int)Images.ModeSelectButtonLeftFrame).GetComponent<RectTransform>();
        var right = GetImage((int)Images.ModeSelectButtonRightFrame).GetComponent<RectTransform>();
        left.anchoredPosition = _leftButtonOriginalPos;
        right.anchoredPosition = _rightButtonOriginalPos;
        
        if (pageIndex == 2)
        {
            _shakeButtonRoutine ??= StartCoroutine(_utilWidget.ShakeModeSelectButtons(left, right));
        }
        else
        {
            if (_shakeButtonRoutine != null)
            {
                StopCoroutine(_shakeButtonRoutine);
                _shakeButtonRoutine = null;
            }
        }

        return Task.CompletedTask;
    }
    
    private Task UpdateScrollbar(int pageIndex)
    {
        scrollbar.value = _lobbyVm.GetScrollPageValue(pageIndex);
        return Task.CompletedTask;
    }
    
    private async Task UpdateEvents(int pageIndex)
    {
        if (pageIndex != 3) return;

        var events = await _lobbyVm.GetEventList();
        var noticeScrollView = GetImage((int)Images.EventScrollView).gameObject;
        var parent = Util.FindChild(noticeScrollView, "EventContents", true).transform;
        
        Util.DestroyAllChildren(parent);

        foreach (var eventInfo in events)
        {
            var frame = await Managers.Resource.Instantiate("UI/Lobby/Notice/EventFrame", parent);
            var titleText = Util.FindChild(frame, "EventTitleText", true).GetComponent<TextMeshProUGUI>();
            var infoText = Util.FindChild(frame, "EventContentText", true).GetComponent<TextMeshProUGUI>();
            var eventFrame = frame.GetComponent<EventFrame>();
            
            eventFrame.EventInfo = eventInfo;
            titleText.text = eventInfo.Title;
            infoText.text = eventInfo.Content;        
        }
    }

    private async Task UpdateNotices(int pageIndex)
    {
        if (pageIndex != 4) return;

        var notices = await _lobbyVm.GetNoticeList();
        var noticeScrollView = GetImage((int)Images.NoticeScrollView).gameObject;
        var parent = Util.FindChild(noticeScrollView, "NoticeContents", true).transform;
        
        Util.DestroyAllChildren(parent);
        
        foreach (var noticeInfo in notices)
        {
            await Managers.Resource.GetNoticeFrame(noticeInfo, parent);
        }
    }
    
    private void UpdateUsername()
    {
        GetText((int)Texts.UsernameText).text = User.Instance.UserInfo.UserName;
    }
    
    private IEnumerator OnSwipeOneStep(int index)
    {
        float start = scrollbar.value;
        float current = 0;
        float percent = 0;

        _lobbyVm.IsSwipeMode = true;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / _lobbyVm.SwipeTime;

            scrollbar.value = Mathf.Lerp(start, _lobbyVm.ScrollPageValues[index], percent);

            yield return null;
        }

        _lobbyVm.IsSwipeMode = false;
    }

    protected override async Task BindObjectsAsync()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        
        await Managers.Localization.UpdateTextAndFont(_textDict);
        await Managers.Localization.UpdateFont(GetText((int)Texts.UsernameText));
        
        var deckButtonText = GetButton((int)Buttons.DeckTabButton).GetComponent<TextMeshProUGUI>();
        var collectionButtonText = GetButton((int)Buttons.CollectionTabButton).GetComponent<TextMeshProUGUI>();
        var craftingButtonText = GetButton((int)Buttons.CraftingTabButton).GetComponent<TextMeshProUGUI>();
        
        await Managers.Localization.BindLocalizedText(deckButtonText, "deck_text");
        await Managers.Localization.BindLocalizedText(collectionButtonText, "collection_text");
        await Managers.Localization.BindLocalizedText(craftingButtonText, "crafting_text");
        await BindAssetHoldingLabelText(Util.Faction);
        
        _expSlider = GetImage((int)Images.ExpSliderBackground).transform.parent.GetComponent<Slider>();
        _loadingMark = GetImage((int)Images.LoadingMarkImage).GetComponent<RectTransform>();
        _leftButtonOriginalPos = GetImage((int)Images.ModeSelectButtonLeftFrame)
            .transform.parent.GetComponent<RectTransform>().anchoredPosition;
        _rightButtonOriginalPos = GetImage((int)Images.ModeSelectButtonRightFrame)
            .transform.parent.GetComponent<RectTransform>().anchoredPosition;
        
        _modes = new List<GameObject>
        {            
            GetImage((int)Images.FriendlyMatchPanel).gameObject,
            GetImage((int)Images.RankGamePanel).gameObject,
            GetImage((int)Images.SinglePlayPanel).gameObject
        };
        
        BindDictionary();
    }

    private void ChangeButtonFocus(int pageIndex)
    {
        var list = new List<string> { "ShopButton", "ItemButton", "GameButton", "EventButton", "ClanButton" };
        _utilWidget.SetBottomButton(list[pageIndex]);
    }

    // UI Size Adjustments

    #region UiAdjustment

    private async Task BindAssetHoldingLabelText(Faction faction)
    {
        var holdText = GetText((int)Texts.AssetHoldingLabelText);
        var notHoldText = GetText((int)Texts.AssetNotHoldingLabelText);
        string holdKey;
        string notHoldKey;   
        
        if (faction == Faction.Wolf)
        {
            holdKey = "asset_holding_label_text_wolf";
            notHoldKey = "asset_not_holding_label_text_wolf";
        }
        else
        {
            holdKey = "asset_holding_label_text_sheep";
            notHoldKey = "asset_not_holding_label_text_sheep";
        }
        
        await Managers.Localization.BindLocalizedText(holdText, holdKey);
        await Managers.Localization.BindLocalizedText(notHoldText, notHoldKey);
    }

    private void InitWidgetProperties()
    {
        _utilWidget.BindViews(
            _tabButtonDict,
            _arrangeButtonDict,
            _bottomButtonDict,
            _bottomButtonFocusDict);
        
        _deckWidget.BindViews(
            _deckButtonDict,
            _lobbyDeckButtonDict,
            GetImage((int)Images.Deck).transform,
            GetImage((int)Images.LobbyDeck).transform,
            GetImage((int)Images.BattleSettingPanel).transform);
        
        _collectionWidget.BindViews(
            GetImage((int)Images.UnitHoldingCardPanel).transform,
            GetImage((int)Images.UnitNotHoldingCardPanel).transform,
            GetImage((int)Images.AssetHoldingCardPanel).transform,
            GetImage((int)Images.AssetNotHoldingCardPanel).transform,
            GetImage((int)Images.CharacterHoldingCardPanel).transform,
            GetImage((int)Images.CharacterNotHoldingCardPanel).transform,
            GetImage((int)Images.MaterialHoldingPanel).transform,
            GetImage((int)Images.CraftingPanel).GetComponent<RectTransform>(),
            GetImage((int)Images.CollectionScrollView).gameObject,
            GetButton((int)Buttons.ArrangeAllButton).gameObject,
            _collectionUiDict);
        
        _craftingWidget.BindViews(_collectionUiDict, _craftingUiDict, 
            GetImage((int)Images.DeckScrollView).gameObject,
            GetImage((int)Images.CollectionScrollView).gameObject,
            GetImage((int)Images.CollectionScrollView).GetComponent<ScrollRect>(),
            GetImage((int)Images.CraftingPanel).GetComponent<RectTransform>(),
            GetImage((int)Images.CraftingCardPanel).GetComponent<RectTransform>(),
            GetImage((int)Images.CraftCardPanel).transform,
            GetImage((int)Images.MaterialPanel).transform,
            GetImage((int)Images.ReinforceCardPanel).transform,
            GetImage((int)Images.ReinforceResultPanel).transform,
            GetImage((int)Images.UnitHoldingCardPanel).transform,
            GetImage((int)Images.ArrowPanel).transform,
            GetButton((int)Buttons.ReinforceButton),
            GetButton((int)Buttons.ReinforcingButton),
            GetButton((int)Buttons.CraftingButton),
            GetButton((int)Buttons.RecyclingButton),
            GetText((int)Texts.CraftCountText),
            GetText((int)Texts.ReinforceCardNumberText),
            GetText((int)Texts.SuccessRateText));
        
        var adsRemover = GetButton((int)Buttons.AdsRemover).GetOrAddComponent<AdsRemover>();
        _shopWidget.BindViews(GetImage((int)Images.SpecialPackagePanel).transform,
            GetImage((int)Images.BeginnerPackagePanel).transform,
            GetImage((int)Images.ReservedSalePanel).transform,
            GetImage((int)Images.DailyDealProductPanel).transform,
            GetImage((int)Images.GoldStorePanel).transform,
            GetImage((int)Images.SpinelStorePanel).transform,
            GetImage((int)Images.GoldPackagePanel).transform,
            GetImage((int)Images.SpinelPackagePanel).transform,
            adsRemover,
            GetButton((int)Buttons.DailyProductsRefreshButton),
            GetText((int)Texts.DailyProductsRefreshButtonTimeText));
    }
    
    protected override void InitButtonEvents()
    {
        foreach (var gameModePanel in _modes)
        {
            gameModePanel.BindEvent(OnModeButtonClicked);
        }
        
        foreach (var pair in _bottomButtonDict)
        {
            pair.Value.BindEvent(OnBottomButtonClicked);
        }
        
        foreach (var pair in _deckButtonDict)
        {
            pair.Value.BindEvent(_deckWidget.OnDeckButtonClicked);
        }

        foreach (var pair in _lobbyDeckButtonDict)
        {
            pair.Value.BindEvent(_deckWidget.OnDeckButtonClicked);
        }
        
        GetButton((int)Buttons.ProfilePanelButton).gameObject.BindEvent(OnProfileClicked);
        GetButton((int)Buttons.SettingsButton).gameObject.BindEvent(OnSettingsClicked);
        GetButton((int)Buttons.FriendsButton).gameObject.BindEvent(OnFriendsClicked);
        GetButton((int)Buttons.MailButton).gameObject.BindEvent(OnMailClicked);
        GetButton((int)Buttons.DiscordButton).gameObject.BindEvent(OnDiscordClicked);
        GetButton((int)Buttons.FactionButton).gameObject.BindEvent(OnFactionClicked);
        GetButton((int)Buttons.GoldButton).onClick.AddListener(OnResourceClicked);
        GetButton((int)Buttons.SpinelButton).onClick.AddListener(OnResourceClicked);
        GetButton((int)Buttons.PlayButton).gameObject.BindEvent(OnPlayButtonClicked);
        GetButton((int)Buttons.ModeSelectButtonLeft).gameObject
            .BindEvent(data => OnModeSelectButtonClicked(data, 1));
        GetButton((int)Buttons.ModeSelectButtonRight).gameObject
            .BindEvent(data => OnModeSelectButtonClicked(data, -1));
        
        GetButton((int)Buttons.DeckTabButton).gameObject.BindEvent(OnDeckTabClicked);
        GetButton((int)Buttons.CollectionTabButton).gameObject.BindEvent(OnCollectionTabClicked);
        
        GetButton((int)Buttons.ArrangeAllButton).gameObject.BindEvent(OnArrangeAllClicked);
        GetButton((int)Buttons.ArrangeSummaryButton).gameObject.BindEvent(OnArrangeSummaryClicked);
        GetButton((int)Buttons.ArrangeClassButton).gameObject.BindEvent(OnArrangeClassClicked);
        GetButton((int)Buttons.ArrangeCountButton).gameObject.BindEvent(OnArrangeCountClicked);
        
        GetButton((int)Buttons.CraftingTabButton).onClick.AddListener(_craftingWidget.OnCraftingTabClicked);
        
        GetButton((int)Buttons.CraftingBackButton).onClick.AddListener(_craftingWidget.OnCraftingBackClicked);
        GetButton((int)Buttons.CraftingButton).gameObject.BindEvent(_craftingWidget.OnCraftingClicked);
        GetButton((int)Buttons.CraftButton).onClick
            .AddListener(() => _ = _craftingWidget.OnCraftClicked());
        GetButton((int)Buttons.CraftUpperArrowButton).gameObject.BindEvent(_craftingWidget.OnCraftUpperArrowClicked);
        GetButton((int)Buttons.CraftLowerArrowButton).gameObject.BindEvent(_craftingWidget.OnCraftLowerArrowClicked);
        
        GetButton((int)Buttons.ReinforcingButton).gameObject.BindEvent(_craftingWidget.OnReinforcingClicked);
        GetButton((int)Buttons.ReinforceButton).onClick
            .AddListener(() => _ = _craftingWidget.OnReinforceClicked());
        
        GetButton((int)Buttons.RecyclingButton).onClick
            .AddListener(() => _ = _craftingWidget.OnRecyclingClicked());
        GetButton((int)Buttons.RestorePurchaseButton).gameObject.BindEvent(_shopWidget.OnRestorePurchaseClicked);
    }
    
    private async Task InitMainLobby()
    {
        await SwitchLobbyUI(Util.Faction);
        GetImage((int)Images.FriendAlertIcon).gameObject.SetActive(false);
        GetImage((int)Images.MailAlertIcon).gameObject.SetActive(false);

        // 이미지 크기 조정을 위해 캔버스 강제 업데이트
        Canvas.ForceUpdateCanvases();
        _utilWidget.SetBottomButton("GameButton");
        
        // Init GameMode
        _gameMode = Define.GameMode.RankGame;
        var go = _modes.FirstOrDefault(go => go.name.Contains(_gameMode.ToString()));
        if (go != null)
        {
            var iconRect = go.transform.GetChild(0).GetComponent<RectTransform>();
            Util.FindChild(go, $"{go.name}Text", false, true).SetActive(true);
            iconRect.anchorMin = new Vector2(iconRect.anchorMin.x, 0.66f);
            iconRect.anchorMax = new Vector2(iconRect.anchorMax.x, 0.66f);
        }

        await InitUserInfo();
        await InitMainLobbyItems();
        GetImage((int)Images.LoadingPanel).gameObject.SetActive(false);
        
        var sheepTutorialDone = _userService.TutorialInfo.SheepTutorialDone;
        var wolfTutorialDone = _userService.TutorialInfo.WolfTutorialDone;
        var changeFactionTutorialDone = _userService.TutorialInfo.ChangeFactionTutorialDone;
        
        await Managers.Policy.EnsureConsentsAndInitAdsAsync();
        
        if (!sheepTutorialDone || !wolfTutorialDone || !changeFactionTutorialDone)
        {
            await _tutorialWidget.ProcessTutorial(_userService.TutorialInfo);
        }
        
        if (User.Instance.IsGuest && !User.Instance.GuestPopupShown)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            const string titleKey = "warning_text";
            const string messageKey = "notify_warning_guest_account_message";
            await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
            
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Close()
            {
                if (User.Instance.IsGuest) User.Instance.GuestPopupShown = true;
                tcs.TrySetResult(true);
                Managers.UI.ClosePopupUI(popup);
            }

            popup.SetYesCallback(Close);
            popup.SetExitCallback(Close);
            
            await tcs.Task;
        }
    }

    private async Task InitMainLobbyItems()
    {
        await Task.WhenAll(
            _collectionWidget.InitCollection(),
            _deckWidget.InitDeck(),
            _shopWidget.InitShop(),
            _lobbyVm.InitFriendAlert(),
            _lobbyVm.InitMailAlert(),
            _lobbyVm.ConnectSignalR(User.Instance.UserInfo.UserTag)
        );
    }

    private async Task InitUserInfo()
    {
        await _userService.LoadUserInfo();
        BindUserInfo();
    }
    
    private async Task InitMailAlert()
    {
        await _lobbyVm.InitMailAlert();
    }

    private async Task OnLanguageChanged()
    {
        _shopWidget.DestroyShop();
        await Managers.Localization.UpdateTextAndFont(_textDict);
        
        var deckButtonText = GetButton((int)Buttons.DeckTabButton).GetComponent<TextMeshProUGUI>();
        var collectionButtonText = GetButton((int)Buttons.CollectionTabButton).GetComponent<TextMeshProUGUI>();
        var craftingButtonText = GetButton((int)Buttons.CraftingTabButton).GetComponent<TextMeshProUGUI>();
        
        await Managers.Localization.BindLocalizedText(deckButtonText, "deck_text");
        await Managers.Localization.BindLocalizedText(collectionButtonText, "collection_text");
        await Managers.Localization.BindLocalizedText(craftingButtonText, "crafting_text");
        
        await _shopWidget.InitShop();
    }
    
    private void BindUserInfo()
    {
        var userInfo = User.Instance.UserInfo;
        var exp = userInfo.Exp;
        var expMax = userInfo.ExpToLevelUp;

        _expSlider.value = exp / (float)expMax;
        
        _textDict["GoldText"].GetComponent<TextMeshProUGUI>().text = userInfo.Gold.ToString();
        _textDict["SpinelText"].GetComponent<TextMeshProUGUI>().text = userInfo.Spinel.ToString();
        _textDict["LevelText"].GetComponent<TextMeshProUGUI>().text = userInfo.Level.ToString();
        _textDict["UsernameText"].GetComponent<TextMeshProUGUI>().text = userInfo.UserName;
        _textDict["RankText"].GetComponent<TextMeshProUGUI>().text = userInfo.RankPoint.ToString();
        _textDict["ExpText"].GetComponent<TextMeshProUGUI>().text = $"{exp.ToString()} / {expMax.ToString()}";
    }
    
    private async Task SwitchLobbyUI(Faction faction)
    {
        GetImage((int)Images.TopPanel).color = Util.ThemeColor;
        GetImage((int)Images.GameImageGlow).color = Util.ThemeColor;
        GetImage((int)Images.ItemBackground).color = Util.ThemeColor;
        GetImage((int)Images.ShopBackground).color = Util.ThemeColor;
        GetImage((int)Images.EventBackground).color = Util.ThemeColor;
        GetImage((int)Images.ClanBackground).color = Util.ThemeColor;
        
        var gamePanelImage = GetImage((int)Images.GamePanelBackground);
        var factionButtonIcon = GetImage((int)Images.FactionButtonIcon);
        
        switch (faction)
        {
            case Faction.Sheep:
                gamePanelImage.sprite = await Managers.Resource.LoadAsync<Sprite>("Sprites/Backgrounds/MainLobbySheep");
                factionButtonIcon.sprite = await Managers.Resource.LoadAsync<Sprite>("Sprites/SheepButton");
                break;
            case Faction.Wolf:
                gamePanelImage.sprite = await Managers.Resource.LoadAsync<Sprite>("Sprites/Backgrounds/MainLobbyWolf");
                factionButtonIcon.sprite = await Managers.Resource.LoadAsync<Sprite>("Sprites/WolfButton");
                break;
        }

        await BindAssetHoldingLabelText(faction);
    }

    private async Task SetCardPopupUI(Card card)
    {
        _selectedCard = card;
        CardPopup = await Managers.UI.ShowPopupUI<UI_CardClickPopup>();
        CardPopup.SelectedCard = _selectedCard;
        CardPopup.CardPosition = _selectedCard.transform.position - new Vector3(0, 60);
        CardPopup.FromDeck = card.gameObject.transform.parent == GetImage((int)Images.Deck).transform 
                             || card.gameObject.transform.parent == GetImage((int)Images.BattleSettingPanel).transform;
        CardPopup.SelectMode = SelectMode;
    }

    private async Task<bool> VerifyCard(UnitInfo unitInfo)
    {
        if (_craftingVm.VerityCardByCondition1(unitInfo) == false)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
            await Managers.Localization.UpdateWarningPopupText(popup, "warning_cards_cannot_be_used_as_materials");
            return false;
        }
            
        if (_craftingVm.VerifyCardByCondition2(unitInfo) == false)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
            await Managers.Localization.UpdateWarningPopupText(popup, "warning_keep_minimum_cards");
            return false;
        }
            
        if (_craftingVm.VerifyCardByCondition3(unitInfo) == false)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
            await Managers.Localization.UpdateWarningPopupText(popup, "no_cards_available");
            return false;
        }

        return true;
    }
    
    private void OnFriendAlert()
    {
        GetImage((int)Images.FriendAlertIcon).gameObject.SetActive(true);
    }
    
    private void OffFriendAlert()
    {
        GetImage((int)Images.FriendAlertIcon).gameObject.SetActive(false);
    }
    
    private void OnMailAlert()
    {
        GetImage((int)Images.MailAlertIcon).gameObject.SetActive(true);
    }
    
    private void OffMailAlert()
    {
        GetImage((int)Images.MailAlertIcon).gameObject.SetActive(false);
    }
    
    #endregion
    
    // Touch Events
    #region TouchEvent

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CardPopup != null) Managers.UI.ClosePopupUI(CardPopup);
    }

    #endregion
    
    // Button Click Events
    #region ButtonEvent

    private async Task OnFactionClicked(PointerEventData data)
    {
        Util.Faction = Util.Faction == Faction.Sheep ? Faction.Wolf : Faction.Sheep;
        
        _deckVm.SwitchDeck(Util.Faction);
        _collectionVm.SwitchCards(Util.Faction);
        
        _selectedCard = null;
        var selectMode = SelectMode;
        _craftingWidget.InitCraftingPanel();
        SelectMode = selectMode;
        
        await SwitchLobbyUI(Util.Faction);
    }

    private void OnResourceClicked()
    {
        var shopScrollbar = GetImage((int)Images.ShopScrollbar).GetComponent<Scrollbar>();
        _lobbyVm.SetCurrentPage(0);
        shopScrollbar.value = 0;
    }
    
    private async Task OnProfileClicked(PointerEventData data)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_PlayerProfilePopup>();
        popup.PlayerUserInfo = User.Instance.UserInfo;
    }
    
    private async Task OnSettingsClicked(PointerEventData data)
    {
        await Managers.UI.ShowPopupUI<UI_SettingsPopup>();
    }
    
    private async Task OnFriendsClicked(PointerEventData data)
    {
        await Managers.UI.ShowPopupUI<UI_FriendsListPopup>();
    }
    
    private async Task OnMailClicked(PointerEventData data)
    {
        await Managers.UI.ShowPopupUI<UI_MailBoxPopup>();
    }

    private async Task OnDiscordClicked(PointerEventData data)
    {
        Application.OpenURL(DiscordInviteUrl);
        await Task.CompletedTask;
    }
    
    private void OnMissionClicked(PointerEventData data)
    {
        
    }
    
    private void OnGiftClicked(PointerEventData data)
    {
        
    }
    
    private void OnPlayButtonClicked(PointerEventData data)
    {
        _lobbyVm.OnPlayButtonClicked(_gameMode);
    }

    private void OnModeSelectButtonClicked(PointerEventData data, int direction)
    {
        _currentModeIndex = (_currentModeIndex + direction + _modes.Count) % _modes.Count;
        _gameMode = (Define.GameMode)_currentModeIndex;
        StartCoroutine(_utilWidget.MoveModeIcons(_modes, _currentModeIndex));
    }

    private void OnModeButtonClicked(PointerEventData data)
    {
        Enum.TryParse<Define.GameMode>(data.pointerPress.gameObject.name.Replace("Panel", ""), out var mode);
        _currentModeIndex = (int)mode;
        _gameMode = mode;
        StartCoroutine(_utilWidget.MoveModeIcons(_modes, _currentModeIndex));
    }
    
    private void OnBottomButtonClicked(PointerEventData data)
    {
        switch (data.pointerPress.name)
        {
            case "ShopButton":
                _lobbyVm.SetCurrentPage(0);
                break;
            case "ItemButton":
                _lobbyVm.SetCurrentPage(1);
                break;
            case "GameButton":
                _lobbyVm.SetCurrentPage(2);
                break;
            case "EventButton":
                _lobbyVm.SetCurrentPage(3);
                break;
            case "ClanButton":
                _lobbyVm.SetCurrentPage(4);
                break;
        }
        
        _utilWidget.SetBottomButton(data.pointerPress.name);
    }
    
    private void OnDeckTabClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        _lobbyVm.SetCurrentPage(1);
        _utilWidget.FocusTabButton("DeckTabButton");
        _craftingWidget.CloseCraftingPanel();
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(false);
        GetImage((int)Images.DeckScrollView).gameObject.SetActive(true);
    }

    private void OnCollectionTabClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        _utilWidget.FocusTabButton("CollectionTabButton");
        _craftingWidget.CloseCraftingPanel();
        GetImage((int)Images.DeckScrollView).gameObject.SetActive(false);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(true);
    }

    private void OnArrangeAllClicked(PointerEventData data)
    {
        ArrangeMode = Define.ArrangeMode.All;
        _utilWidget.SetArrangeButtonColor("ArrangeAllButton");
    }

    private void OnArrangeSummaryClicked(PointerEventData data)
    {
        ArrangeMode = Define.ArrangeMode.Summary;
        _utilWidget.SetArrangeButtonColor("ArrangeSummaryButton");
    }
    
    private void OnArrangeClassClicked(PointerEventData data)
    {
        ArrangeMode = Define.ArrangeMode.Class;
        _utilWidget.SetArrangeButtonColor("ArrangeClassButton");
    }
    
    private void OnArrangeCountClicked(PointerEventData data)
    {
        ArrangeMode = Define.ArrangeMode.Count;
        _utilWidget.SetArrangeButtonColor("ArrangeCountButton");
    }
    
    private async void OnCardClicked(PointerEventData data)
    {
        try
        {
            if (data.pointerPress.TryGetComponent(out Card card) == false) return;
            if (card.IsDragging) return;

            if (SelectMode == Define.SelectMode.Normal)
            {
                await SetCardPopupUI(card);
            }
            else if (SelectMode is Define.SelectMode.Reinforce)
            {
                var unitInfo = Managers.Data.UnitInfoDict[card.Id];
                var verifyResult = await VerifyCard(unitInfo);
                if (verifyResult == false) return;
            
                _craftingVm.AddNewUnitMaterial(unitInfo);
            
                var parent = GetImage((int)Images.MaterialPanel).transform;
                var cardFrame = await Managers.Resource.GetCardResources<UnitId>(
                    unitInfo, parent, _craftingWidget.OnReinforceMaterialClicked);
            
                Util.FindChild(cardFrame, "Role").SetActive(false);
                _craftingWidget.UpdateReinforcePanel();
                await _craftingWidget.ResetCollectionUIForReinforce();
                return;
            }
            else if (SelectMode is Define.SelectMode.Recycle)
            {
                // Add Card into recycle scroll view.
            }
            else
            {
                _selectedCard = card;
                _craftingVm.SetCard(card);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    #endregion

    private void BindEvents()
    {
        _lobbyVm.OnInitUserInfo += InitUserInfo;
        _lobbyVm.OnFriendRequestNotificationReceived += OnFriendAlert;
        _lobbyVm.OnFriendRequestNotificationOff += OffFriendAlert;
        _lobbyVm.OnMailAlert += OnMailAlert;
        _lobbyVm.OffMailAlert += OffMailAlert;
        _lobbyVm.OnPageChanged += ShakeModeSelectButtons;
        _lobbyVm.OnPageChanged += UpdateScrollbar;
        _lobbyVm.OnPageChanged += UpdateEvents;
        _lobbyVm.OnPageChanged += UpdateNotices;
        _lobbyVm.OnUpdateUsername += UpdateUsername;
        _lobbyVm.OnChangeButtonFocus += ChangeButtonFocus;
        _lobbyVm.OnChangeLanguage += OnLanguageChanged;
        
        _paymentService.OnPaymentSuccess += InitMailAlert;
        _paymentService.OnPaymentSuccess += InitUserInfo;
        _paymentService.OnPaymentSuccess += _collectionVm.LoadCollection;
        _paymentService.OnCashPaymentSuccess += InitMailAlert;
        _paymentService.OnCashPaymentSuccess += InitUserInfo;
        
        _craftingVm.SetCollectionUI += _collectionWidget.SetCollectionUI;
        
        _userService.InitDeckButton += _deckWidget.OnInitDeckButton;
        
    }

    private void ReleaseEvents()
    {
        _lobbyVm.OnInitUserInfo -= InitUserInfo;
        _lobbyVm.OnFriendRequestNotificationReceived -= OnFriendAlert;
        _lobbyVm.OnFriendRequestNotificationOff -= OffFriendAlert;
        _lobbyVm.OnMailAlert -= OnMailAlert;
        _lobbyVm.OffMailAlert -= OffMailAlert;
        _lobbyVm.OnPageChanged -= ShakeModeSelectButtons;
        _lobbyVm.OnPageChanged -= UpdateScrollbar;
        _lobbyVm.OnPageChanged -= UpdateEvents;
        _lobbyVm.OnPageChanged -= UpdateNotices;
        _lobbyVm.OnUpdateUsername -= UpdateUsername;
        _lobbyVm.OnChangeButtonFocus -= ChangeButtonFocus;
        _lobbyVm.OnChangeLanguage -= OnLanguageChanged;
        
        _paymentService.OnPaymentSuccess -= InitMailAlert;
        _paymentService.OnPaymentSuccess -= InitUserInfo;
        _paymentService.OnPaymentSuccess -= _collectionVm.LoadCollection;
        _paymentService.OnCashPaymentSuccess -= InitMailAlert;
        _paymentService.OnCashPaymentSuccess -= InitUserInfo;
        
        _craftingVm.SetCollectionUI -= _collectionWidget.SetCollectionUI;
        _userService.InitDeckButton -= _deckWidget.OnInitDeckButton;
    }
    
    private async void OnDestroy()
    {
        try
        {
            ReleaseEvents();
            await _lobbyVm.LeaveLobby();
            DisposeWidgets();
            _lobbyVm.Dispose();
        }
        catch (Exception e)
        {
            Debug.LogError($"Main Lobby Destroy Error: {e}");
        }
    }
    
    private void DisposeWidgets()
    {
        _tutorialWidget?.Dispose();
        _deckWidget?.Dispose();
        _collectionWidget?.Dispose();
        _craftingWidget?.Dispose();
        _utilWidget?.Dispose();
    }
}
