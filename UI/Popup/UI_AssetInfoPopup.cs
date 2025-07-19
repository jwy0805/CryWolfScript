using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    protected override async void Init()
    {
        try
        {
            base.Init();
        
            await BindObjectsAsync();
            InitButtonEvents();
            await InitUIAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override async Task BindObjectsAsync()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        
        await Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).onClick.AddListener(() => Managers.UI.ClosePopupUI(this));
        GetButton((int)Buttons.EquipButton).onClick.AddListener(OnEquipButtonClicked);
    }

    protected override async Task InitUIAsync()
    {
        var frame = GetImage((int)Images.Frame);
        var frameRect = frame.GetComponent<RectTransform>();
        
        var iconPath = string.Empty;
        var assetTypeKey = string.Empty;
        var assetNameKey = string.Empty;
        var assetInfoKey = string.Empty;
        var assetClassKey = $"unit_class_{SelectedCard.Class.ToString()}";

        GameObject card = null;
        switch (SelectedCard.AssetType)
        {
            case Asset.Sheep:
                iconPath = "Sprites/Icons/icon_assetinfo_sheep";
                assetTypeKey = "asset_type_sheep";
                assetNameKey = $"sheep_name_{((SheepId)SelectedCard.Id).ToString()}";
                assetInfoKey = $"sheep_info_{((SheepId)SelectedCard.Id).ToString()}";
                if (Managers.Data.SheepInfoDict.TryGetValue(SelectedCard.Id, out var sheepInfo))
                {
                    card = await Managers.Resource.GetCardResources<SheepId>(sheepInfo, frameRect);
                }
                break;
            
            case Asset.Enchant:
                iconPath = "Sprites/Icons/icon_assetinfo_enchant";
                assetTypeKey = "asset_type_enchant";
                assetNameKey = $"enchant_name_{((EnchantId)SelectedCard.Id).ToString()}";
                assetInfoKey = $"enchant_info_{((EnchantId)SelectedCard.Id).ToString()}";
                if (Managers.Data.EnchantInfoDict.TryGetValue(SelectedCard.Id, out var enchantInfo))
                {
                    card = await Managers.Resource.GetCardResources<EnchantId>(enchantInfo, frameRect);
                }
                break;
            
            case Asset.Character:
                iconPath = "Sprites/Icons/icon_assetinfo_character";
                assetTypeKey = "asset_type_character";
                assetNameKey = $"character_name_{((CharacterId)SelectedCard.Id).ToString()}";
                assetInfoKey = $"character_info_{((CharacterId)SelectedCard.Id).ToString()}";
                if (Managers.Data.CharacterInfoDict.TryGetValue(SelectedCard.Id, out var characterInfo))
                {
                    card = await Managers.Resource.GetCardResources<CharacterId>(characterInfo, frameRect);
                }
                break;
        }

        if (card == null)
        {
            Debug.Log($"Failed to load card for {SelectedCard.AssetType} with ID {SelectedCard.Id}");
            return;
        }
        // var card = Instantiate(SelectedCard, frameRect, false);
        var cardRect = card.GetComponent<RectTransform>();
        
        GetImage((int)Images.AssetTypeIcon).sprite = await Managers.Resource.LoadAsync<Sprite>(iconPath);
        cardRect.sizeDelta = new Vector2(250, 400);
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        var assetTypeText = GetText((int)Texts.AssetTypeText);
        var assetTitleText = GetText((int)Texts.AssetTitleText);
        var assetClassText = GetText((int)Texts.AssetClassText);
        var assetInfoText = GetText((int)Texts.AssetInfoText);
        
        var task1 = Managers.Localization.BindLocalizedText(assetTypeText, assetTypeKey, FontType.BlackLined);
        var task2 = Managers.Localization.BindLocalizedText(assetTitleText, assetNameKey, FontType.BlackLined);
        var task3 = Managers.Localization.BindLocalizedText(assetClassText, assetClassKey, FontType.BlackLined);
        var task4 = Managers.Localization.BindLocalizedText(assetInfoText, assetInfoKey);
        
        await Task.WhenAll(task1, task2, task3, task4);
    }
    
    private async void OnEquipButtonClicked()
    {
        try
        {
            if (SelectedCard == null) return;
            await Managers.UI.ShowPopupUI<UI_AssetChangeScrollPopup>();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
}
