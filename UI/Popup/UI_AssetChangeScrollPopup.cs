using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_AssetChangeScrollPopup : UI_Popup, IPointerClickHandler
{
    private DeckViewModel _deckVm;
    
    private bool _changing;
    
    private bool Changing
    {
        get => _changing;
        set
        {
            _changing = value;
            GetImage((int)Images.SelectTextPanel).gameObject.SetActive(_changing);
        }
    }
    
    public Card SelectedCard { get; set; }

    private enum Images
    {
        PopupPanel,
        SelectTextPanel,
        SelectedCardPanel,
        CollectionPanel,
    }

    private enum Buttons
    {
        ExitButton,
        EnterButton,
    }

    private enum Texts
    {
        
    }
    
    [Inject]
    public void Construct(DeckViewModel deckViewModel)
    {
        _deckVm = deckViewModel;
    }

    protected override void Init()
    {
        base.Init();

        BindObjects();
        InitButtonEvents();

        switch (SelectedCard.AssetType)
        {
            case Asset.Sheep:
                InitUI<SheepId>();
                break;
            case Asset.Enchant:
                InitUI<EnchantId>();
                break;
            case Asset.Character:
                InitUI<CharacterId>();
                break;
        }
    }
    
    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    private void InitUI<TEnum>() where TEnum : struct, Enum
    {
        GetImage((int)Images.SelectTextPanel).gameObject.SetActive(false);
        
        SetSelectedCardInPopup<TEnum>();
        SetCardInPopup<TEnum>();
    }
    
    protected override void InitButtonEvents()
    {
        GetImage((int)Images.PopupPanel).gameObject.BindEvent(OnPointerClick);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(CloseAllPopup);
        GetButton((int)Buttons.EnterButton).gameObject.BindEvent(CloseAllPopup);
    }

    private void SetSelectedCardInPopup<TEnum>() where TEnum : struct, Enum
    {
        var parent = GetImage((int)Images.SelectedCardPanel).transform;
        foreach (Transform child in parent) Destroy(child.gameObject);
        
        var cardFrame = Util.GetCardResources<TEnum>(SelectedCard, parent, 0, data =>
        {
            SelectedCard = data.pointerPress.GetComponent<Card>();
            Changing = true;
        });
        
        cardFrame.transform.SetParent(parent);
        cardFrame.GetComponent<RectTransform>().anchorMax = Vector2.one;
        cardFrame.GetComponent<RectTransform>().anchorMin = Vector2.zero;
    }
    
    private void SetCardInPopup<TEnum>() where TEnum : struct, Enum
    {
        var parent = GetImage((int)Images.CollectionPanel).transform;
        foreach (Transform child in parent) Destroy(child.gameObject);

        var assets = typeof(TEnum).ToString() switch
        {
            "Google.Protobuf.Protocol.SheepId" => User.Instance.OwnedSheepList.Cast<IAsset>().ToList(),
            "Google.Protobuf.Protocol.EnchantId" => User.Instance.OwnedEnchantList.Cast<IAsset>().ToList(),
            "Google.Protobuf.Protocol.CharacterId" => User.Instance.OwnedCharacterList.Cast<IAsset>().ToList(),
            _ => new List<IAsset>() 
        };

        assets = assets.Where(asset => asset.Id != SelectedCard.Id).OrderBy(asset => asset.Class).ToList();

        foreach (var asset in assets)
        {
            var cardFrame = Util.GetCardResources<TEnum>(asset, parent, 0, data =>
            {
                if (data.pointerPress.TryGetComponent(out Card card) == false) return;

                // Change actual battle setting information
                var oldCard = Changing 
                    ? SelectedCard 
                    : GetImage((int)Images.SelectedCardPanel).GetComponentInChildren<Card>();
                _deckVm.UpdateBattleSetting(oldCard, card);
                SelectedCard = card;
                Changing = false;
                
                // Change UI in Scroll Popup
                SetSelectedCardInPopup<TEnum>();
                SetCardInPopup<TEnum>();
            });
            
            cardFrame.transform.SetParent(parent);
            cardFrame.GetComponent<RectTransform>().anchorMax = Vector2.one;
            cardFrame.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        }
    }
    
    public void OnPointerClick(PointerEventData data)
    {
        _deckVm.ResetDeckUI(Util.Camp);
    }
    
    private void CloseAllPopup(PointerEventData data)
    {
        _deckVm.ResetDeckUI(Util.Camp);
        Managers.UI.CloseAllPopupUI();
    }
}