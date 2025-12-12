using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_CraftSuccessPopup : UI_Popup
{
    private CraftingViewModel _craftingVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private enum Images
    {
        CardPanel,
    }

    private enum Buttons
    {
        TextButton,
    }

    private enum Texts
    {
        CraftSuccessCompleteText,
        CraftSuccessTouchText,
    }
    
    [Inject]
    public void Construct(CraftingViewModel craftingVm)
    {
        _craftingVm = craftingVm;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            BindObjects();
            InitButtonEvents();
            await InitUIAsync();
        
            _craftingVm.InitReinforceSetting();
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
        GetButton((int)Buttons.TextButton).gameObject.BindEvent(ClosePopup);
    }
    
    protected override async Task InitUIAsync()
    {
        var parent = GetImage((int)Images.CardPanel).transform;
        var cardFrame = _craftingVm.CardToBeCrafted.AssetType switch
        {
            Asset.Unit => await Managers.Resource.GetCardResources<UnitId>(_craftingVm.CardToBeCrafted, parent, ClosePopup),
            Asset.Sheep => await Managers.Resource.GetCardResources<SheepId>(_craftingVm.CardToBeCrafted, parent, ClosePopup),
            Asset.Enchant => await Managers.Resource.GetCardResources<EnchantId>(_craftingVm.CardToBeCrafted, parent, ClosePopup),
            Asset.Character => await Managers.Resource.GetCardResources<CharacterId>(_craftingVm.CardToBeCrafted, parent, ClosePopup),
            _ => null
        };
        
        if (cardFrame == null) return;
        var cardFrameRect = cardFrame.GetComponent<RectTransform>();
        cardFrameRect.sizeDelta = new Vector2(350, 560);
    }
    
    private void ClosePopup(PointerEventData eventData)
    {
        // Reset the crafting UI
        Managers.UI.ClosePopupUI();
    }
}
