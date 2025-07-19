using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            BindObjects();
            InitButtonEvents();
            await InitUIAsync();

            switch (SelectedCard.AssetType)
            {
                case Asset.Unit:
                    await SetCardInPopup<UnitId>();
                    break;
                case Asset.Sheep:
                    await SetCardInPopup<SheepId>();
                    break;
                case Asset.Enchant:
                    await SetCardInPopup<EnchantId>();
                    break;
                case Asset.Character:
                    await SetCardInPopup<CharacterId>();
                    break;
            }
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
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.UnitInfoButton).gameObject.BindEvent(OnInfoClicked);
        GetButton((int)Buttons.UnitSelectButton).gameObject.BindEvent(OnSelectClicked);
        GetButton((int)Buttons.UnitCraftingButton).gameObject.BindEvent(OnCraftingClicked);
        GetImage((int)Images.CardBackground).gameObject.BindEvent(ClosePopup);
    }
    
    protected override async Task InitUIAsync()
    {
        GetImage((int)Images.CardClickPanel).TryGetComponent(out RectTransform rt);
        rt.position = CardPosition;
        
        var unitSelectText = GetText((int)Texts.CardClickUnitSelectText);
        var cardParentName = SelectedCard.transform.parent.name;
        if (cardParentName is "Deck" or "BattleSettingPanel")
        {
            var key = "card_click_unit_select_text_deck";
            await Managers.Localization.UpdateTextAndFont(unitSelectText.gameObject, key);
        }
        
        ActiveUnitSelectButton();
        ActiveCraftingButton();
    }

    private void ActiveUnitSelectButton()
    {
        var user = User.Instance;
        var faction = Util.Faction;
        var isOwned = false;
        var interactable = false;
        
        switch (SelectedCard.AssetType)
        {
            case Asset.Unit:
                var deck = faction == Faction.Sheep ? user.DeckSheep.UnitsOnDeck : user.DeckWolf.UnitsOnDeck;
                var index = Array.FindIndex(deck, unitInfo => unitInfo.Id == SelectedCard.Id);
                isOwned = User.Instance.OwnedUnitList.Exists(info => info.UnitInfo.Id == SelectedCard.Id);
                interactable = (index == -1 || FromDeck) && isOwned;
                break;
            case Asset.Sheep:
                isOwned = user.OwnedSheepList.Exists(info => info.SheepInfo.Id == SelectedCard.Id);
                interactable = isOwned;
                break;
            case Asset.Enchant:
                isOwned = user.OwnedEnchantList.Exists(info => info.EnchantInfo.Id == SelectedCard.Id);
                interactable = isOwned;
                break;
            case Asset.Character:
                isOwned = user.OwnedCharacterList.Exists(info => info.CharacterInfo.Id == SelectedCard.Id);
                interactable = isOwned;
                break;
        }
        
        GetButton((int)Buttons.UnitSelectButton).interactable = interactable;
    }

    private void ActiveCraftingButton()
    {
        var interactable = false;
        
        switch (SelectedCard.AssetType)
        {
            case Asset.Sheep:
            case Asset.Enchant:
            case Asset.Character:
                interactable = false;
                break;
            case Asset.Unit:
                interactable = true;
                break;
        }
        
        GetButton((int)Buttons.UnitCraftingButton).interactable = interactable;
    }
    
    private async Task SetCardInPopup<TEnum>() where TEnum : struct, Enum
    {
        var parent = GetImage((int)Images.CardPanel).transform;
        var cardFrame = await Managers.Resource.GetCardResources<TEnum>(SelectedCard, parent, ClosePopup, true);
        var cardFrameRect = cardFrame.GetComponent<RectTransform>();
        cardFrameRect.sizeDelta = new Vector2(200, 320);
        cardFrameRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardFrameRect.anchorMax = new Vector2(0.5f, 0.5f);
    }
    
    private async Task OnInfoClicked(PointerEventData data)
    {
        if (SelectedCard.AssetType == Asset.Unit)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_UnitInfoPopup>();
            popup.SelectedCard = SelectedCard;
        }
        else
        {
            var popup = await Managers.UI.ShowPopupUI<UI_AssetInfoPopup>();
            popup.SelectedCard = SelectedCard;
        }
    }

    private async Task OnSelectClicked(PointerEventData data)
    {
        var deck = Util.Faction == Faction.Sheep 
            ? User.Instance.DeckSheep.UnitsOnDeck 
            : User.Instance.DeckWolf.UnitsOnDeck;
        var index = Array.FindIndex(deck, unitInfo => unitInfo.Id == SelectedCard.Id);
        // var isOwned = User.Instance.OwnedUnitList.Exists(info => info.UnitInfo.Id == SelectedCard.Id);
        // var interactable = (index == -1 || FromDeck) && isOwned;
        var isOwned = SelectedCard.AssetType switch
        {
            Asset.Unit =>
                User.Instance.OwnedUnitList.Exists(info => info.UnitInfo.Id == SelectedCard.Id),
            Asset.Sheep =>
                User.Instance.OwnedSheepList.Exists(info => info.SheepInfo.Id == SelectedCard.Id),
            Asset.Enchant => 
                User.Instance.OwnedEnchantList.Exists(info => info.EnchantInfo.Id == SelectedCard.Id),
            Asset.Character =>
                User.Instance.OwnedCharacterList.Exists(info => info.CharacterInfo.Id == SelectedCard.Id),
            _ => false
        };
        if (isOwned == false) return;
        
        if (SelectedCard.AssetType == Asset.Unit)
        {
            if (FromDeck)
            {
                var popup = await Managers.UI.ShowPopupUI<UI_DeckChangeScrollPopup>();
                popup.SelectedCard = SelectedCard;
            }
            else
            {
                var popup = await Managers.UI.ShowPopupUI<UI_DeckChangePopup>();
                popup.SelectedCard = SelectedCard;
            }
        }
        else
        {
            var popup = await Managers.UI.ShowPopupUI<UI_AssetChangeScrollPopup>();
            Debug.Log("sss");
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
