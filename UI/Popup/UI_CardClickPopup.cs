using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Image = UnityEngine.UI.Image;

/* Last Modified : 24. 08. 14
   * Version : 1.0
   */

public class UI_CardClickPopup : UI_Popup
{
    private IUserService _userService;
    
    private Card _selectedCard;
    public Vector3 CardPosition { get; set; }
    public Vector2 Size { get; set; }
    public UnitInfo UnitInfo { get; set; }
    public int Level { get; set; }
    public bool FromDeck { get; set; }

    public Card SelectedCard
    {
        get => _selectedCard;
        set
        {
            _selectedCard = value;
            _selectedCard.TryGetComponent(out Card card);
            UnitInfo = card.UnitInfo;
        }
    }

    private enum Images
    {
        CardClickPanel,
        CardBackground,
        CardPanel,
    }

    private enum Buttons
    {
        UnitInfoButton,
        UnitSelectButton,
    }

    private enum Texts
    {
        UnitNameText,
        UnitSelectText,
    }
    
    [Inject]
    public void Construct(IUserService userService)
    {
        _userService = userService;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
        SetCardInPopup();
    }

    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.UnitInfoButton).gameObject.BindEvent(OnInfoClicked);
        var selectButton = GetButton((int)Buttons.UnitSelectButton);
        selectButton.GetComponent<Button>().onClick.AddListener(OnSelectClicked);
        GetImage((int)Images.CardBackground).gameObject.BindEvent(ClosePopup);
        GetText((int)Texts.UnitNameText).gameObject.BindEvent(ClosePopup);
    }
    
    protected override void InitUI()
    {
        GetImage((int)Images.CardClickPanel).TryGetComponent(out RectTransform rt);
        rt.position = CardPosition;
        rt.sizeDelta = Size;
        
        var unitSelectText = GetText((int)Texts.UnitSelectText);
        if (SelectedCard.transform.parent.name == "Deck") unitSelectText.text = "CHANGE";
        
        var deck = Util.Camp == Camp.Sheep 
            ? User.Instance.DeckSheep.UnitsOnDeck 
            : User.Instance.DeckWolf.UnitsOnDeck;
        var index = Array.FindIndex(deck, unitInfo => unitInfo.Id == UnitInfo.Id);
        GetButton((int)Buttons.UnitSelectButton).interactable = index == -1 || FromDeck;
    }

    private void SetCardInPopup()
    {
        var parent = GetImage((int)Images.CardPanel).transform;
        var cardFrame = Managers.Resource.Instantiate("UI/Deck/CardFrame", parent);
        var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
        
        cardFrame.GetComponent<Image>().sprite = Util.SetCardFrame(UnitInfo.Class);
        cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(
            $"Sprites/Portrait/{UnitInfo.Id.ToString()}");
        
        cardFrame.TryGetComponent(out RectTransform rectTransform);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.anchorMin = new Vector2(0, 0);
        cardFrame.BindEvent(ClosePopup);
    }
    
    private void OnInfoClicked(PointerEventData data)
    {
        // 정보 팝업
        var popup = Managers.UI.ShowPopupUI<UI_CardInfoPopup>();
    }

    private void OnSelectClicked()
    {
        if (FromDeck)
        {
            var popup = Managers.UI.ShowPopupUI<UI_DeckChangeScrollPopup>();
            popup.SelectedCard = SelectedCard;
        }
        else
        {
            var popup = Managers.UI.ShowPopupUI<UI_DeckChangePopup>();
            popup.SelectedCard = SelectedCard;
        }
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
