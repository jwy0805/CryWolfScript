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
        UnitNameText,
        UnitSelectText,
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
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
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
        rt.sizeDelta = Size;
        
        var unitSelectText = GetText((int)Texts.UnitSelectText);
        if (SelectedCard.transform.parent.name == "Deck") unitSelectText.text = "CHANGE";
        
        var deck = Util.Faction == Faction.Sheep 
            ? User.Instance.DeckSheep.UnitsOnDeck 
            : User.Instance.DeckWolf.UnitsOnDeck;
        var index = Array.FindIndex(deck, unitInfo => unitInfo.Id == SelectedCard.Id);
        GetButton((int)Buttons.UnitSelectButton).interactable = index == -1 || FromDeck;
        GetText((int)Texts.UnitNameText).text = Managers.Data.UnitDict[SelectedCard.Id].Name;
    }

    private void SetCardInPopup<TEnum>() where TEnum : struct, Enum
    {
        var parent = GetImage((int)Images.CardPanel).transform;
        var cardFrame = Managers.Resource.Instantiate("UI/Deck/CardFrame", parent);
        var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
        
        if (cardFrame.TryGetComponent(out Card card) == false) return;
        card.Id = SelectedCard.Id;
        card.Class = SelectedCard.Class;
        card.AssetType = typeof(TEnum).Name switch
        {
            "UnitId" => Asset.Unit,
            "SheepId" => Asset.Sheep,
            "EnchantId" => Asset.Enchant,
            "CharacterId" => Asset.Character,
            _ => Asset.None
        };
        
        var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), SelectedCard.Id);
        var path = $"Sprites/Portrait/{enumValue.ToString()}";
        cardFrame.GetComponent<Image>().sprite = Util.SetCardFrame(SelectedCard.Class);
        cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        
        cardFrame.TryGetComponent(out RectTransform rectTransform);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        cardFrame.BindEvent(ClosePopup);
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
