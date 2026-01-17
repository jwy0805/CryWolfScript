using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

/* Last Modified : 24. 10. 30
 * Version : 1.02
 */

public class UI_DeckChangePopup : UI_Popup
{
    private DeckViewModel _deckVm;
    private IUserService _userService;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public Card SelectedCard { get; set; }
    
    private enum Images
    {
        WarningPanel,
        PopupPanel,
        Deck,
        CardPanel,
    }
    
    private enum Buttons
    {
        ExitButton,
        EnterButton,
    }

    private enum Texts
    {
        DeckChangeWarningText,
        DeckChangeInfoText,
    }
    
    [Inject]
    public void Construct(IUserService userService, DeckViewModel deckViewModel)
    {
        _userService = userService;
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
            await SetCardInPopup();
            SetDeckUiInPopup();
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
        GetImage((int)Images.PopupPanel).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.EnterButton).gameObject.BindEvent(ClosePopup);
    }

    protected override void InitUI()
    {
        var warningPanel = GetImage((int)Images.WarningPanel).gameObject;
        warningPanel.SetActive(false);
    }
    
    private void SetDeckUiInPopup()
    {
        var parent = GetImage((int)Images.Deck).transform;
        var deck = _deckVm.GetDeck(Util.Faction);
        foreach (var unit in deck.UnitsOnDeck)
        {
            var cardFrame = Managers.Resource.GetCardResourcesF<UnitId>(unit, parent, async data =>
            {
                // 실제 덱이 수정되고, DeckChangeScrollPopup으로 넘어감
                if (!data.pointerPress.TryGetComponent(out Card card)) return;
                await _deckVm.UpdateDeck(card, SelectedCard);
                await Managers.UI.ShowPopupUI<UI_DeckChangeScrollPopup>();
            });
        }
    }

    private async Task SetCardInPopup()
    {
        var parent = GetImage((int)Images.CardPanel).transform;
        var cardFrame = await Managers.Resource.GetCardResources<UnitId>(SelectedCard, parent);
        cardFrame.TryGetComponent(out RectTransform rectTransform);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
    }
    
    private void ClosePopup(PointerEventData data)
    {
        _deckVm.ResetDeckUI(Util.Faction);
        Managers.UI.CloseAllPopupUI();
    }
}
