using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using ModestTree;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Image = UnityEngine.UI.Image;

/* Last Modified : 24. 09. 10
 * Version : 1.012
 */

public class UI_CardClickPopup : UI_Popup
{
    private IUserService _userService;
    private CraftingViewModel _craftingVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public Vector3 CardPosition { get; set; }
    public Vector2 Size { get; set; }
    public bool FromDeck { get; set; }

    public Card SelectedCard { get; set; }

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
        UnitCraftingButton,
    }

    private enum Texts
    {
        CardClickUnitInfoText,
        CardClickUnitSelectText,
        CardClickUnitCraftText,
    }
    
    [Inject]
    public void Construct(IUserService userService, CraftingViewModel craftingViewModel)
    {
        _userService = userService;
        _craftingVm = craftingViewModel;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();

        switch (SelectedCard.AssetType)
        {
            case Asset.Unit:
                SetCardInPopup<UnitId>();
                break;
            case Asset.Sheep:
                SetCardInPopup<SheepId>();
                break;
            case Asset.Enchant:
                SetCardInPopup<EnchantId>();
                break;
            case Asset.Character:
                SetCardInPopup<CharacterId>();
                break;
        }
    }

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        
        Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.UnitInfoButton).gameObject.BindEvent(OnInfoClicked);
        GetButton((int)Buttons.UnitSelectButton).gameObject.BindEvent(OnSelectClicked);
        GetButton((int)Buttons.UnitCraftingButton).gameObject.BindEvent(OnCraftingClicked);
        GetImage((int)Images.CardBackground).gameObject.BindEvent(ClosePopup);
    }
    
    protected override void InitUI()
    {
        GetImage((int)Images.CardClickPanel).TryGetComponent(out RectTransform rt);
        rt.position = CardPosition;
        // rt.sizeDelta = Size;
        
        var unitSelectText = GetText((int)Texts.CardClickUnitSelectText);
        if (SelectedCard.transform.parent.name == "Deck")
        {
            Managers.Localization.UpdateTextAndFont(unitSelectText.gameObject, "card_click_unit_select_text_change");
        }
        
        var deck = Util.Faction == Faction.Sheep 
            ? User.Instance.DeckSheep.UnitsOnDeck 
            : User.Instance.DeckWolf.UnitsOnDeck;
        var index = Array.FindIndex(deck, unitInfo => unitInfo.Id == SelectedCard.Id);
        var isOwned = User.Instance.OwnedUnitList.Exists(info => info.UnitInfo.Id == SelectedCard.Id);
        var interactable = (index == -1 || FromDeck) && isOwned;

        GetButton((int)Buttons.UnitSelectButton).interactable = interactable;
    }

    private void SetCardInPopup<TEnum>() where TEnum : struct, Enum
    {
        var parent = GetImage((int)Images.CardPanel).transform;
        var cardFrame = Managers.Resource.GetCardResources<TEnum>(SelectedCard, parent, ClosePopup, true);
        var cardFrameRect = cardFrame.GetComponent<RectTransform>();
        cardFrameRect.sizeDelta = new Vector2(200, 320);
        cardFrameRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardFrameRect.anchorMax = new Vector2(0.5f, 0.5f);
    }
    
    private void OnInfoClicked(PointerEventData data)
    {
        if (SelectedCard.AssetType == Asset.Unit)
        {
            var popup = Managers.UI.ShowPopupUI<UI_UnitInfoPopup>();
            popup.SelectedCard = SelectedCard;
        }
        else
        {
            var popup = Managers.UI.ShowPopupUI<UI_AssetInfoPopup>();
            popup.SelectedCard = SelectedCard;
        }
    }

    private void OnSelectClicked(PointerEventData data)
    {
        var deck = Util.Faction == Faction.Sheep 
            ? User.Instance.DeckSheep.UnitsOnDeck 
            : User.Instance.DeckWolf.UnitsOnDeck;
        var index = Array.FindIndex(deck, unitInfo => unitInfo.Id == SelectedCard.Id);
        var isOwned = User.Instance.OwnedUnitList.Exists(info => info.UnitInfo.Id == SelectedCard.Id);
        var interactable = (index == -1 || FromDeck) && isOwned;
        if (interactable == false) return;
        
        if (SelectedCard.AssetType == Asset.Unit)
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
        else
        {
            var popup = Managers.UI.ShowPopupUI<UI_AssetChangeScrollPopup>();
            popup.SelectedCard = SelectedCard;
        }
    }

    private void OnCraftingClicked(PointerEventData data)
    {
        _craftingVm.SetCard(SelectedCard);
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
