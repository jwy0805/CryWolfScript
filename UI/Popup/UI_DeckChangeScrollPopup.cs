using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Vector2 = UnityEngine.Vector2;

/* Last Modified : 24. 09. 09
 * Version : 1.011
 */

public class UI_DeckChangeScrollPopup : UI_Popup, IPointerClickHandler
{
    private IUserService _userService;
    private ICardFactory _cardFactory;
    private DeckViewModel _deckVm;
    
    private Card[] _deck;
    private UI_CardClickPopup _cardPopup;
    private bool _changing;
    private Dictionary<string, GameObject> _deckButtonDict;
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public Card SelectedCard { get; set; }

    private bool Changing
    {
        get => _changing;
        set
        {
            _changing = value;
            GetImage((int)Images.SelectTextPanel).gameObject.SetActive(_changing);
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

    private enum Images
    {
        PopupPanel,
        WarningPanel,
        Deck,
        SelectTextPanel,
    }
    
    private enum Buttons
    {
        ExitButton,
        EnterButton,
        DeckButton1,
        DeckButton2,
        DeckButton3,
        DeckButton4,
        DeckButton5,
    }

    private enum Texts
    {
        DeckChangeScrollWarningText,
        DeckChangeScrollSelectText,
    }

    #endregion
    
    [Inject]
    public void Construct(IUserService userService, ICardFactory cardFactory, DeckViewModel deckViewModel)
    {
        _userService = userService;
        _cardFactory = cardFactory;
        _deckVm = deckViewModel;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            BindObjects();
            InitButtonEvents();
            InitUI();
            await Task.WhenAll(SetCollectionInPopup(), SetDeckInPopup());
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        
        _ = Managers.Localization.UpdateTextAndFont(_textDict);
        
        _deckButtonDict = new Dictionary<string, GameObject>
        {
            {"DeckButton1", GetButton((int)Buttons.DeckButton1).gameObject},
            {"DeckButton2", GetButton((int)Buttons.DeckButton2).gameObject},
            {"DeckButton3", GetButton((int)Buttons.DeckButton3).gameObject},
            {"DeckButton4", GetButton((int)Buttons.DeckButton4).gameObject},
            {"DeckButton5", GetButton((int)Buttons.DeckButton5).gameObject},
        };
    }

    protected override void InitButtonEvents()
    {
        GetImage((int)Images.PopupPanel).gameObject.BindEvent(OnPointerClick);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(CloseAllPopup);
        GetButton((int)Buttons.EnterButton).gameObject.BindEvent(CloseAllPopup);

        foreach (var deckButton in _deckButtonDict.Values)
        {
            deckButton.BindEvent(OnDeckButtonClicked);
        }
    }

    protected override void InitUI()
    {
        for (var i = 1; i <= 5; i++)
        {
            _deckButtonDict[$"DeckButton{i}"].GetComponent<DeckButtonInfo>().DeckIndex = i;
        }
        
        var warningPanel = GetImage((int)Images.WarningPanel).gameObject;
        var selectTextPanel = GetImage((int)Images.SelectTextPanel).gameObject;
        warningPanel.SetActive(false);
        selectTextPanel.SetActive(false);
    }
    
    private async Task SetDeckInPopup()
    {
        var parent = GetImage((int)Images.Deck).transform;
        foreach (Transform child in parent) Managers.Resource.Destroy(child.gameObject);
        
        var deck = _deckVm.GetDeck(Util.Faction).UnitsOnDeck;
        foreach (var unit in deck)
        {
            var cardFrame = await _cardFactory.GetCardResources<UnitId>(unit, parent, OnDeckCardClicked);
            cardFrame.TryGetComponent(out RectTransform rectTransform);
        }
        
        _deckVm.ResetDeckUI(Util.Faction);
    }

    private async Task SetCollectionInPopup()
    {
        var parent = Util.FindChild(gameObject, "Content", true).transform;
        foreach (Transform child in parent) Managers.Resource.Destroy(child.gameObject);
        
        var collection = _userService.User.OwnedUnitList
            .Where(info => info.UnitInfo.Faction == Util.Faction).ToList();
        var deck = _deckVm.GetDeck(Util.Faction);
        var units = collection
            .Where(unitInfo => deck.UnitsOnDeck.All(u => u.Id != unitInfo.UnitInfo.Id)).ToList();
        
        foreach (var unit in units)
        {
            var cardFrame = 
                await _cardFactory.GetCardResourcesF<UnitId>(unit.UnitInfo, parent, OnCollectionCardClicked);
            cardFrame.TryGetComponent(out RectTransform rectTransform);
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = Vector2.zero;
            
            var layoutElement = cardFrame.GetOrAddComponent<LayoutElement>();    
            layoutElement.preferredWidth = 200;
            layoutElement.preferredHeight = 320;
        }
    }
    
    // Event Methods
    private async Task OnCollectionCardClicked(PointerEventData data)
    {
        if (CardPopup != null) Managers.UI.ClosePopupUI(CardPopup);
        if (data.pointerPress.TryGetComponent(out Card card) == false) return;

        if (Changing)
        {
            // 실제 덱 정보 변경
            await _deckVm.UpdateDeck(SelectedCard, card);
            
            // Scroll Popup 내 UI 변경
            await Task.WhenAll(SetCollectionInPopup(), SetDeckInPopup());
            
            Changing = false;
        }
        else
        {
            await SetCardPopupUI(card);
        }
    }
    
    private void OnDeckCardClicked(PointerEventData data)
    {
        // TODO: 컬렉션 카드에 효과
        if (CardPopup != null) Managers.UI.ClosePopupUI(CardPopup);
        SelectedCard = data.pointerPress.GetComponent<Card>();
        Changing = true;
    }
    
    private void CloseAllPopup(PointerEventData data)
    {
        _deckVm.ResetDeckUI(Util.Faction);
        Managers.UI.CloseAllPopupUI();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        _deckVm.ResetDeckUI(Util.Faction);
        if (CardPopup != null) Managers.UI.ClosePopupUI(CardPopup);
    }
    
    private async Task SetCardPopupUI(Card card)
    {
        SelectedCard = card;
        CardPopup = await Managers.UI.ShowPopupUI<UI_CardClickPopup>();
        CardPopup.FromDeck = false;
        CardPopup.SelectedCard = SelectedCard;
        CardPopup.CardPosition = SelectedCard.transform.position;
        
        // TODO: Set Size By Various Resolution
    }

    private async Task OnDeckButtonClicked(PointerEventData data)
    {
        var buttonNumber = data.pointerPress.GetComponent<DeckButtonInfo>().DeckIndex;
        await _deckVm.SelectDeck(buttonNumber, Util.Faction);
    }
}
