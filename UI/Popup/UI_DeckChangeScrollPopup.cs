using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

/* Last Modified : 24. 08. 14
 * Version : 1.0
 */

public class UI_DeckChangeScrollPopup : UI_Popup, IPointerClickHandler
{
    private DeckViewModel _deckVm;
    
    private UI_MainLobby _mainLobby;
    private Card[] _deck;
    private UI_CardClickPopup _cardPopup;
    private bool _changing;
    
    public Card SelectedCard { get; set; }
    public List<Card[]> Decks { get; set; }
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
        CollectionPanel,
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
        WarningText,
    }

    #endregion
    
    [Inject]
    public void Construct(DeckViewModel deckViewModel)
    {
        _deckVm = deckViewModel;
    }
    
    protected override void Init()
    {
        base.Init();
        
        _mainLobby = GameObject.Find("UI_MainLobby(Clone)").GetComponent<UI_MainLobby>();
        BindObjects();
        InitButtonEvents();
        InitUI();
        SetCollectionInPopup();
        SetDeckInPopup();
    }

    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void InitButtonEvents()
    {
        GetImage((int)Images.PopupPanel).gameObject.BindEvent(OnPointerClick);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(CloseAllPopup);
        GetButton((int)Buttons.EnterButton).gameObject.BindEvent(CloseAllPopup);
        
        GetButton((int)Buttons.DeckButton1).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton2).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton3).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton4).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton5).gameObject.BindEvent(OnDeckButtonClicked);
    }

    protected override void InitUI()
    {
        GetButton((int)Buttons.DeckButton1).GetComponent<DeckButtonInfo>().DeckIndex = 1;
        GetButton((int)Buttons.DeckButton2).GetComponent<DeckButtonInfo>().DeckIndex = 2;
        GetButton((int)Buttons.DeckButton3).GetComponent<DeckButtonInfo>().DeckIndex = 3;
        GetButton((int)Buttons.DeckButton4).GetComponent<DeckButtonInfo>().DeckIndex = 4;
        GetButton((int)Buttons.DeckButton5).GetComponent<DeckButtonInfo>().DeckIndex = 5;
        
        var warningPanel = GetImage((int)Images.WarningPanel).gameObject;
        var selectTextPanel = GetImage((int)Images.SelectTextPanel).gameObject;
        warningPanel.SetActive(false);
        selectTextPanel.SetActive(false);
    }
    
    private void SetDeckInPopup()
    {
        var parent = GetImage((int)Images.Deck).transform;
        foreach (Transform child in parent) Destroy(child.gameObject);
        
        var deck = _deckVm.GetDeck(Util.Camp).UnitsOnDeck;
        foreach (var unit in deck)
        {
            var cardFrame = Util.GetCardResources<UnitId>(unit, parent, 0, OnDeckCardClicked);
            cardFrame.TryGetComponent(out RectTransform rectTransform);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchorMin = new Vector2(0, 0);
        }
    }

    private void SetCollectionInPopup()
    {
        var parent = GetImage((int)Images.CollectionPanel).transform;
        foreach (Transform child in parent) Destroy(child.gameObject);
        
        var collection = Util.Camp == Camp.Sheep 
            ? User.Instance.OwnedCardListSheep 
            : User.Instance.OwnedCardListWolf;
        var deck = _deckVm.GetDeck(Util.Camp);
        var units = collection
            .Where(unitInfo => deck.UnitsOnDeck.All(u => u.Id != unitInfo.Id)).ToList();
        
        
        foreach (var unit in units)
        {
            var cardFrame = Util.GetCardResources<UnitId>(unit, parent, 0, OnCollectionCardClicked);
            cardFrame.TryGetComponent(out RectTransform rectTransform);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchorMin = new Vector2(0, 0);
            
            cardFrame.BindEvent(OnCollectionCardClicked);
        }
    }
    
    // Event Methods
    private void OnCollectionCardClicked(PointerEventData data)
    {
        if (CardPopup != null) Managers.UI.ClosePopupUI(CardPopup);
        if (data.pointerPress.TryGetComponent(out Card card) == false) return;

        if (Changing)
        {
            // 실제 덱 정보 변경
            _deckVm.UpdateDeck(SelectedCard, card);
            
            // Scroll Popup 내 UI 변경
            SetDeckInPopup();
            SetCollectionInPopup();
            
            Changing = false;
        }
        else
        {
            SetCardPopupUI(card);
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
        _mainLobby.ResetDeckUI(Util.Camp);
        Managers.UI.CloseAllPopupUI();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        _mainLobby.ResetDeckUI(Util.Camp);
        if (CardPopup != null) Managers.UI.ClosePopupUI(CardPopup);
    }
    
    private void SetCardPopupUI(Card card)
    {
        SelectedCard = card;
        CardPopup = Managers.UI.ShowPopupUI<UI_CardClickPopup>();
        CardPopup.FromDeck = false;
        CardPopup.SelectedCard = SelectedCard;
        CardPopup.CardPosition = SelectedCard.transform.position;
        
        // TODO: Set Size By Various Resolution
    }

    private void OnDeckButtonClicked(PointerEventData data)
    {
        _mainLobby.OnDeckButtonClicked(data);
    }
}
