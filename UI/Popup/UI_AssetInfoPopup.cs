using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_AssetInfoPopup : UI_Popup
{
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public Card SelectedCard {get; set;}

    private enum Buttons
    {
        ExitButton,
        EquipButton,
    }
    
    private enum Images
    {
        Frame,
        AssetTypeIcon,
    }

    private enum Texts
    {
        AssetTitleText,
        AssetTypeText,
        AssetClassText,
        AssetInfoText,
        EquipText,
    }

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
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
        GetButton((int)Buttons.ExitButton).onClick.AddListener(() => Managers.UI.ClosePopupUI(this));
        GetButton((int)Buttons.EquipButton).onClick.AddListener(OnEquipButtonClicked);
    }

    protected override void InitUI()
    {
        var frame = GetImage((int)Images.Frame);
        var frameRect = frame.GetComponent<RectTransform>();
        var card = Instantiate(SelectedCard, frameRect, false);
        var cardRect = card.GetComponent<RectTransform>();
        
        var assetTypeText = GetText((int)Texts.AssetTypeText);
        var assetTitleText = GetText((int)Texts.AssetTitleText);
        var assetClassText = GetText((int)Texts.AssetClassText);
        var iconPath = string.Empty;
        var assetTypeKey = string.Empty;
        var assetNameKey = string.Empty;
        var assetClassKey = $"unit_class_{SelectedCard.Class.ToString()}";
        
        switch (SelectedCard.AssetType)
        {
            case Asset.Sheep:
                iconPath = "Sprites/Icons/icon_assetinfo_sheep";
                assetTypeKey = "asset_type_sheep";
                assetNameKey = $"sheep_name_{((SheepId)SelectedCard.Id).ToString()}";
                break;
            case Asset.Enchant:
                iconPath = "Sprites/Icons/icon_assetinfo_enchant";
                assetTypeKey = "asset_type_enchant";
                assetNameKey = $"enchant_name_{((EnchantId)SelectedCard.Id).ToString()}";
                break;
            case Asset.Character:
                iconPath = "Sprites/Icons/icon_assetinfo_character";
                assetTypeKey = "asset_type_character";
                assetNameKey = $"character_name_{((CharacterId)SelectedCard.Id).ToString()}";
                break;
        }

        GetImage((int)Images.AssetTypeIcon).sprite = Managers.Resource.Load<Sprite>(iconPath);
        cardRect.sizeDelta = new Vector2(250, 400);
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Managers.Localization.BindLocalizedText(assetTypeText, assetTypeKey, FontType.BlackLined);
        Managers.Localization.BindLocalizedText(assetTitleText, assetNameKey, FontType.BlackLined);
        Managers.Localization.BindLocalizedText(assetClassText, assetClassKey, FontType.BlackLined);
    }
    
    private void OnEquipButtonClicked()
    {
        if (SelectedCard == null) return;
        Managers.UI.ShowPopupUI<UI_AssetChangeScrollPopup>();
    }
}
