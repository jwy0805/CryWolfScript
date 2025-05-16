using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Google.Protobuf.Protocol;
using TMPro;
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
    
    private IUserService _userService;
    private ITokenService _tokenService;
    private IPaymentService _paymentService;
    private MainLobbyViewModel _lobbyVm;
    private DeckViewModel _deckVm;
    private CollectionViewModel _collectionVm;
    private CraftingViewModel _craftingVm;
    private TutorialViewModel _tutorialVm;
    private ShopViewModel _shopVm;

    private readonly float _modeChangeTime = 0.25f;

    private Slider _expSlider;
    private int _currentModeIndex;
    private bool _isCraftingPanelOpen;
    private Card _selectedCard;
    private Card _selectedCardForCrafting;
    private UI_CardClickPopup _cardPopup;
    private RectTransform _craftingPanel;
    private ScrollRect _craftingScrollRect;
    private Camera _tutorialCamera1;
    private Camera _tutorialCamera2;
    private Transform _unitCollection;
    private Transform _unitNoCollection;
    private Transform _assetCollection;
    private Transform _assetNoCollection;
    private Transform _characterCollection;
    private Transform _characterNoCollection;
    private Transform _materialCollection;
    private Transform _specialPackagePanel;
    private Transform _beginnerPackagePanel;
    private Transform _reservedSalePanel;
    private Transform _dailyProductPanel;
    private Transform _goldStorePanel;
    private Transform _spinelStorePanel;
    private Transform _spinelPackagePanel;
    private Transform _goldPackagePanel;
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
    private SelectModeEnums _selectMode;
    private GameModeEnums _gameMode;
    private ArrangeModeEnums _arrangeMode = ArrangeModeEnums.All;
    
    private SelectModeEnums SelectMode
    {
        get => _selectMode;
        set
        {
            if (_selectMode is SelectModeEnums.Reinforce or SelectModeEnums.Recycle)
            {
                SetCollectionUIDetails(Util.Faction);
            }
            _selectMode = value;
        }
    }
    
    private ArrangeModeEnums ArrangeMode
    {
        get => _arrangeMode;
        set
        {
            _arrangeMode = value;
            
            if (SelectMode == SelectModeEnums.Reinforce)
            {
                ResetCollectionUIForReinforce();
            }
            else
            {
                SetCollectionUIDetails(Util.Faction);
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
    
    #region Enums

    public enum GameModeEnums
    {
        RankGame = 0,
        FriendlyMatch = 1,
        SinglePlay = 2,
    }
    
    private enum SelectModeEnums
    {
        Normal,
        Reinforce,
        Recycle
    }
    
    private enum ArrangeModeEnums
    {
        All,
        Summary,
        Class,
        Count
    }
    
    private enum Buttons
    {
        ShopButton,
        ShopButtonFocus,
        ItemButton,
        ItemButtonFocus,
        GameButton,
        GameButtonFocus,
        EventButton,
        EventButtonFocus,
        ClanButton,
        ClanButtonFocus,
        
        FactionButton,
        
        ProfilePanelButton,
        SettingsButton,
        FriendsButton,
        MailButton,
        // MissionButton,
        // GiftButton,
        
        PlayButton,
        ModeSelectButtonLeft,
        ModeSelectButtonRight,
        
        LobbyDeckButton1,
        LobbyDeckButton2,
        LobbyDeckButton3,
        LobbyDeckButton4,
        LobbyDeckButton5,
        
        DeckTabButton,
        DeckButton1,
        DeckButton2,
        DeckButton3,
        DeckButton4,
        DeckButton5,
        
        CollectionTabButton,
        
        ArrangeAllButton,
        ArrangeSummaryButton,
        ArrangeClassButton,
        ArrangeCountButton,
        
        CraftingBackButton,
        CraftingTabButton,
        CraftingButton,
        CraftUpperArrowButton,
        CraftLowerArrowButton,
        CraftButton,
        
        ReinforcingButton,
        ReinforceButton,
        
        RecyclingButton,
        
        DailyProductsRefreshButton,
        AdsRemover
    }

    private enum Texts
    {
        GoldText,
        SpinelText,
        LevelText,
        ExpText,
        
        UsernameText,
        RankText,
        
        MainSettingsButtonText,
        MainFriendsButtonText,
        MainMailButtonText,
        MainMissionButtonText,
        MainGiftButtonText,
        PlayButtonText,
        FriendlyMatchPanelText,
        RankGamePanelText,
        SinglePlayPanelText,
        MainShopButtonText,
        MainItemButtonText,
        MainBattleButtonText,
        MainEventButtonText,
        MainClanButtonText,
        
        MainDeckLabelText,
        MainBattleSettingLabelText,
        MainCraftingButtonText,
        MainReinforcingButtonText,
        MainRecyclingButtonText,
        MainCraftText,
        CraftCountText,
        MainTempText,
        ReinforceCardSelectText,
        ReinforceCardNumberText,
        SuccessRateText,
        SuccessText,
        
        HoldingLabelText,
        NotHoldingLabelText,
        AssetHoldingLabelText,
        AssetNotHoldingLabelText,
        CharacterHoldingLabelText,
        CharacterNotHoldingLabelText,
        MaterialHoldingLabelText,
        
        SpecialDealText,
        SpecialPackageLabelText,
        BeginnerPackageLabelText,
        ReservedSaleLabelText,
        DailyDealLabelText,
        SpinelStoreLabelText,
        GoldStoreLabelText,
        GoldPackageLabelText,
        SpinelPackageLabelText,
        
        DailyProductsRefreshButtonText,
        DailyProductsRefreshButtonTimeText,
    }

    private enum Images
    {
        TopPanel,
        GamePanelBackground,
        GameImageGlow,
        LobbyDeck,
        
        FactionButtonIcon,
        ExpSliderBackground,
        
        FriendAlertIcon,
        MailAlertIcon,
        
        RankGamePanel,
        FriendlyMatchPanel,
        SinglePlayPanel,
        
        ShopBackground,
        ItemBackground,
        EventBackground,
        ClanBackground,
        
        DeckScrollView,
        CollectionScrollView,
        Deck,
        
        ShopPanel,
        UnitHoldingCardPanel,
        UnitNotHoldingCardPanel,
        AssetHoldingCardPanel,
        AssetNotHoldingCardPanel,
        CharacterHoldingCardPanel,
        CharacterNotHoldingCardPanel,
        MaterialHoldingPanel,
        
        UnitHoldingLabelPanel,
        UnitNotHoldingLabelPanel,
        AssetHoldingLabelPanel,
        AssetNotHoldingLabelPanel,
        CharacterHoldingLabelPanel,
        CharacterNotHoldingLabelPanel,
        MaterialHoldingLabelPanel,
        
        BattleSettingPanel,
        CraftingPanel,
        
        CraftingCardPanel,
        CardPedestal,
        
        CraftingBackButtonFakePanel,
        CraftingSelectPanel,
        CraftingCraftPanel,
        CraftCardPanel,
        
        CraftingReinforcePanel,
        ReinforceCardPanel,
        ArrowPanel,
        ReinforceResultPanel,
        
        CraftingRecyclePanel,
        
        MaterialScrollView,
        MaterialPanel,
        
        BeginnerPackagePanel,
        SpecialPackagePanel,
        ReservedSalePanel,
        DailyDealPanel,
        DailyDealProductPanel,
        SpinelStorePanel,
        GoldStorePanel,
        GoldPackagePanel,
        SpinelPackagePanel,
    }

    #endregion

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
        _lobbyVm.Initialize(Util.FindChild(gameObject, "HorizontalContents", true)
            .transform.childCount);

        _lobbyVm.OnFriendRequestNotificationReceived += OnFriendAlert;
        _lobbyVm.OnFriendRequestNotificationOff += OffFriendAlert;
        _lobbyVm.OnMailAlert += OnMailAlert;
        _lobbyVm.OffMailAlert += OffMailAlert;
        _lobbyVm.OnPageChanged += UpdateScrollbar;
        _lobbyVm.OnChangeButtonFocus += ChangeButtonFocus;
        _lobbyVm.OnChangeLanguage += ChangeLanguage;

        _paymentService.OnPaymentSuccess += OnMailAlert;
        _paymentService.OnCashPaymentSuccess += OnMailAlert;
        
        _deckVm.OnDeckInitialized += SetDeckUI;
        _deckVm.OnDeckSwitched += SetDeckButtonUI;
        _deckVm.OnDeckSwitched += ResetDeckUI;
        
        _collectionVm.OnCardInitialized += SetCollectionUI;
        _collectionVm.OnCardSwitched += SwitchCollection;

        _craftingVm.SetCardOnCraftingPanel += SetCardOnCraftingPanel;
        _craftingVm.SetMaterialsOnCraftPanel += InitMaterialsOnCraftPanel;
        _craftingVm.InitCraftingPanel += InitCraftingPanel;
        _craftingVm.SetCollectionUI += SetCollectionUI;
        
        _tutorialVm.OnInitTutorialCamera1 += InitTutorialMainCamera1;
        _tutorialVm.OnInitTutorialCamera2 += InitTutorialMainCamera2;
        
        _userService.InitDeckButton += SetDeckButtonUI;
        
        Managers.Ads.OnRewardedCheckDailyProduct += RevealDailyProduct;
        Managers.Ads.OnRewardedRefreshDailyProducts += RefreshDailyProducts;
    }

    protected override async void Init()
    {
        try
        {
            base.Init();

            BindObjects();
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

    private void UpdateScrollbar(int pageIndex)
    {
        scrollbar.value = _lobbyVm.GetScrollPageValue(pageIndex);
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

    private void ChangeLanguage(string language2Letter)
    {
        Managers.Localization.UpdateChangedTextAndFont(_textDict, language2Letter);
        var shopPanel = GetImage((int)Images.ShopPanel);
        
        foreach (var product in shopPanel.GetComponentsInChildren<ProductSimple>())
        {
            product.SetProductText();
        }
        
        foreach (var product in shopPanel.GetComponentsInChildren<ProductPackage>())
        {
            product.SetProductText();
        }
    }
    
    private void ChangeButtonFocus(int pageIndex)
    {
        var list = new List<string> { "ShopButton", "ItemButton", "GameButton", "EventButton", "ClanButton" };
        SetBottomButton(list[pageIndex]);
    }

    private IEnumerator MoveModeIcons()
    {
        var startAnchors = new Vector2[_modes.Count];
        var targetAnchors = new Vector2[_modes.Count];
        var anchorPositions = new Vector2[] { new(0.17f, 0.17f), new(0.5f, 0.5f), new(0.83f, 0.83f) };

        for (var i = 0; i < _modes.Count; i++)
        {
            var targetIndex = (_currentModeIndex + i) % _modes.Count;
            var rect = _modes[i].GetComponent<RectTransform>();
            startAnchors[i] = new Vector2(rect.anchorMin.x, rect.anchorMin.y);
            targetAnchors[i] = anchorPositions[targetIndex];
        }

        float elapsedTime = 0;
        
        while (elapsedTime < _modeChangeTime)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / _modeChangeTime);

            for (var i = 0; i < _modes.Count; i++)
            {
                var newX = Mathf.Lerp(startAnchors[i].x, targetAnchors[i].x, t);
                var rect = _modes[i].GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(newX, rect.anchorMin.y);
                rect.anchorMax = new Vector2(newX, rect.anchorMax.y);
            }

            yield return null;
        }

        for (var i = 0; i < _modes.Count; i++)
        {
            var rect = _modes[i].GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(targetAnchors[i].x, rect.anchorMin.y);
            rect.anchorMax = new Vector2(targetAnchors[i].y, rect.anchorMax.y);
        }

        foreach (var mode in _modes)
        {
            var iconRect = mode.transform.GetChild(0).GetComponent<RectTransform>();
            
            if (Mathf.Approximately(mode.GetComponent<RectTransform>().anchorMin.x, 0.5f))
            {
                Util.FindChild(mode, $"{mode.name}Text", false, true).SetActive(true);
                iconRect.anchorMin = new Vector2(iconRect.anchorMin.x, 0.66f);
                iconRect.anchorMax = new Vector2(iconRect.anchorMax.x, 0.66f);
            }
            else
            {
                iconRect.anchorMin = new Vector2(iconRect.anchorMin.x, 0.5f);
                iconRect.anchorMax = new Vector2(iconRect.anchorMax.x, 0.5f);
                var go = Util.FindChild(mode, $"{mode.name}Text", false, true);
                if (go != null) go.SetActive(false);
            }
        }
    }
    
    #region OrderingMethods

    private List<OwnedUnitInfo> OrderOwnedUnits()
    {
        switch (ArrangeMode)
        {
            default:
            case ArrangeModeEnums.All:
                return User.Instance.OwnedUnitList
                    .Where(info => info.UnitInfo.Faction == Util.Faction)
                    .OrderBy(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id).ToList();
            case ArrangeModeEnums.Summary:
                return User.Instance.OwnedUnitList
                    .Where(info => info.UnitInfo.Faction == Util.Faction)
                    .GroupBy(info => info.UnitInfo.Species)
                    .Select(group => group.OrderByDescending(info => info.UnitInfo.Level).First())
                    .ToList();
            case ArrangeModeEnums.Class:
                return User.Instance.OwnedUnitList
                    .Where(info => info.UnitInfo.Faction == Util.Faction)
                    .OrderByDescending(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id).ToList();
            case ArrangeModeEnums.Count:
                return User.Instance.OwnedUnitList
                    .Where(info => info.UnitInfo.Faction == Util.Faction)
                    .OrderByDescending(info => info.Count)
                    .ThenBy(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id).ToList();
        }
    }

    private List<OwnedSheepInfo> OrderOwnedSheep()
    {
        switch (ArrangeMode)
        {
            default:
            case ArrangeModeEnums.All:
            case ArrangeModeEnums.Summary:
                return User.Instance.OwnedSheepList
                    .OrderBy(info => info.SheepInfo.Class)
                    .ThenBy(info => info.SheepInfo.Id).ToList();
            case ArrangeModeEnums.Class:
                return User.Instance.OwnedSheepList
                    .OrderByDescending(info => info.SheepInfo.Class)
                    .ThenBy(info => info.SheepInfo.Id).ToList();
            case ArrangeModeEnums.Count:
                return User.Instance.OwnedSheepList
                    .OrderByDescending(info => info.Count)
                    .ThenBy(info => info.SheepInfo.Class)
                    .ThenBy(info => info.SheepInfo.Id).ToList();
        }
    }

    private List<OwnedEnchantInfo> OrderOwnedEnchants()
    {
        switch (ArrangeMode)
        {
            default:
            case ArrangeModeEnums.All:
            case ArrangeModeEnums.Summary:
                return User.Instance.OwnedEnchantList
                    .OrderBy(info => info.EnchantInfo.Class)
                    .ThenBy(info => info.EnchantInfo.Id).ToList();
            case ArrangeModeEnums.Class:
                return User.Instance.OwnedEnchantList
                    .OrderByDescending(info => info.EnchantInfo.Class)
                    .ThenBy(info => info.EnchantInfo.Id).ToList();
            case ArrangeModeEnums.Count:
                return User.Instance.OwnedEnchantList
                    .OrderByDescending(info => info.Count)
                    .ThenBy(info => info.EnchantInfo.Class)
                    .ThenBy(info => info.EnchantInfo.Id).ToList();
        }
    }

    private List<OwnedCharacterInfo> OrderOwnedCharacters()
    {
        switch (ArrangeMode)
        {
            default:
            case ArrangeModeEnums.All:
            case ArrangeModeEnums.Summary:
                return User.Instance.OwnedCharacterList
                    .OrderBy(info => info.CharacterInfo.Class)
                    .ThenBy(info => info.CharacterInfo.Id).ToList();
            case ArrangeModeEnums.Class:
                return User.Instance.OwnedCharacterList
                    .OrderByDescending(info => info.CharacterInfo.Class)
                    .ThenBy(info => info.CharacterInfo.Id).ToList();
            case ArrangeModeEnums.Count:
                return User.Instance.OwnedCharacterList
                    .OrderByDescending(info => info.Count)
                    .ThenBy(info => info.CharacterInfo.Class)
                    .ThenBy(info => info.CharacterInfo.Id).ToList();
        }
    }
    
    private List<OwnedMaterialInfo> OrderOwnedMaterials()
    {
        switch (ArrangeMode)
        {
            default:
            case ArrangeModeEnums.All:
            case ArrangeModeEnums.Summary:
                return User.Instance.OwnedMaterialList
                    .OrderBy(info => info.MaterialInfo.Class)
                    .ThenBy(info => info.MaterialInfo.Id).ToList();
            case ArrangeModeEnums.Class:
                return User.Instance.OwnedMaterialList
                    .OrderByDescending(info => info.MaterialInfo.Class)
                    .ThenBy(info => info.MaterialInfo.Id).ToList();
            case ArrangeModeEnums.Count:
                return User.Instance.OwnedMaterialList
                    .OrderByDescending(info => info.Count)
                    .ThenBy(info => info.MaterialInfo.Class)
                    .ThenBy(info => info.MaterialInfo.Id).ToList();
        }
    }
    
    private List<T> OrderAssetList<T>(List<T> assetList) where T : IAsset
    {
        switch (ArrangeMode)
        {
            default:
            case ArrangeModeEnums.All:
            case ArrangeModeEnums.Summary:
            case ArrangeModeEnums.Count:
                return assetList.OrderBy(info => info.Class).ThenBy(info => info.Id).ToList();
            case ArrangeModeEnums.Class:
                return assetList.OrderByDescending(info => info.Class).ThenBy(info => info.Id).ToList();
        }
    }

    #endregion
    
    // Button Click Events
    #region ButtonEvent

    private void OnFactionButtonClicked(PointerEventData data)
    {
        Util.Faction = Util.Faction == Faction.Sheep ? Faction.Wolf : Faction.Sheep;
        
        _deckVm.SwitchDeck(Util.Faction);
        _collectionVm.SwitchCards(Util.Faction);
        Util.DestroyAllChildren(GetImage((int)Images.CraftingCardPanel).transform);
        
        SwitchLobbyUI(Util.Faction);
    }

    private void OnProfileClicked(PointerEventData data)
    {
        var popup = Managers.UI.ShowPopupUI<UI_PlayerProfilePopup>();
        popup.PlayerUserInfo = _userService.UserInfo;
    }
    
    private void OnSettingsClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_SettingsPopup>();
    }
    
    private void OnFriendsClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_FriendsListPopup>();
    }
    
    private void OnMailClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_MailBoxPopup>();
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
        _gameMode = (GameModeEnums)_currentModeIndex;
        StartCoroutine(nameof(MoveModeIcons));
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
        
        SetBottomButton(data.pointerPress.name);
    }
    
    private void OnDeckTabClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        FocusTabButton("DeckTabButton");
        CloseCraftingPanel();
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(false);
        GetImage((int)Images.DeckScrollView).gameObject.SetActive(true);
    }

    private void OnDeckButtonClicked(PointerEventData data)
    {
        var buttonNumber = data.pointerPress.GetComponent<DeckButtonInfo>().DeckIndex;
        _deckVm.SelectDeck(buttonNumber, Util.Faction);
    }

    private void OnCollectionTabClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        FocusTabButton("CollectionTabButton");
        CloseCraftingPanel();
        GetImage((int)Images.DeckScrollView).gameObject.SetActive(false);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(true);
    }

    private void OnArrangeAllClicked(PointerEventData data)
    {
        ArrangeMode = ArrangeModeEnums.All;
        SetArrangeButtonColor("ArrangeAllButton");
    }

    private void OnArrangeSummaryClicked(PointerEventData data)
    {
        ArrangeMode = ArrangeModeEnums.Summary;
        SetArrangeButtonColor("ArrangeSummaryButton");
    }
    
    private void OnArrangeClassClicked(PointerEventData data)
    {
        ArrangeMode = ArrangeModeEnums.Class;
        SetArrangeButtonColor("ArrangeClassButton");
    }
    
    private void OnArrangeCountClicked(PointerEventData data)
    {
        ArrangeMode = ArrangeModeEnums.Count;
        SetArrangeButtonColor("ArrangeCountButton");
    }
    
    private void OnCardClicked(PointerEventData data)
    {
        if (data.pointerPress.TryGetComponent(out Card card) == false) return;
        if (card.IsDragging) return;
        
        if (SelectMode is SelectModeEnums.Reinforce)
        {
            var unitInfo = Managers.Data.UnitInfoDict[card.Id];
            if (VerifyCard(unitInfo) == false) return;
            
            _craftingVm.AddNewUnitMaterial(unitInfo);
            
            var parent = GetImage((int)Images.MaterialPanel).transform;
            var cardFrame = 
                Managers.Resource.GetCardResources<UnitId>(unitInfo, parent, OnReinforceMaterialClicked);
            
            Util.FindChild(cardFrame, "Role").SetActive(false);
            UpdateReinforcePanel();
            ResetCollectionUIForReinforce();
            return;
        }

        if (SelectMode is SelectModeEnums.Recycle)
        {
            // Add Card into recycle scroll view.
            return;
        }

        SetCardPopupUI(card);
    }
    
    private void OnCraftingTabClicked(PointerEventData data)
    {
        GoToCraftingTab();
    }
    
    private void OnCraftingClicked(PointerEventData data)
    {
        var activeUis = new[]
        {
            "CraftingBackButtonFakePanel", "CraftingCraftPanel", "MaterialScrollView"
        };
        SetActivePanels(_craftingUiDict, activeUis);
        
        if (_selectedCard == null || _selectedCardForCrafting == null)
        {
            var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
            Managers.Localization.UpdateWarningPopupText(popup, "warning_select_card");
            return;
        }
        
        InitCraftPanel();
    }
    
    private void OnReinforcingClicked(PointerEventData data)
    {
        if (_selectedCard == null || _selectedCardForCrafting == null) return;
        var activeUis = new[] { "CraftingBackButtonFakePanel", "CraftingReinforcePanel", "MaterialScrollView" };
        SetActivePanels(_craftingUiDict, activeUis);
        InitReinforcePanel();
        ResetCollectionUIForReinforce();
        SelectMode = SelectModeEnums.Reinforce;
    }

    private void OnCraftingBackClicked(PointerEventData data)
    {
        InitCraftingPanel();
    }
    
    private void OnCraftClicked(PointerEventData data)
    {
        if (_selectedCard == null || _selectedCardForCrafting == null)
        {
            var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
            Managers.Localization.UpdateWarningPopupText(popup, "warning_select_card");
            return;
        }
        
        _craftingVm.CardToBeCrafted = GetImage((int)Images.CraftCardPanel).GetComponentInChildren<Card>();
        _ = _craftingVm.CraftCard();
    }
    
    private void OnCraftUpperArrowClicked(PointerEventData data)
    {
        if (_craftingVm.CraftingCount >= 100) return;
        _craftingVm.CraftingCount++;
        UpdateCraftingMaterials(User.Instance.OwnedMaterialList);        
    }
    
    private void OnCraftLowerArrowClicked(PointerEventData data)
    {
        if (_craftingVm.CraftingCount <= 1) return;
        _craftingVm.CraftingCount--;
        UpdateCraftingMaterials(User.Instance.OwnedMaterialList);
    }

    private void OnReinforceMaterialClicked(PointerEventData data)
    {
        var unitInfo = Managers.Data.UnitInfoDict[data.pointerPress.GetComponent<Card>().Id];
        _craftingVm.RemoveNewUnitMaterial(unitInfo);
        ResetCollectionUIForReinforce();
        UpdateReinforcePanel();
        Destroy(data.pointerPress.gameObject);
    }
    
    private async void OnReinforceClicked(PointerEventData data)
    {
        var unitInfo = Managers.Data.UnitInfoDict[_selectedCardForCrafting.Id];
        Managers.UI.ShowPopupUI<UI_ReinforcePopup>();
        
        await _craftingVm.GetReinforceResult(unitInfo);
        
        _craftingVm.InitSetting();
    }   
    
    private void OnRecyclingClicked(PointerEventData data)
    {
        var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
        Managers.Localization.UpdateWarningPopupText(popup, "warning_coming_soon");
    }

    private void OnProductClicked(PointerEventData data)
    {
        GameProduct product = null;
        var go = data.pointerPress.gameObject;
        if (go.TryGetComponent(out ProductSimple productSimple))
        {
            if (productSimple.IsDragging) return;
            var simplePopup = Managers.UI.ShowPopupUI<UI_ProductInfoSimplePopup>();
            simplePopup.FrameObject = Instantiate(go);
            product = productSimple;
            simplePopup.FrameObject.GetComponent<ProductSimple>().ProductInfo = product.ProductInfo;
        }
        
        if (go.TryGetComponent(out ProductPackage productPackage))
        {
            if (productPackage.IsDragging) return;
            var packagePopup = Managers.UI.ShowPopupUI<UI_ProductInfoPopup>();
            packagePopup.FrameObject = Instantiate(go);
            packagePopup.FrameSize = go.GetComponent<RectTransform>().sizeDelta;
            product = productPackage;
            packagePopup.FrameObject.GetComponent<ProductPackage>().ProductInfo = product.ProductInfo;
        }

        if (product == null) return;
        _shopVm.SelectedProduct = product.ProductInfo;
    }
    
    private void OnAdsRemoverClicked(PointerEventData data)
    {
        if (User.Instance.SubscribeAdsRemover) return;
        OnProductClicked(data);
    }
    
    private async Task OnAdsProductClicked(PointerEventData data, DailyProductInfo dailyProductInfo)
    {
        var product = data.pointerPress.gameObject.GetComponent<GameProduct>();
        if (product == null || product.IsDragging) return;
        
        if (User.Instance.SubscribeAdsRemover)
        {
            await _shopVm.RevealDailyProduct(dailyProductInfo);
        }
        else
        {
            Managers.Ads.RevealedDailyProduct = dailyProductInfo;
            Managers.Ads.ShowRewardVideo("Check_Daily_Product");
        }
    }

    private async Task OnRefreshDailyProductsClicked(PointerEventData data)
    {
        if (User.Instance.SubscribeAdsRemover)
        {
            await _shopVm.RefreshDailyProducts();
        }
        else
        {
            Managers.Ads.ShowRewardVideo("Refresh_Daily_Products");
        }
    }
    
    private void OnReservedSalesClicked(PointerEventData data)
    {
        var go = data.pointerPress.gameObject;
        if (go.TryGetComponent(out ProductPackage package) == false) return;
        if (package.IsDragging) return;
        var popup = Managers.UI.ShowPopupUI<UI_ProductReservedInfoPopup>();
        var info = Managers.Data.MaterialInfoDict[package.ProductInfo.Compositions[0].CompositionId];
        var parent = Util.FindChild(popup.gameObject, "Frame", true).transform;
        var size = popup.GetComponent<RectTransform>().sizeDelta.x * 0.42f;
        
        popup.FrameObject = Managers.Resource.GetMaterialResources(info, parent);
        popup.FrameSize = new Vector2(size, size);
        _shopVm.SelectedProduct = package.ProductInfo;
    }
    
    #endregion
    
    // UI Size Adjustments
    #region UiAdjustment
    
    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        
        Managers.Localization.UpdateTextAndFont(_textDict);
        Managers.Localization.UpdateFont(_textDict["UsernameText"]);

        _expSlider = GetImage((int)Images.ExpSliderBackground).transform.parent.GetComponent<Slider>();
        _craftingScrollRect = GetImage((int)Images.CollectionScrollView).GetComponent<ScrollRect>();
        
        _modes = new List<GameObject>
        {            
            GetImage((int)Images.FriendlyMatchPanel).gameObject,
            GetImage((int)Images.RankGamePanel).gameObject,
            GetImage((int)Images.SinglePlayPanel).gameObject
        };
        
        _craftingUiDict = new Dictionary<string, GameObject>
        {
            { "CraftingBackButtonFakePanel", GetImage((int)Images.CraftingBackButtonFakePanel).gameObject },
            { "CraftingSelectPanel", GetImage((int)Images.CraftingSelectPanel).gameObject },
            { "CraftingCraftPanel", GetImage((int)Images.CraftingCraftPanel).gameObject },
            { "CraftingReinforcePanel", GetImage((int)Images.CraftingReinforcePanel).gameObject },
            { "CraftingRecyclePanel", GetImage((int)Images.CraftingRecyclePanel).gameObject },
            { "MaterialScrollView", GetImage((int)Images.MaterialScrollView).gameObject }
        };

        _collectionUiDict = new Dictionary<string, GameObject>
        {
            { "UnitHoldingCardPanel", GetImage((int)Images.UnitHoldingCardPanel).gameObject },
            { "UnitNotHoldingCardPanel", GetImage((int)Images.UnitNotHoldingCardPanel).gameObject },
            { "AssetHoldingCardPanel", GetImage((int)Images.AssetHoldingCardPanel).gameObject },
            { "AssetNotHoldingCardPanel", GetImage((int)Images.AssetNotHoldingCardPanel).gameObject },
            { "CharacterHoldingCardPanel", GetImage((int)Images.CharacterHoldingCardPanel).gameObject },
            { "CharacterNotHoldingCardPanel", GetImage((int)Images.CharacterNotHoldingCardPanel).gameObject },
            { "MaterialHoldingPanel", GetImage((int)Images.MaterialHoldingPanel).gameObject },
            { "UnitHoldingLabelPanel", GetImage((int)Images.UnitHoldingLabelPanel).gameObject },
            { "UnitNotHoldingLabelPanel", GetImage((int)Images.UnitNotHoldingLabelPanel).gameObject },
            { "AssetHoldingLabelPanel", GetImage((int)Images.AssetHoldingLabelPanel).gameObject },
            { "AssetNotHoldingLabelPanel", GetImage((int)Images.AssetNotHoldingLabelPanel).gameObject },
            { "CharacterHoldingLabelPanel", GetImage((int)Images.CharacterHoldingLabelPanel).gameObject },
            { "CharacterNotHoldingLabelPanel", GetImage((int)Images.CharacterNotHoldingLabelPanel).gameObject },
            { "MaterialHoldingLabelPanel", GetImage((int)Images.MaterialHoldingLabelPanel).gameObject }
        };
        
        _arrangeButtonDict = new Dictionary<string, GameObject>
        {
            { "ArrangeAllButton", GetButton((int)Buttons.ArrangeAllButton).gameObject },
            { "ArrangeSummaryButton", GetButton((int)Buttons.ArrangeSummaryButton).gameObject },
            { "ArrangeClassButton", GetButton((int)Buttons.ArrangeClassButton).gameObject },
            { "ArrangeCountButton", GetButton((int)Buttons.ArrangeCountButton).gameObject }
        };

        _bottomButtonDict = new Dictionary<string, GameObject>
        {
            { "ShopButton", GetButton((int)Buttons.ShopButton).gameObject },
            { "ItemButton", GetButton((int)Buttons.ItemButton).gameObject },
            { "GameButton", GetButton((int)Buttons.GameButton).gameObject },
            { "EventButton", GetButton((int)Buttons.EventButton).gameObject },
            { "ClanButton", GetButton((int)Buttons.ClanButton).gameObject },
        };
        
        _bottomButtonFocusDict = new Dictionary<string, GameObject>
        {
            { "ShopButtonFocus", GetButton((int)Buttons.ShopButtonFocus).gameObject },
            { "ItemButtonFocus", GetButton((int)Buttons.ItemButtonFocus).gameObject },
            { "GameButtonFocus", GetButton((int)Buttons.GameButtonFocus).gameObject },
            { "EventButtonFocus", GetButton((int)Buttons.EventButtonFocus).gameObject },
            { "ClanButtonFocus", GetButton((int)Buttons.ClanButtonFocus).gameObject },
        };
        
        _tabButtonDict = new Dictionary<string, GameObject>
        {
            { "DeckTabButton", GetButton((int)Buttons.DeckTabButton).gameObject },
            { "CollectionTabButton", GetButton((int)Buttons.CollectionTabButton).gameObject },
            { "CraftingTabButton", GetButton((int)Buttons.CraftingTabButton).gameObject }
        };
        
        _deckButtonDict = new Dictionary<string, GameObject>
        {
            { "DeckButton1", GetButton((int)Buttons.DeckButton1).gameObject },
            { "DeckButton2", GetButton((int)Buttons.DeckButton2).gameObject },
            { "DeckButton3", GetButton((int)Buttons.DeckButton3).gameObject },
            { "DeckButton4", GetButton((int)Buttons.DeckButton4).gameObject },
            { "DeckButton5", GetButton((int)Buttons.DeckButton5).gameObject }
        };
        
        _lobbyDeckButtonDict = new Dictionary<string, GameObject>
        {
            { "LobbyDeckButton1", GetButton((int)Buttons.LobbyDeckButton1).gameObject },
            { "LobbyDeckButton2", GetButton((int)Buttons.LobbyDeckButton2).gameObject },
            { "LobbyDeckButton3", GetButton((int)Buttons.LobbyDeckButton3).gameObject },
            { "LobbyDeckButton4", GetButton((int)Buttons.LobbyDeckButton4).gameObject },
            { "LobbyDeckButton5", GetButton((int)Buttons.LobbyDeckButton5).gameObject }
        };
    }

    protected override void InitButtonEvents()
    {
        foreach (var pair in _bottomButtonDict)
        {
            pair.Value.BindEvent(OnBottomButtonClicked);
        }
        
        foreach (var pair in _deckButtonDict)
        {
            pair.Value.BindEvent(OnDeckButtonClicked);
        }

        foreach (var pair in _lobbyDeckButtonDict)
        {
            pair.Value.BindEvent(OnDeckButtonClicked);
        }
        
        GetButton((int)Buttons.ProfilePanelButton).gameObject.BindEvent(OnProfileClicked);
        GetButton((int)Buttons.SettingsButton).gameObject.BindEvent(OnSettingsClicked);
        GetButton((int)Buttons.FriendsButton).gameObject.BindEvent(OnFriendsClicked);
        GetButton((int)Buttons.MailButton).gameObject.BindEvent(OnMailClicked);
        GetButton((int)Buttons.FactionButton).gameObject.BindEvent(OnFactionButtonClicked);
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
        
        GetButton((int)Buttons.CraftingTabButton).gameObject.BindEvent(OnCraftingTabClicked);
        
        GetButton((int)Buttons.CraftingBackButton).gameObject.BindEvent(OnCraftingBackClicked);
        GetButton((int)Buttons.CraftingButton).gameObject.BindEvent(OnCraftingClicked);
        GetButton((int)Buttons.CraftButton).gameObject.BindEvent(OnCraftClicked);
        GetButton((int)Buttons.CraftUpperArrowButton).gameObject.BindEvent(OnCraftUpperArrowClicked);
        GetButton((int)Buttons.CraftLowerArrowButton).gameObject.BindEvent(OnCraftLowerArrowClicked);
        
        GetButton((int)Buttons.ReinforcingButton).gameObject.BindEvent(OnReinforcingClicked);
        GetButton((int)Buttons.ReinforceButton).gameObject.BindEvent(OnReinforceClicked);
        
        GetButton((int)Buttons.RecyclingButton).gameObject.BindEvent(OnRecyclingClicked);
    }
    
    private async Task InitMainLobby()
    {
        SwitchLobbyUI(Util.Faction);
        GetImage((int)Images.FriendAlertIcon).gameObject.SetActive(false);
        GetImage((int)Images.MailAlertIcon).gameObject.SetActive(false);

        // 이미지 크기 조정을 위해 캔버스 강제 업데이트
        Canvas.ForceUpdateCanvases();
        SetBottomButton("GameButton");
        
        // Init GameMode
        _gameMode = GameModeEnums.RankGame;
        var go = _modes.FirstOrDefault(go => go.name.Contains(_gameMode.ToString()));
        if (go != null)
        {
            var iconRect = go.transform.GetChild(0).GetComponent<RectTransform>();
            Util.FindChild(go, $"{go.name}Text", false, true).SetActive(true);
            iconRect.anchorMin = new Vector2(iconRect.anchorMin.x, 0.66f);
            iconRect.anchorMax = new Vector2(iconRect.anchorMax.x, 0.66f);
        }

        // // Test
        // await _userService.LoadTestUserInfo(userId: 1);
        // MainLobby_Item Setting
        await _userService.LoadUserInfo();
        await Task.WhenAll(
            InitCollection(),
            InitShop(),
            _lobbyVm.InitFriendAlert(),
            _lobbyVm.InitMailAlert(),
            _lobbyVm.ConnectSignalR(_userService.UserInfo.UserName));
        
        BindUserInfo();
        
#if !UNITY_EDITOR
        var policyFinished = Managers.Policy.CheckPolicyConsent();
        var attFinished = Managers.Policy.CheckAttConsent();
        if (policyFinished == false || attFinished == false)
        {
            await Managers.Policy.RequestConsents(policyFinished, attFinished);
        }

        Managers.Ads.FetchIdfa();
        Managers.Ads.InitLevelPlay();
#endif
        
        var sheepTutorialDone = _userService.TutorialInfo.SheepTutorialDone;
        var wolfTutorialDone = _userService.TutorialInfo.WolfTutorialDone;
        var changeFactionTutorialDone = _userService.TutorialInfo.ChangeFactionTutorialDone;
        
        if (sheepTutorialDone == false || wolfTutorialDone == false || changeFactionTutorialDone == false)
        {
            ProcessTutorial();
        }
    }

    private void BindUserInfo()
    {
        var userInfo = _userService.UserInfo;
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

    private void ProcessTutorial()
    {
        var tutorialInfo = _userService.TutorialInfo;
        
        // Case 1: Both tutorials are completed
        if (tutorialInfo.WolfTutorialDone && tutorialInfo.SheepTutorialDone)
        {
            if (tutorialInfo.ChangeFactionTutorialDone) return;
            Managers.UI.ShowPopupUI<UI_ChangeFactionPopup>();
            return;
        }

        // Case 2: One of the tutorials is completed, Succeed in the other tutorial
        if (tutorialInfo.WolfTutorialDone)
        {
            _tutorialVm.CompleteTutorialWolf();
        }
        else if (tutorialInfo.SheepTutorialDone)
        {
            _tutorialVm.CompleteTutorialSheep();
        }
        // Case 3: Both tutorials are not completed -> First time to play
        else
        {
            Managers.UI.ShowPopupUI<UI_TutorialMainPopup>();
        }
    }
    
    private void SwitchLobbyUI(Faction faction)
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
                gamePanelImage.sprite = Resources.Load<Sprite>("Sprites/Backgrounds/MainLobbySheep");
                factionButtonIcon.sprite = Resources.Load<Sprite>("Sprites/SheepButton");
                break;
            case Faction.Wolf:
                gamePanelImage.sprite = Resources.Load<Sprite>("Sprites/Backgrounds/MainLobbyWolf");
                factionButtonIcon.sprite = Resources.Load<Sprite>("Sprites/WolfButton");
                break;
        }
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
    
    private void SetActivePanels(Dictionary<string, GameObject> dictionary, string[] uiNames)
    {
        foreach (var pair in dictionary)
        {
            pair.Value.SetActive(uiNames.Contains(pair.Key));
        }
    }
    
    private void SetArrangeButtonColor(string buttonName)
    {
        foreach (var pair in _arrangeButtonDict)
        {
            pair.Value.GetComponentInChildren<Image>().color = pair.Key == buttonName ? Color.green : Color.white;
        }
    }

    private void SetBottomButton(string buttonName)
    {
        foreach (var pair in _bottomButtonDict)
        {
            pair.Value.SetActive(pair.Key != buttonName);
        }

        foreach (var pair in _bottomButtonFocusDict)
        {
            pair.Value.SetActive(pair.Key == $"{buttonName}Focus");
        }
    }

    private void InitTutorialMainCamera1(Vector3 npcPos, Vector3 cameraPos)
    {
        var cameraObjects = GameObject.FindGameObjectsWithTag("Camera");
        var cameraObject = cameraObjects.FirstOrDefault(go => go.name == "TutorialCamera1");
        if (cameraObject == null) return;
        _tutorialCamera1 = cameraObject.GetComponent<Camera>();
        _tutorialCamera1.transform.position = cameraPos;
        _tutorialCamera1.transform.LookAt(npcPos);
    }

    private void InitTutorialMainCamera2(Vector3 npcPos, Vector3 cameraPos)
    {
        var cameraObjects = GameObject.FindGameObjectsWithTag("Camera");
        var cameraObject = cameraObjects.FirstOrDefault(go => go.name == "TutorialCamera2");
        if (cameraObject == null) return;
        _tutorialCamera2 = cameraObject.GetComponent<Camera>();
        _tutorialCamera2.transform.position = cameraPos;
        _tutorialCamera2.transform.LookAt(npcPos);
    }
    
    #endregion
    
    // Touch Events
    #region TouchEvent

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CardPopup != null) Managers.UI.ClosePopupUI(CardPopup);
    }

    #endregion
    
    private async void OnDestroy()
    {
        try
        {
            _lobbyVm.OnFriendRequestNotificationReceived -= OnFriendAlert;
            _lobbyVm.OnFriendRequestNotificationOff -= OffFriendAlert;
            _lobbyVm.OnMailAlert -= OnMailAlert;
            _lobbyVm.OffMailAlert -= OffMailAlert;
            _lobbyVm.OnPageChanged -= UpdateScrollbar;
            _lobbyVm.OnChangeButtonFocus -= ChangeButtonFocus;
            _lobbyVm.OnChangeLanguage -= ChangeLanguage;
            _paymentService.OnPaymentSuccess -= OnMailAlert;
            _paymentService.OnCashPaymentSuccess -= OnMailAlert;
            _deckVm.OnDeckInitialized -= SetDeckUI;
            _deckVm.OnDeckSwitched -= SetDeckButtonUI;
            _deckVm.OnDeckSwitched -= ResetDeckUI;
            _collectionVm.OnCardInitialized -= SetCollectionUI;
            _collectionVm.OnCardSwitched -= SwitchCollection;
            _craftingVm.SetCardOnCraftingPanel -= SetCardOnCraftingPanel;
            _craftingVm.SetMaterialsOnCraftPanel -= InitMaterialsOnCraftPanel;
            _craftingVm.InitCraftingPanel -= InitCraftingPanel;
            _craftingVm.SetCollectionUI -= SetCollectionUI;
            _tutorialVm.OnInitTutorialCamera1 -= InitTutorialMainCamera1;
            _tutorialVm.OnInitTutorialCamera2 -= InitTutorialMainCamera2;
            _userService.InitDeckButton -= SetDeckButtonUI;
            Managers.Ads.OnRewardedCheckDailyProduct -= RevealDailyProduct;
            Managers.Ads.OnRewardedRefreshDailyProducts -= RefreshDailyProducts;
        
            await _lobbyVm.LeaveLobby();
            _lobbyVm.Dispose();
        }
        catch (Exception e)
        {
            Debug.LogError($"Main Lobby Destroy Error: {e}");
        }
    }
}
