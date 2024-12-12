using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_CraftSuccessPopup : UI_Popup
{
    private CraftingViewModel _craftingVm;
    
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
        
    }
    
    [Inject]
    public void Construct(CraftingViewModel craftingVm)
    {
        _craftingVm = craftingVm;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
        
        _craftingVm.InitSetting();
    }
    
    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.TextButton).gameObject.BindEvent(ClosePopup);
    }
    
    protected override void InitUI()
    {
        var parent = GetImage((int)Images.CardPanel).transform;
        var cardFrame = _craftingVm.CardToBeCrafted.AssetType switch
        {
            Asset.Unit => Managers.Resource.GetCardResources<UnitId>(_craftingVm.CardToBeCrafted, parent),
            Asset.Sheep => Managers.Resource.GetCardResources<SheepId>(_craftingVm.CardToBeCrafted, parent),
            Asset.Enchant => Managers.Resource.GetCardResources<EnchantId>(_craftingVm.CardToBeCrafted, parent),
            Asset.Character => Managers.Resource.GetCardResources<CharacterId>(_craftingVm.CardToBeCrafted, parent),
            _ => null
        };
        
        if (cardFrame == null) return;
        var cardFrameRect = cardFrame.GetComponent<RectTransform>();
        cardFrameRect.sizeDelta = new Vector2(250, 400);
    }
    
    private void ClosePopup(PointerEventData eventData)
    {
        // Reset the crafting UI
        Managers.UI.ClosePopupUI();
    }
}
