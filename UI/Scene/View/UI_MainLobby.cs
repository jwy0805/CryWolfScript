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

/* Last Modified : 24. 10. 27
 * Version : 1.017
 */

// This class includes the binding, initialization and core logics for the main lobby UI.
public partial class UI_MainLobby : UI_Scene, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Scrollbar scrollbar;
    
    private IUserService _userService;
    private ITokenService _tokenService;
    private MainLobbyViewModel _lobbyVm;
    private DeckViewModel _deckVm;
    private CollectionViewModel _collectionVm;
    private CraftingViewModel _craftingVm;
    private ShopViewModel _shopVm;

    private readonly float _modeChangeTime = 0.25f;
    
    private int _currentModeIndex;
    private bool _isCraftingPanelOpen;
    private Card _selectedCard;
    private Card _selectedCardForCrafting;
    private UI_CardClickPopup _cardPopup;
    private RectTransform _craftingPanel;
    private ScrollRect _craftingScrollRect;
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
    private Transform _dailyDealPanel;
    private Transform _goldPackagePanel;
    private Transform _spinelPackagePanel;
    private Transform _spinelItemsPanel;
    private Transform _goldItemsPanel;
    private List<GameObject> _modes;
    private Dictionary<string, GameObject> _craftingUiDict;
    private Dictionary<string, GameObject> _collectionUiDict;
    private Dictionary<string, GameObject> _arrangeButtonDict;
    private Dictionary<string, GameObject> _bottomButtonDict;
    private Dictionary<string, GameObject> _bottomButtonFocusDict;
    private Dictionary<string, GameObject> _tabButtonDict;
    private Dictionary<string, GameObject> _deckButtonDict;
    private Dictionary<string, GameObject> _lobbyDeckButtonDict;
    private SelectModeEnums _selectMode;
    private GameModeEnums _gameMode;
    private ArrangeModeEnums _arrangeMode = ArrangeModeEnums.All;

    private Color ThemeColor => Util.Faction == Faction.Sheep
        ? new Color(39 / 255f, 107 / 255f, 214 / 255f)
        : new Color(133 / 255f, 29 / 255f, 72 / 255f);
    
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

    private enum GameModeEnums
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
    }

    private enum Texts
    {
        GoldText,
        SpinelText,
        
        RankScoreText,
        RankingText,
        RankNameText,
        
        CraftCountText,
        
        ReinforceCardSelectText,
        ReinforceCardNumberText,
        SuccessRateText,
        SuccessText,
    }

    private enum Images
    {
        TopPanel,
        GamePanelBackground,
        GameImageGlow,
        LobbyDeck,
        
        FactionButtonIcon,
        
        FriendlyMatchPanel,
        RankGamePanel,
        SinglePlayPanel,
        
        ShopBackground,
        ItemBackground,
        
        RankStar,
        RankTextIcon,
        
        DeckScrollView,
        CollectionScrollView,
        Deck,
        
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
        
        CraftingBackButtonPanel,
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
        GoldPackagePanel,
        SpinelPackagePanel,
        SpinelItemPanel,
        GoldItemPanel
    }

    #endregion

    [Inject]
    public void Construct(
        IUserService userService,
        ITokenService tokenService,
        MainLobbyViewModel viewModel,
        DeckViewModel deckViewModel,
        CollectionViewModel collectionViewModel,
        CraftingViewModel craftingViewModel,
        ShopViewModel shopViewModel)
    {
        _userService = userService;
        _tokenService = tokenService;
        _lobbyVm = viewModel;
        _deckVm = deckViewModel;
        _collectionVm = collectionViewModel;
        _craftingVm = craftingViewModel;
        _shopVm = shopViewModel;
    }

    private void Awake()
    {
        _lobbyVm.Initialize(Util.FindChild(gameObject, "HorizontalContents", true)
            .transform.childCount);

        _lobbyVm.OnPageChanged -= UpdateScrollbar;
        _lobbyVm.OnPageChanged += UpdateScrollbar;
        _lobbyVm.ChangeButtonFocus -= ChangeButtonFocus;
        _lobbyVm.ChangeButtonFocus += ChangeButtonFocus;

        _deckVm.OnDeckInitialized -= SetDeckUI;
        _deckVm.OnDeckInitialized += SetDeckUI;
        _deckVm.OnDeckSwitched -= SetDeckButtonUI;
        _deckVm.OnDeckSwitched += SetDeckButtonUI;
        _deckVm.OnDeckSwitched -= ResetDeckUI;
        _deckVm.OnDeckSwitched += ResetDeckUI;
        
        _collectionVm.OnCardInitialized -= SetCollectionUI;
        _collectionVm.OnCardInitialized += SetCollectionUI;
        _collectionVm.OnCardSwitched -= SwitchCollection;
        _collectionVm.OnCardSwitched += SwitchCollection;

        _craftingVm.SetCardOnCraftingPanel -= SetCardOnCraftingPanel;
        _craftingVm.SetCardOnCraftingPanel += SetCardOnCraftingPanel;
        _craftingVm.SetMaterialsOnCraftPanel -= InitMaterialsOnCraftPanel;
        _craftingVm.SetMaterialsOnCraftPanel += InitMaterialsOnCraftPanel;
        _craftingVm.InitCraftingPanel -= InitCraftingPanel;
        _craftingVm.InitCraftingPanel += InitCraftingPanel;
        _craftingVm.SetCollectionUI -= SetCollectionUI;
        _craftingVm.SetCollectionUI += SetCollectionUI;
        
        _userService.InitDeckButton -= SetDeckButtonUI;
        _userService.InitDeckButton += SetDeckButtonUI;
    }

    protected override void Init()
    {
        base.Init();

        BindObjects();
        InitButtonEvents();

        Util.Faction = Faction.Sheep;
#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
        InitMainLobby();
#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
        
        _lobbyVm.SetCurrentPage(2);
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
    
    private void OnPlayButtonClicked(PointerEventData data)
    {
        
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
            var cardFrame = Util.GetCardResources<UnitId>(unitInfo, parent, OnReinforceMaterialClicked);
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
        var activeUis = new[] { "CraftingBackButtonPanel", "CraftingCraftPanel", "MaterialScrollView" };
        SetActivePanels(_craftingUiDict, activeUis);
        InitCraftPanel();
    }
    
    private void OnReinforcingClicked(PointerEventData data)
    {
        if (_selectedCard == null || _selectedCardForCrafting == null) return;
        var activeUis = new[] { "CraftingBackButtonPanel", "CraftingReinforcePanel", "MaterialScrollView" };
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
            popup.SetWarning("카드를 선택해주세요.");
            return;
        }
        
        _craftingVm.CardToBeCrafted = GetImage((int)Images.CraftCardPanel).GetComponentInChildren<Card>();
        _ = _craftingVm.CraftCard();
    }
    
    private void OnCraftUpperArrowClicked(PointerEventData data)
    {
        if (_craftingVm.CraftingCount >= 100) return;
        _craftingVm.CraftingCount++;
        UpdateCraftingMaterials();        
    }
    
    private void OnCraftLowerArrowClicked(PointerEventData data)
    {
        if (_craftingVm.CraftingCount <= 1) return;
        _craftingVm.CraftingCount--;
        UpdateCraftingMaterials();
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
        popup.SetWarning("준비중인 기능입니다.");
    }

    private void OnProductClicked(PointerEventData data, GameObject frameObject)
    {
        Product product = null;
        if (data.pointerPress.TryGetComponent(out ProductSimple productSimple))
        {
            if (productSimple.IsDragging) return;
            var simplePopup = Managers.UI.ShowPopupUI<UI_ProductInfoSimplePopup>();
            simplePopup.FrameObject = Instantiate(frameObject);
            product = productSimple;
        }
        
        if (data.pointerPress.TryGetComponent(out ProductPackage productPackage))
        {
            if (productPackage.IsDragging) return;
            var packagePopup = Managers.UI.ShowPopupUI<UI_ProductInfoPopup>();
            packagePopup.FrameObject = Instantiate(frameObject);
            packagePopup.FrameSize = frameObject.GetComponent<RectTransform>().sizeDelta;
            product = productPackage;
        }

        if (product == null) return;
        _shopVm.SelectedProduct = product.ProductInfo;
    }
    
    #endregion
    
    // UI Size Adjustments
    #region UiAdjustment
    
    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        _craftingScrollRect = GetImage((int)Images.CollectionScrollView).GetComponent<ScrollRect>();
        
        _modes = new List<GameObject>
        {            
            GetImage((int)Images.FriendlyMatchPanel).gameObject,
            GetImage((int)Images.RankGamePanel).gameObject,
            GetImage((int)Images.SinglePlayPanel).gameObject
        };
        
        _craftingUiDict = new Dictionary<string, GameObject>
        {
            { "CraftingBackButtonPanel", GetImage((int)Images.CraftingBackButtonPanel).gameObject },
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
        GetImage((int)Images.TopPanel).color = ThemeColor;
        GetImage((int)Images.GameImageGlow).color = ThemeColor;
        
        SetObjectSize(GetButton((int)Buttons.FactionButton).gameObject, 0.95f);

        // 이미지 크기 조정을 위해 캔버스 강제 업데이트
        Canvas.ForceUpdateCanvases();
        SetObjectSize(GetImage((int)Images.RankStar).gameObject, 0.9f);
        SetObjectSize(GetImage((int)Images.RankTextIcon).gameObject, 0.25f);
        
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
        
        // MainLobby_Item Setting
        await InitCollection();
        await InitShop();
    }

    private void SwitchLobbyUI(Faction faction)
    {
        GetImage((int)Images.TopPanel).color = ThemeColor;
        GetImage((int)Images.GameImageGlow).color = ThemeColor;
        GetImage((int)Images.ItemBackground).color = ThemeColor;
        GetImage((int)Images.ShopBackground).color = ThemeColor;
        
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
    
    #endregion
    
    // Touch Events
    #region TouchEvent

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CardPopup != null) Managers.UI.ClosePopupUI(CardPopup);
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("end drag");
    }

    #endregion

    private void OnDestroy()
    {
        _lobbyVm.OnPageChanged -= UpdateScrollbar;
        _lobbyVm.ChangeButtonFocus -= ChangeButtonFocus;
        _deckVm.OnDeckInitialized -= SetDeckUI;
        _deckVm.OnDeckSwitched -= SetDeckButtonUI;
        _deckVm.OnDeckSwitched -= ResetDeckUI;
        _collectionVm.OnCardInitialized -= SetCollectionUI;
        _collectionVm.OnCardSwitched -= SwitchCollection;
        _craftingVm.SetCardOnCraftingPanel -= SetCardOnCraftingPanel;
        _craftingVm.SetMaterialsOnCraftPanel -= InitMaterialsOnCraftPanel;
        _craftingVm.InitCraftingPanel -= InitCraftingPanel;
        _craftingVm.SetCollectionUI -= SetCollectionUI;
        _userService.InitDeckButton -= SetDeckButtonUI;
    }
}
