using System;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

/*
 * Last Modified : 24. 08. 14
 * Version : 1.0
 */

public class UI_DeckChangePopup : UI_Popup
{
    private DeckViewModel _deckVm;
    private IUserService _userService;

    private UI_MainLobby _mainLobby;
    
    public Card SelectedCard { get; set; }
    
    private enum Images
    {
        PopupPanel,
        Deck,
        CardPanel,
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
        
    }
    
    [Inject]
    public void Construct(IUserService userService, DeckViewModel deckVm)
    {
        _userService = userService;
        _deckVm = deckVm;
    }
    
    protected override void Init()
    {
        base.Init();
        
        _mainLobby = GameObject.Find("UI_MainLobby").GetComponent<UI_MainLobby>();
        BindObjects();
        InitButtonEvents();
        InitUI();
        SetCardInPopup();
        SetDeckUiInPopup();
    }

    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void InitButtonEvents()
    {
        GetImage((int)Images.PopupPanel).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.EnterButton).gameObject.BindEvent(ClosePopup);
    }

    protected override void InitUI()
    {
        
    }
    
    private void SetDeckUiInPopup()
    {
        var parent = GetImage((int)Images.Deck).transform;
        var deck = _deckVm.GetDeck(Util.Camp);
        foreach (var unit in deck.UnitsOnDeck)
        {
            var cardFrame = Util.GetCardResources(unit, parent, 0, OnChangeDeck);
            cardFrame.TryGetComponent(out RectTransform rectTransform);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchorMin = new Vector2(0, 0);
        }
    }

    private void SetCardInPopup()
    {
        var parent = GetImage((int)Images.CardPanel).transform;
        var cardFrame = Util.GetCardResources(SelectedCard.UnitInfo, parent, 0, OnCardClickedOnDeck);
        cardFrame.TryGetComponent(out RectTransform rectTransform);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.anchorMin = new Vector2(0, 0);
    }
    
    // Event Methods
    private void OnChangeDeck(PointerEventData data)
    {   
        // 실제 덱이 수정되고, DeckChangeScrollPopup으로 넘어감
        if (data.pointerPress.TryGetComponent(out Card card) == false) return;
        _deckVm.UpdateDeck(card, SelectedCard);
        Managers.UI.ShowPopupUI<UI_DeckChangeScrollPopup>();
    }
    
    private void OnCardClickedOnDeck(PointerEventData data)
    {
        
    }
    
    private void ClosePopup(PointerEventData data)
    {
        _mainLobby.ResetDeckUI(Util.Camp);
        Managers.UI.CloseAllPopupUI();
    }
}
