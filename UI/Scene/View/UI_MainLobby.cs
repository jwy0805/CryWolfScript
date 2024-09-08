using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

/* Last Modified : 24. 08. 14
 * Version : 1.0
 */

public class UI_MainLobby : UI_Scene, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Scrollbar scrollbar;

    private IUserService _userService;
    private ITokenService _tokenService;
    private MainLobbyViewModel _lobbyVm;
    private DeckViewModel _deckVm;
    private CollectionViewModel _collectionVm;
    
    private Card _selectedCard;
    private UI_CardClickPopup _cardPopup;
    private Transform _deck;
    private Transform _unitCollection;
    private Transform _unitNoCollection;
    private Transform _assetCollection;
    private Transform _assetNoCollection;
    private Transform _characterCollection;
    private Transform _characterNoCollection;
    
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

    private enum Buttons
    {
        CampButton,
        RankButton,
        SingleButton,
        MultiButton,
        ChatButton,
        ClanButton,
        NewsButton,
        
        DeckTabButton,
        DeckButton1,
        DeckButton2,
        DeckButton3,
        DeckButton4,
        DeckButton5,
        
        CollectionTabButton,
    }

    private enum Texts
    {
        GoldText,
        GemText,
        CloverText,
        CloverTimeText,
        
        RankScoreText,
        RankingText,
        RankNameText,
    }

    private enum Images
    {
        GamePanelBackground,
        
        GoldImage,
        GemImage,
        CloverImage,
        CloverTimeImage,
        RankStar,
        RankTextIcon,
        SingleButtonImage,
        MultiButtonImage,
        ChatImageIcon,
        ClanIcon,
        NoticeIcon,
        
        ButtonSelectFrame,
        ShopButtonIcon,
        ShopButtonEffect,
        ItemButtonIcon,
        ItemButtonEffect,
        GameButtonIcon,
        GameButtonEffect,
        EventButtonIcon,
        EventButtonEffect,
        ClanButtonIcon,
        ClanButtonEffect,
        
        DeckScrollView,
        CollectionScrollView,
        Deck,
        
        UnitHoldingCardPanel,
        UnitNotHoldingCardPanel,
        AssetHoldingCardPanel,
        AssetNotHoldingCardPanel,
        CharacterHoldingCardPanel,
        CharacterNotHoldingCardPanel,
        
        CharacterFrame,
        AssetFrame,
    }

    #endregion

    [Inject]
    public void Construct(
        IUserService userService,
        ITokenService tokenService,
        MainLobbyViewModel viewModel,
        DeckViewModel deckViewModel,
        CollectionViewModel collectionViewModel)
    {
        _userService = userService;
        _tokenService = tokenService;
        _lobbyVm = viewModel;
        _deckVm = deckViewModel;
        _collectionVm = collectionViewModel;
    }

    private void Awake()
    {
        _lobbyVm.Initialize(Util.FindChild(gameObject, "HorizontalContents", true)
            .transform.childCount);

        _lobbyVm.OnPageChanged -= UpdateScrollbar;
        _lobbyVm.OnPageChanged += UpdateScrollbar;

        _deckVm.OnDeckInitialized -= SetDeckUI;
        _deckVm.OnDeckInitialized += SetDeckUI;
        _deckVm.OnDeckSelected -= ResetDeckUI;
        _deckVm.OnDeckSelected += ResetDeckUI;
        _deckVm.OnDeckSelected -= SetDeckButtonUI;
        _deckVm.OnDeckSelected += SetDeckButtonUI;
        _deckVm.OnDeckSwitched -= ResetDeckUI;
        _deckVm.OnDeckSwitched += ResetDeckUI;
        
        _collectionVm.OnCardInitialized -= SetCollectionUI;
        _collectionVm.OnCardInitialized += SetCollectionUI;
        _collectionVm.OnCardSwitched -= SwitchCollection;
        _collectionVm.OnCardSwitched += SwitchCollection;
        
        _userService.InitDeckButton -= SetDeckButtonUI;
        _userService.InitDeckButton += SetDeckButtonUI;
    }

    protected override void Init()
    {
        base.Init();

        BindObjects();
        InitButtonEvents();

        Util.Camp = Camp.Sheep;
        InitUI();
        
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
    
    private async void SetMainLobbyItemUI()
    {
        _deck = GetImage((int)Images.Deck).transform;
        _unitCollection = GetImage((int)Images.UnitHoldingCardPanel).transform;
        _unitNoCollection = GetImage((int)Images.UnitNotHoldingCardPanel).transform;
        _assetCollection = GetImage((int)Images.AssetHoldingCardPanel).transform;
        _assetNoCollection = GetImage((int)Images.AssetNotHoldingCardPanel).transform;
        _characterCollection = GetImage((int)Images.CharacterHoldingCardPanel).transform;
        _characterNoCollection = GetImage((int)Images.CharacterNotHoldingCardPanel).transform;

        GetButton((int)Buttons.DeckButton1).GetComponent<DeckButtonInfo>().DeckIndex = 1;
        GetButton((int)Buttons.DeckButton2).GetComponent<DeckButtonInfo>().DeckIndex = 2;
        GetButton((int)Buttons.DeckButton3).GetComponent<DeckButtonInfo>().DeckIndex = 3;
        GetButton((int)Buttons.DeckButton4).GetComponent<DeckButtonInfo>().DeckIndex = 4;
        GetButton((int)Buttons.DeckButton5).GetComponent<DeckButtonInfo>().DeckIndex = 5;

        await Task.WhenAll(_deckVm.Initialize(), _collectionVm.Initialize());
        Debug.Log("Set Cards");
        SetItemUI();
    }
    
    private void SetDeckUI(Camp camp)
    {
        // Set Deck - As using layout group, no need to set position
        var deck = _deckVm.GetDeck(camp);
        var deckParent = Util.FindChild(gameObject, _deck.name, true, true).transform;
        
        foreach (var unit in deck.UnitsOnDeck)
        {
            Util.GetCardResources<UnitId>(unit, deckParent, 0, OnCardClicked);
        }

        // Set Asset Frame
        IAsset asset = camp == Camp.Sheep ? User.Instance.BattleSetting.SheepInfo : User.Instance.BattleSetting.EnchantInfo;
        var assetParent = GetImage((int)Images.AssetFrame).transform;
        var assetFrame = camp == Camp.Sheep ?
            Util.GetCardResources<SheepId>(asset, assetParent, 1, OnCardClicked):
            Util.GetCardResources<EnchantId>(asset, assetParent, 1, OnCardClicked);
        assetFrame.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        assetFrame.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        
        // Set Character Frame
        var characterParent = GetImage((int)Images.CharacterFrame).transform;
        var character = User.Instance.BattleSetting.CharacterInfo;
        var characterFrame = Util.GetCardResources<CharacterId>(character, characterParent, 0.75f, OnCardClicked);
        characterFrame.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        characterFrame.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
    }
    
    private void SetCollectionUI(Camp camp)
    {
        _collectionVm.OrderCardsByClass(User.Instance.OwnedCardListSheep, User.Instance.NotOwnedCardListWolf);
        _collectionVm.OrderCardsByClass(User.Instance.OwnedCardListWolf, User.Instance.NotOwnedCardListWolf);
        _collectionVm.OrderCardsByClass(User.Instance.OwnedSheepList, User.Instance.NotOwnedSheepList);
        _collectionVm.OrderCardsByClass(User.Instance.OwnedEnchantList, User.Instance.NotOwnedEnchantList);
        _collectionVm.OrderCardsByClass(User.Instance.OwnedCharacterList, User.Instance.NotOwnedCharacterList);
        
        
        var ownedUnits = camp == Camp.Sheep ? User.Instance.OwnedCardListSheep : User.Instance.OwnedCardListWolf;
        var notOwnedUnits = camp == Camp.Sheep ? User.Instance.NotOwnedCardListSheep : User.Instance.NotOwnedCardListWolf;
        var ownedAssets = camp == Camp.Sheep 
            ? User.Instance.OwnedSheepList.Cast<IAsset>().ToList() 
            : User.Instance.OwnedEnchantList.Cast<IAsset>().ToList();
        var notOwnedAssets = camp == Camp.Sheep 
            ? User.Instance.NotOwnedSheepList.Cast<IAsset>().ToList() 
            : User.Instance.NotOwnedEnchantList.Cast<IAsset>().ToList();
        
        
        Util.DestroyAllChildren(_unitCollection);
        Util.DestroyAllChildren(_unitNoCollection);
        Util.DestroyAllChildren(_assetCollection);
        Util.DestroyAllChildren(_assetNoCollection);
        Util.DestroyAllChildren(_characterCollection);
        Util.DestroyAllChildren(_characterNoCollection);

        // Units in collection UI
        foreach (var unit in ownedUnits)
        {
            Util.GetCardResources<UnitId>(unit, _unitCollection, 0, OnCardClicked);
        }

        foreach (var unit in notOwnedUnits)
        {
            var cardFrame = Util.GetCardResources<UnitId>(unit, _unitNoCollection, 0, OnCardClicked);
            var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
            var path = $"Sprites/Portrait/{((UnitId)unit.Id).ToString()}_gray";
            cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        }

        // Assets in collection UI
        foreach (var asset in ownedAssets)
        {
            var cardFrame = camp == Camp.Sheep 
                ? Util.GetCardResources<SheepId>(asset, _assetCollection, 0, OnCardClicked) 
                : Util.GetCardResources<EnchantId>(asset, _assetCollection, 0, OnCardClicked);
        }

        foreach (var asset in notOwnedAssets)
        {
            var cardFrame = camp == Camp.Sheep 
                ? Util.GetCardResources<SheepId>(asset, _assetNoCollection, 0, OnCardClicked) 
                : Util.GetCardResources<EnchantId>(asset, _assetNoCollection, 0, OnCardClicked);
            var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
            var path = $"Sprites/Portrait/{((SheepId)asset.Id).ToString()}_gray";
            cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        }
        
        // Characters in collection UI
        foreach (var character in User.Instance.OwnedCharacterList)
        {
            Util.GetCardResources<CharacterId>(character, _characterCollection, 0, OnCardClicked);
        }
        
        foreach (var character in User.Instance.NotOwnedCharacterList)
        {
            var cardFrame = Util.GetCardResources<CharacterId>(character, _characterNoCollection, 0, OnCardClicked);
            var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
            var path = $"Sprites/Portrait/{((CharacterId)character.Id).ToString()}_gray";
            cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        }
    }

    private void SetDeckButtonUI(Camp camp)
    {
        var deckNumber = camp == Camp.Sheep ? User.Instance.DeckSheep.DeckNumber : User.Instance.DeckWolf.DeckNumber;
        var deckButtons = new List<Button>
        {
            GetButton((int)Buttons.DeckButton1),
            GetButton((int)Buttons.DeckButton2),
            GetButton((int)Buttons.DeckButton3),
            GetButton((int)Buttons.DeckButton4),
            GetButton((int)Buttons.DeckButton5)
        };
        
        foreach (var deckButton in deckButtons)
        {
            deckButton.GetComponent<DeckButtonInfo>().IsSelected = false;
        }
        
        deckButtons[deckNumber - 1].GetComponent<DeckButtonInfo>().IsSelected = true;
    }
    
    private void SetItemUI()
    {
        Util.SetAlpha(GetButton((int)Buttons.CollectionTabButton).GetComponent<Image>(), 0.4f);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(false);
    }

    private void SetCardPopupUI(Card card)
    {
        _selectedCard = card;
        CardPopup = Managers.UI.ShowPopupUI<UI_CardClickPopup>();
        CardPopup.SelectedCard = _selectedCard;
        CardPopup.CardPosition = _selectedCard.transform.position;
        CardPopup.FromDeck = card.gameObject.transform.parent == GetImage((int)Images.Deck).transform;
        
        // TODO: Set Size By Various Resolution
    }
    
    public void ResetDeckUI(Camp camp)
    {
        var parent = GetImage((int)Images.Deck).transform;
        foreach (Transform child in parent) Destroy(child.gameObject);
        SetDeckUI(camp);
    }
    
    private void SwitchCollection(Camp camp)
    {
        SetCollectionUI(camp);
    }
    
    // Button Click Events
    #region ButtonEvent

    private void OnCampButtonClicked(PointerEventData data)
    {
        Util.Camp = Util.Camp == Camp.Sheep ? Camp.Wolf : Camp.Sheep;
        
        _deckVm.SwitchDeck(Util.Camp);
        _collectionVm.SwitchCards(Util.Camp);
        
        SwitchLobbyUI(Util.Camp);
    }

    private void OnSingleClicked(PointerEventData data)
    {
        Util.Deck = _deckVm.GetDeck(Util.Camp);
    }

    private void OnMultiClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_Dim>();
        Managers.UI.ShowPopupUI<UI_BattlePopupSheep>();

        Util.Deck = _deckVm.GetDeck(Util.Camp);
    }
    
    private void OnDeckTabClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        Util.SetAlpha(GetButton((int)Buttons.CollectionTabButton).GetComponent<Image>(), 0.4f);
        Util.SetAlpha(GetButton((int)Buttons.DeckTabButton).GetComponent<Image>(), 1f);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(false);
        GetImage((int)Images.DeckScrollView).gameObject.SetActive(true);
    }

    public void OnDeckButtonClicked(PointerEventData data)
    {
        var buttonNumber = data.pointerPress.GetComponent<DeckButtonInfo>().DeckIndex;
        _deckVm.SelectDeck(buttonNumber, Util.Camp);
    }

    private void OnCollectionTabClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        Util.SetAlpha(GetButton((int)Buttons.DeckTabButton).GetComponent<Image>(), 0.4f);
        Util.SetAlpha(GetButton((int)Buttons.CollectionTabButton).GetComponent<Image>(), 1f);
        GetImage((int)Images.DeckScrollView).gameObject.SetActive(false);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(true);
    }
    
    private void OnCardClicked(PointerEventData data)
    {
        if (data.pointerPress.TryGetComponent(out Card card) == false) return;
        SetCardPopupUI(card);
    }
    
    #endregion

    // UI Size Adjustments
    #region UiAdjustment
    
    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.CampButton).gameObject.BindEvent(OnCampButtonClicked);
        GetButton((int)Buttons.SingleButton).gameObject.BindEvent(OnSingleClicked);
        GetButton((int)Buttons.MultiButton).gameObject.BindEvent(OnMultiClicked);
        
        GetButton((int)Buttons.DeckTabButton).gameObject.BindEvent(OnDeckTabClicked);
        GetButton((int)Buttons.DeckButton1).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton2).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton3).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton4).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton5).gameObject.BindEvent(OnDeckButtonClicked);
        
        GetButton((int)Buttons.CollectionTabButton).gameObject.BindEvent(OnCollectionTabClicked);
    }
    
    protected override void InitUI()
    {
        SetObjectSize(GetButton((int)Buttons.CampButton).gameObject, 1.0f);
        SetObjectSize(GetImage((int)Images.GoldImage).gameObject, 1.2f);
        SetObjectSize(GetImage((int)Images.GemImage).gameObject, 1.2f);
        SetObjectSize(GetImage((int)Images.CloverImage).gameObject, 0.9f);
        SetObjectSize(GetImage((int)Images.CloverTimeImage).gameObject, 0.9f);

        // 이미지 크기 조정을 위해 캔버스 강제 업데이트
        Canvas.ForceUpdateCanvases();
        SetObjectSize(GetImage((int)Images.RankStar).gameObject, 0.9f);
        SetObjectSize(GetImage((int)Images.RankTextIcon).gameObject, 0.25f);
        SetObjectSize(GetImage((int)Images.SingleButtonImage).gameObject, 0.7f);
        SetObjectSize(GetImage((int)Images.MultiButtonImage).gameObject, 0.7f);
        
        SetObjectSize(GetImage((int)Images.ChatImageIcon).gameObject, 0.9f);
        SetObjectSize(GetImage((int)Images.ClanIcon).gameObject, 0.6f);
        SetObjectSize(GetImage((int)Images.NoticeIcon).gameObject, 0.6f);
        
        SetObjectSize(GetImage((int)Images.ItemButtonIcon).gameObject, 0.8f);
        SetObjectSize(GetImage((int)Images.GameButtonIcon).gameObject, 0.8f);
        SetObjectSize(GetImage((int)Images.ShopButtonIcon).gameObject, 0.8f);
        SetObjectSize(GetImage((int)Images.EventButtonIcon).gameObject, 0.8f);
        SetObjectSize(GetImage((int)Images.ClanButtonIcon).gameObject, 0.8f);
        
        SetObjectSize(GetImage((int)Images.AssetFrame).gameObject, 0.5f);
        SetObjectSize(GetImage((int)Images.CharacterFrame).gameObject, 0.5f);
        
        // MainLobby_Item Setting
        SetMainLobbyItemUI();
    }

    private void SwitchLobbyUI(Camp camp)
    {
        var gamePanelImage = GetImage((int)Images.GamePanelBackground);
        var campButtonImage = GetButton((int)Buttons.CampButton).GetComponent<Image>();
        var singleImage = GetImage((int)Images.SingleButtonImage);
        var multiImage = GetImage((int)Images.MultiButtonImage);
        
        switch (camp)
        {
            case Camp.Sheep:
                gamePanelImage.sprite = Resources.Load<Sprite>("Sprites/Backgrounds/MainLobbySheep");
                campButtonImage.sprite = Resources.Load<Sprite>("Sprites/SheepButton");
                singleImage.sprite = Resources.Load<Sprite>("Sprites/SheepButton");
                multiImage.sprite = Resources.Load<Sprite>("Sprites/SheepMultiButton");
                break;
            case Camp.Wolf:
                gamePanelImage.sprite = Resources.Load<Sprite>("Sprites/Backgrounds/MainLobbyWolf");
                campButtonImage.sprite = Resources.Load<Sprite>("Sprites/WolfButton");
                singleImage.sprite = Resources.Load<Sprite>("Sprites/WolfButton");
                multiImage.sprite = Resources.Load<Sprite>("Sprites/WolfMultiButton");
                break;
            default:
                break;
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
        throw new NotImplementedException();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("end drag");
    }

    #endregion

    private void OnDestroy()
    {
        _lobbyVm.OnPageChanged -= UpdateScrollbar;
        _deckVm.OnDeckInitialized -= SetDeckUI;
        _deckVm.OnDeckSelected -= ResetDeckUI;
        _deckVm.OnDeckSelected -= SetDeckButtonUI;
        _deckVm.OnDeckSwitched -= ResetDeckUI;
        _collectionVm.OnCardInitialized -= SetCollectionUI;
        _collectionVm.OnCardSwitched -= SwitchCollection;
        _userService.InitDeckButton -= SetDeckButtonUI;
    }
}
