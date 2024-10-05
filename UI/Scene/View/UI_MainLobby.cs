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

/* Last Modified : 24. 10. 04
 * Version : 1.012
 */

public class UI_MainLobby : UI_Scene, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Scrollbar scrollbar;

    private IUserService _userService;
    private ITokenService _tokenService;
    private MainLobbyViewModel _lobbyVm;
    private DeckViewModel _deckVm;
    private CollectionViewModel _collectionVm;
    private CraftingViewModel _craftingVm;
    
    private bool _isCraftingPanelOpen;
    private Card _selectedCard;
    private Card _selectedCardForCrafting;
    private UI_CardClickPopup _cardPopup;
    private RectTransform _craftingPanel;
    private ScrollRect _craftingScrollRect;
    private Transform _deck;
    private Transform _unitCollection;
    private Transform _unitNoCollection;
    private Transform _assetCollection;
    private Transform _assetNoCollection;
    private Transform _characterCollection;
    private Transform _characterNoCollection;
    private Transform _materialCollection;
    
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
        FactionButton,
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
        
        CraftingTabButton,
        CraftingButton,
        CraftUpperArrowButton,
        CraftLowerArrowButton,
        CraftButton,
        
        ReinforceButton,
        RecycleButton,
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
        
        CraftCountText,
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
        MaterialHoldingPanel,
        
        CharacterFrame,
        AssetFrame,
        
        CraftingPanel,
        
        CraftingCardPanel,
        CardPedestal,
        
        CraftingSelectPanel,
        
        CraftingCraftPanel,
        CraftCardPanel,
        MaterialPanel,
        
        CraftingReinforcePanel,
        
        CraftingRecyclePanel,
    }

    #endregion

    [Inject]
    public void Construct(
        IUserService userService,
        ITokenService tokenService,
        MainLobbyViewModel viewModel,
        DeckViewModel deckViewModel,
        CollectionViewModel collectionViewModel,
        CraftingViewModel craftingViewModel)
    {
        _userService = userService;
        _tokenService = tokenService;
        _lobbyVm = viewModel;
        _deckVm = deckViewModel;
        _collectionVm = collectionViewModel;
        _craftingVm = craftingViewModel;
    }

    private void Awake()
    {
        _lobbyVm.Initialize(Util.FindChild(gameObject, "HorizontalContents", true)
            .transform.childCount);

        _lobbyVm.OnPageChanged -= UpdateScrollbar;
        _lobbyVm.OnPageChanged += UpdateScrollbar;

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
        _materialCollection = GetImage((int)Images.MaterialHoldingPanel).transform;

        GetButton((int)Buttons.DeckButton1).GetComponent<DeckButtonInfo>().DeckIndex = 1;
        GetButton((int)Buttons.DeckButton2).GetComponent<DeckButtonInfo>().DeckIndex = 2;
        GetButton((int)Buttons.DeckButton3).GetComponent<DeckButtonInfo>().DeckIndex = 3;
        GetButton((int)Buttons.DeckButton4).GetComponent<DeckButtonInfo>().DeckIndex = 4;
        GetButton((int)Buttons.DeckButton5).GetComponent<DeckButtonInfo>().DeckIndex = 5;

        await Task.WhenAll(_deckVm.Initialize(), _collectionVm.Initialize());
        Debug.Log("Set Cards");
        SetItemUI();
    }
    
    private void SetDeckUI(Faction faction)
    {
        // Set Deck - As using layout group, no need to set position
        var deck = _deckVm.GetDeck(faction);
        var deckParent = Util.FindChild(gameObject, _deck.name, true, true).transform;
        
        foreach (var unit in deck.UnitsOnDeck)
        {
            Util.GetCardResources<UnitId>(unit, deckParent, 0, OnCardClicked);
        }

        // Set Asset Frame
        IAsset asset = faction == Faction.Sheep ? User.Instance.BattleSetting.SheepInfo : User.Instance.BattleSetting.EnchantInfo;
        var assetParent = GetImage((int)Images.AssetFrame).transform;
        var assetFrame = faction == Faction.Sheep ?
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
    
    private void SetCollectionUI(Faction faction)
    {
        var ownedUnits = User.Instance.OwnedUnitList
            .Where(info => info.UnitInfo.Faction == faction)
            .OrderBy(info => info.UnitInfo.Class)
            .ThenBy(info => info.UnitInfo.Id).ToList();
        var notOwnedUnits = User.Instance.NotOwnedUnitList
            .Where(info => info.Faction == faction)
            .OrderBy(info => info.Class).ToList();
        var ownedSheep = User.Instance.OwnedSheepList
            .OrderBy(info => info.SheepInfo.Class).ToList();
        var notOwnedSheep = User.Instance.NotOwnedSheepList
            .OrderBy(info => info.Class).ToList();
        var ownedEnchants = User.Instance.OwnedEnchantList
            .OrderBy(info => info.EnchantInfo.Class).ToList();
        var notOwnedEnchants = User.Instance.NotOwnedEnchantList
            .OrderBy(info => info.Class).ToList();
        var ownedCharacters = User.Instance.OwnedCharacterList
            .OrderBy(info => info.CharacterInfo.Class).ToList();
        var notOwnedCharacters = User.Instance.NotOwnedCharacterList
            .OrderBy(info => info.Class).ToList();
        var ownedMaterials = User.Instance.OwnedMaterialList
            .OrderBy(info => info.MaterialInfo.Class).ToList();
        
        Util.DestroyAllChildren(_unitCollection);
        Util.DestroyAllChildren(_unitNoCollection);
        Util.DestroyAllChildren(_assetCollection);
        Util.DestroyAllChildren(_assetNoCollection);
        Util.DestroyAllChildren(_characterCollection);
        Util.DestroyAllChildren(_characterNoCollection);
        Util.DestroyAllChildren(_materialCollection);

        // Units in collection UI
        foreach (var unit in ownedUnits)
        {
            var cardFrame = 
                Util.GetCardResources<UnitId>(unit.UnitInfo, _unitCollection, 0, OnCardClicked);
            GetCountText(cardFrame.transform, unit.Count);
        }

        foreach (var unit in notOwnedUnits)
        {
            var cardFrame = Util.GetCardResources<UnitId>(unit, _unitNoCollection, 0, OnCardClicked);
            var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
            var path = $"Sprites/Portrait/{((UnitId)unit.Id).ToString()}_gray";
            cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        }

        // Assets in collection UI
        if (faction == Faction.Sheep)
        {
            foreach (var sheep in ownedSheep)
            {
                var cardFrame = 
                    Util.GetCardResources<SheepId>(sheep.SheepInfo, _assetCollection, 0, OnCardClicked);
                GetCountText(cardFrame.transform, sheep.Count);
            }

            foreach (var sheep in notOwnedSheep)
            {
                var cardFrame = 
                    Util.GetCardResources<SheepId>(sheep, _assetNoCollection, 0, OnCardClicked);
                var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
                var path = $"Sprites/Portrait/{((SheepId)sheep.Id).ToString()}_gray";
                cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
            }
        }
        else
        {
            foreach (var enchant in ownedEnchants)
            {
                var cardFrame = 
                    Util.GetCardResources<EnchantId>(enchant.EnchantInfo, _assetCollection, 0, OnCardClicked);
                GetCountText(cardFrame.transform, enchant.Count);
            }

            foreach (var enchant in notOwnedEnchants)     
            {
                var cardFrame = 
                    Util.GetCardResources<EnchantId>(enchant, _assetNoCollection, 0, OnCardClicked);
                var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
                var path = $"Sprites/Portrait/{((EnchantId)enchant.Id).ToString()}_gray";
                cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
            }
        }
        
        // Characters in collection UI
        foreach (var character in ownedCharacters)
        {
            var cardFrame = 
                Util.GetCardResources<CharacterId>(character.CharacterInfo, _characterCollection, 0, OnCardClicked);
            GetCountText(cardFrame.transform, character.Count);
        }
        
        foreach (var character in notOwnedCharacters)
        {
            var cardFrame = 
                Util.GetCardResources<CharacterId>(character, _characterNoCollection, 0, OnCardClicked);
            var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
            var path = $"Sprites/Portrait/{((CharacterId)character.Id).ToString()}_gray";
            cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        }
        
        // Materials in collection UI
        foreach (var material in ownedMaterials)
        {
            var cardFrame = Util.GetMaterialResources(material.MaterialInfo, _materialCollection);
            GetCountText(cardFrame.transform, material.Count);
        }
    }

    private void SetDeckButtonUI(Faction faction)
    {
        var deckNumber = faction == Faction.Sheep 
            ? User.Instance.DeckSheep.DeckNumber 
            : User.Instance.DeckWolf.DeckNumber;
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
        Util.SetAlpha(GetButton((int)Buttons.CraftingTabButton).GetComponent<Image>(), 0.4f);
        _craftingPanel = GetImage((int)Images.CraftingPanel).GetComponent<RectTransform>();
        _craftingPanel.sizeDelta = new Vector2(_craftingPanel.sizeDelta.x, 0);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(false);
    }

    private void SetCardPopupUI(Card card)
    {
        _selectedCard = card;
        CardPopup = Managers.UI.ShowPopupUI<UI_CardClickPopup>();
        CardPopup.SelectedCard = _selectedCard;
        CardPopup.CardPosition = _selectedCard.transform.position - new Vector3(0, 60);
        CardPopup.FromDeck = card.gameObject.transform.parent == GetImage((int)Images.Deck).transform 
                             || card.gameObject.transform.parent == GetImage((int)Images.AssetFrame).transform
                             || card.gameObject.transform.parent == GetImage((int)Images.CharacterFrame).transform;
        // TODO: Set Size By Various Resolution
    }
    
    private void ResetDeckUI(Faction faction)
    {
        var parent = GetImage((int)Images.Deck).transform;
        foreach (Transform child in parent) Destroy(child.gameObject);
        SetDeckUI(faction);
    }
    
    private void SwitchCollection(Faction faction) 
    {
        SetCollectionUI(faction);
    }
    
    private void GetCountText(Transform parent, int count, float rate = 0.32f)
    {
        var countTextPanel = Managers.Resource.Instantiate("UI/Deck/CountTextPanel", parent);
        countTextPanel.GetComponentInChildren<TextMeshProUGUI>().text = count.ToString();
        
        var gridLayout = parent.transform.parent.GetComponent<GridLayoutGroup>();
        var countTextRect = countTextPanel.GetComponent<RectTransform>();
        countTextRect.sizeDelta = new Vector2(gridLayout.cellSize.x * rate, gridLayout.cellSize.x * rate);
        countTextRect.anchorMin = new Vector2(0.8f, 0.15f);
        countTextRect.anchorMax = new Vector2(0.8f, 0.15f);
    }

    private void GoToCraftingTab()
    {
        Managers.UI.CloseAllPopupUI();
        Util.SetAlpha(GetButton((int)Buttons.DeckTabButton).GetComponent<Image>(), 0.4f);
        Util.SetAlpha(GetButton((int)Buttons.CollectionTabButton).GetComponent<Image>(), 0.4f);
        Util.SetAlpha(GetButton((int)Buttons.CraftingTabButton).GetComponent<Image>(), 1f);
        GetImage((int)Images.DeckScrollView).gameObject.SetActive(false);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(true);
        GetImage((int)Images.CraftingSelectPanel).gameObject.SetActive(true);

        OpenCraftingPanel();
        _craftingScrollRect.verticalNormalizedPosition = 1f;
    }
    
    private void OpenCraftingPanel()
    {
        if (_isCraftingPanelOpen) return;
        
        InitCraftingPanel();
        StartCoroutine(AdjustCraftingPanel(_craftingVm.CraftingPanelHeight));
    }
    
    private void CloseCraftingPanel()
    {
        _craftingPanel.sizeDelta = new Vector2(_craftingPanel.sizeDelta.x, 0);
        _craftingPanel.gameObject.SetActive(false);
    }
    
    private IEnumerator AdjustCraftingPanel(float targetHeight)
    {
        _isCraftingPanelOpen = true;
        float elapsedTime = 0;
        float startHeight = _craftingPanel.sizeDelta.y;

        while (elapsedTime < _craftingVm.CraftingPanelDuration)
        {
            float newHeight = Mathf.Lerp(startHeight, targetHeight, elapsedTime / _craftingVm.CraftingPanelDuration);
            _craftingPanel.sizeDelta = new Vector2(_craftingPanel.sizeDelta.x, newHeight);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        _craftingPanel.sizeDelta = new Vector2(_craftingPanel.sizeDelta.x, 1000);
        _isCraftingPanelOpen = false;
    }
    
    private void SetCardOnCraftingPanel(Card card)
    {
        GoToCraftingTab();
        LoadCraftingCard(card);
    }

    private void LoadCraftingCard(Card card)
    {
        var parent = GetImage((int)Images.CraftingCardPanel).transform;
        Util.DestroyAllChildren(parent);
        var cardFrame = card.AssetType switch
        {
            Asset.Unit => Util.GetCardResources<UnitId>(card, parent),
            Asset.Sheep => Util.GetCardResources<SheepId>(card, parent),
            Asset.Enchant => Util.GetCardResources<EnchantId>(card, parent),
            Asset.Character => Util.GetCardResources<CharacterId>(card, parent),
            _ => null
        };
        
        if (cardFrame == null) return;
        var cardFrameRect = cardFrame.GetComponent<RectTransform>();
        Util.SetCardSize(cardFrameRect, 250, 400, new Vector2(0, 40));
    }
    
    private void InitCraftingPanel()
    {
        _craftingPanel.gameObject.SetActive(true);
        GetImage((int)Images.CraftingSelectPanel).gameObject.SetActive(true);
        GetImage((int)Images.CraftingCraftPanel).gameObject.SetActive(false);
        GetImage((int)Images.CraftingReinforcePanel).gameObject.SetActive(false);
        GetImage((int)Images.CraftingRecyclePanel).gameObject.SetActive(false);

        if (_selectedCard == null) return;
        _selectedCardForCrafting = _selectedCard;
        var unitLevel = _collectionVm.GetLevelFromUiObject((UnitId)_selectedCard.Id);
        if (unitLevel == 3)
        {
            GetButton((int)Buttons.ReinforceButton).interactable = false;
        }
    }

    private async Task InitCraftPanel()
    {
        if (_selectedCard == null || _selectedCardForCrafting == null) return;
        var unitLevel = _collectionVm.GetLevelFromUiObject((UnitId)_selectedCard.Id);
        var unitId = unitLevel switch
        {
            1 => (UnitId)_selectedCardForCrafting.Id,
            2 => (UnitId)_selectedCardForCrafting.Id - 1,
            _ => (UnitId)_selectedCardForCrafting.Id - 2
        };

        var parent = GetImage((int)Images.CraftCardPanel);
        Util.DestroyAllChildren(parent.transform);
        
        var parentRect = parent.GetComponent<RectTransform>();
        var cardFrame = Managers.Resource.Instantiate("UI/Deck/CardFrame", parent.transform);
        var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
        
        parentRect.anchorMin = new Vector2(0.5f, 0.5f);
        parentRect.anchorMax = new Vector2(0.5f, 0.5f);
        parentRect.anchoredPosition = new Vector2(0, 40);
        parentRect.sizeDelta = new Vector2(250, 400);
        
        if (cardFrame.TryGetComponent(out Card craftingCard) == false) return ;
        // Have to set card info to crafting card, not selected card
        craftingCard.Id = (int)unitId;
        craftingCard.Class = _selectedCardForCrafting.Class;
        craftingCard.AssetType = _selectedCardForCrafting.AssetType;
        
        var enumValue = (UnitId)Enum.ToObject(typeof(UnitId), craftingCard.Id);
        var path = $"Sprites/Portrait/{enumValue.ToString()}";
        cardFrame.GetComponent<Image>().sprite = Util.SetCardFrame(craftingCard.Class);
        cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        
        cardFrame.TryGetComponent(out RectTransform rectTransform);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);     
        
        await _craftingVm.LoadMaterials(unitId);
        _craftingVm.Count = 1;
        _craftingVm.TotalCraftingMaterials = _craftingVm.CraftingMaterials;
        GetText((int)Texts.CraftCountText).text = _craftingVm.Count.ToString();
    }

    private void InitMaterialsOnCraftPanel(List<OwnedMaterialInfo> craftingMaterials, List<OwnedMaterialInfo> ownedMaterials)
    {
        var parent = GetImage((int)Images.MaterialPanel).transform;
        Util.DestroyAllChildren(parent);

        foreach (var material in craftingMaterials)
        {
            var scrollViewChild = Managers.Resource.Instantiate("UI/Deck/CraftingMaterialFrame", parent);
            var itemPanel =  Util.GetMaterialResources(material.MaterialInfo, scrollViewChild.transform);
            var itemPanelRect = itemPanel.GetComponent<RectTransform>();
            var itemFrame = Util.FindChild(itemPanel, "Frame", true);
            var needText = Util.FindChild(scrollViewChild, "NeedCountText", true);
            var ownedText = Util.FindChild(scrollViewChild, "OwnedCountText", true);
            var nameText = Util.FindChild(scrollViewChild, "MaterialNameText", true);
            var materialName = ((MaterialId)material.MaterialInfo.Id).ToString();
            if (materialName.Length > 11)
            {
                materialName = materialName.Substring(0, 9) + "..";
            }
            
            itemPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemPanelRect.sizeDelta = new Vector2(125, 125);
            
            var ownedCount = ownedMaterials.FirstOrDefault(info =>
                info.MaterialInfo.Id == material.MaterialInfo.Id)?.Count ?? 0;
            needText.GetComponent<TextMeshProUGUI>().text = material.Count.ToString();
            ownedText.GetComponent<TextMeshProUGUI>().text = ownedCount.ToString();
            nameText.GetComponent<TextMeshProUGUI>().text = materialName;

            CheckAndSetColorOnMaterialFrame(material, itemFrame);
        }
    }
    
    private void CheckAndSetColorOnMaterialFrame(OwnedMaterialInfo material, GameObject itemFrame)
    {
        var ownedCount = User.Instance.OwnedMaterialList
            .FirstOrDefault(info => info.MaterialInfo.Id == material.MaterialInfo.Id)?.Count ?? 0;
        itemFrame.GetComponent<Image>().color = ownedCount >= material.Count ? Color.green : Color.red;
    }
    
    private void UpdateCraftingMaterials()
    {
        _craftingVm.TotalCraftingMaterials = _craftingVm.CraftingMaterials.Select(info => new OwnedMaterialInfo 
        {
            MaterialInfo = new MaterialInfo
            {
                Id = info.MaterialInfo.Id,
                Class = info.MaterialInfo.Class,
            },
            Count = info.Count * _craftingVm.Count
        }).ToList();
        GetText((int)Texts.CraftCountText).text = _craftingVm.Count.ToString();
        
        var materialPanel = GetImage((int)Images.MaterialPanel).transform;
        var childCount = materialPanel.childCount;
        for (var i = 0; i < childCount; i++)
        {
            var materialFrame = materialPanel.GetChild(i);
            var itemFrame = Util.FindChild(materialFrame.gameObject, "Frame", true);
            var needText = Util.FindChild(materialFrame.gameObject, "NeedCountText", true);
            var material = _craftingVm.TotalCraftingMaterials
                .FirstOrDefault(info => info.MaterialInfo.Id == materialFrame.GetComponentInChildren<MaterialItem>().Id);
            if (material == null) continue;
            needText.GetComponent<TextMeshProUGUI>().text = material.Count.ToString();
            CheckAndSetColorOnMaterialFrame(material, itemFrame);
        }
    }
    
    private void InitReinforcePanel()
    {
        if (_selectedCard == null || _selectedCardForCrafting == null) return;
    }

    private void InitRecyclePanel()
    {
        if (_selectedCard == null || _selectedCardForCrafting == null) return;
    }
    
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

    private void OnSingleClicked(PointerEventData data)
    {
        Util.Deck = _deckVm.GetDeck(Util.Faction);
    }

    private void OnMultiClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_Dim>();
        Managers.UI.ShowPopupUI<UI_BattlePopupSheep>();

        Util.Deck = _deckVm.GetDeck(Util.Faction);
    }
    
    private void OnDeckTabClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        Util.SetAlpha(GetButton((int)Buttons.CollectionTabButton).GetComponent<Image>(), 0.4f);
        Util.SetAlpha(GetButton((int)Buttons.CraftingTabButton).GetComponent<Image>(), 0.4f);
        Util.SetAlpha(GetButton((int)Buttons.DeckTabButton).GetComponent<Image>(), 1f);
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
        Util.SetAlpha(GetButton((int)Buttons.DeckTabButton).GetComponent<Image>(), 0.4f);
        Util.SetAlpha(GetButton((int)Buttons.CraftingTabButton).GetComponent<Image>(), 0.4f);
        Util.SetAlpha(GetButton((int)Buttons.CollectionTabButton).GetComponent<Image>(), 1f);
        CloseCraftingPanel();
        GetImage((int)Images.DeckScrollView).gameObject.SetActive(false);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(true);
    }

    private void OnCardClicked(PointerEventData data)
    {
        if (data.pointerPress.TryGetComponent(out Card card) == false) return;
        SetCardPopupUI(card);
    }
    
    private void OnCraftingTabClicked(PointerEventData data)
    {
        GoToCraftingTab();
    }
    
    private void OnCraftingButtonClicked(PointerEventData data)
    {
        GetImage((int)Images.CraftingSelectPanel).gameObject.SetActive(false);
        GetImage((int)Images.CraftingCraftPanel).gameObject.SetActive(true);
        GetImage((int)Images.CraftingReinforcePanel).gameObject.SetActive(false);
        GetImage((int)Images.CraftingRecyclePanel).gameObject.SetActive(false);
        
        _ = InitCraftPanel();
    }
    
    private void OnReinforceButtonClicked(PointerEventData data)
    {
        GetImage((int)Images.CraftingSelectPanel).gameObject.SetActive(false);
        GetImage((int)Images.CraftingCraftPanel).gameObject.SetActive(false);
        GetImage((int)Images.CraftingReinforcePanel).gameObject.SetActive(true);
        GetImage((int)Images.CraftingRecyclePanel).gameObject.SetActive(false);
        
        InitReinforcePanel();
    }
    
    private void OnRecycleButtonClicked(PointerEventData data)
    {
        GetImage((int)Images.CraftingSelectPanel).gameObject.SetActive(false);
        GetImage((int)Images.CraftingCraftPanel).gameObject.SetActive(false);
        GetImage((int)Images.CraftingReinforcePanel).gameObject.SetActive(false);
        GetImage((int)Images.CraftingRecyclePanel).gameObject.SetActive(true);
        
        InitRecyclePanel();
    }
    
    private void OnCraftClicked(PointerEventData data)
    {
        _craftingVm.CardToBeCrafted = GetImage((int)Images.CraftCardPanel).GetComponentInChildren<Card>();
        _ = _craftingVm.CraftCard();
    }
    
    private void OnCraftUpperArrowClicked(PointerEventData data)
    {
        if (_craftingVm.Count >= 100) return;
        _craftingVm.Count++;
        UpdateCraftingMaterials();        
    }
    
    private void OnCraftLowerArrowClicked(PointerEventData data)
    {
        if (_craftingVm.Count <= 1) return;
        _craftingVm.Count--;
        UpdateCraftingMaterials();
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
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.FactionButton).gameObject.BindEvent(OnFactionButtonClicked);
        GetButton((int)Buttons.SingleButton).gameObject.BindEvent(OnSingleClicked);
        GetButton((int)Buttons.MultiButton).gameObject.BindEvent(OnMultiClicked);
        
        GetButton((int)Buttons.DeckTabButton).gameObject.BindEvent(OnDeckTabClicked);
        GetButton((int)Buttons.DeckButton1).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton2).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton3).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton4).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton5).gameObject.BindEvent(OnDeckButtonClicked);
        
        GetButton((int)Buttons.CollectionTabButton).gameObject.BindEvent(OnCollectionTabClicked);
        GetButton((int)Buttons.CraftingTabButton).gameObject.BindEvent(OnCraftingTabClicked);
        
        GetButton((int)Buttons.CraftingButton).gameObject.BindEvent(OnCraftingButtonClicked);
        GetButton((int)Buttons.ReinforceButton).gameObject.BindEvent(OnReinforceButtonClicked);
        GetButton((int)Buttons.RecycleButton).gameObject.BindEvent(OnRecycleButtonClicked);
        
        GetButton((int)Buttons.CraftButton).gameObject.BindEvent(OnCraftClicked);
        GetButton((int)Buttons.CraftUpperArrowButton).gameObject.BindEvent(OnCraftUpperArrowClicked);
        GetButton((int)Buttons.CraftLowerArrowButton).gameObject.BindEvent(OnCraftLowerArrowClicked);
    }
    
    protected override void InitUI()
    {
        SetObjectSize(GetButton((int)Buttons.FactionButton).gameObject, 1.0f);
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

    private void SwitchLobbyUI(Faction faction)
    {
        var gamePanelImage = GetImage((int)Images.GamePanelBackground);
        var factionButtonImage = GetButton((int)Buttons.FactionButton).GetComponent<Image>();
        var singleImage = GetImage((int)Images.SingleButtonImage);
        var multiImage = GetImage((int)Images.MultiButtonImage);
        
        switch (faction)
        {
            case Faction.Sheep:
                gamePanelImage.sprite = Resources.Load<Sprite>("Sprites/Backgrounds/MainLobbySheep");
                factionButtonImage.sprite = Resources.Load<Sprite>("Sprites/SheepButton");
                singleImage.sprite = Resources.Load<Sprite>("Sprites/SheepButton");
                multiImage.sprite = Resources.Load<Sprite>("Sprites/SheepMultiButton");
                break;
            case Faction.Wolf:
                gamePanelImage.sprite = Resources.Load<Sprite>("Sprites/Backgrounds/MainLobbyWolf");
                factionButtonImage.sprite = Resources.Load<Sprite>("Sprites/WolfButton");
                singleImage.sprite = Resources.Load<Sprite>("Sprites/WolfButton");
                multiImage.sprite = Resources.Load<Sprite>("Sprites/WolfMultiButton");
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
