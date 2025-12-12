using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_RewardSubscriptionPopup : UI_Popup
{
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public string ProductCode { get; set; }

    private enum Images
    {
        RewardPanel,
    }
    
    private enum Texts
    {
        SubscriptionInfoText,
        TapToContinueText,
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

        await Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetText((int)Texts.TapToContinueText).gameObject.BindEvent(OnTapToContinueClicked);
    }

    protected override async Task InitUIAsync()
    {
        var subscriptionText = _textDict["SubscriptionInfoText"];
        var key = Managers.Localization.GetConvertedString($"ProductInfo_{ProductCode}");
        await Managers.Localization.UpdateTextAndFont(subscriptionText, key);
        
        var rewardPanel = GetImage((int)Images.RewardPanel);
        var path = $"UI/Shop/{Util.ConvertProductCodeToProductName(ProductCode)}";
        var go = await Managers.Resource.Instantiate(path, rewardPanel.transform);
        var rect = go.GetOrAddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        switch (ProductCode)
        {
            case "com.hamon.crywolf.non-consumable.ads_remover":
                var adsRemover = go.GetOrAddComponent<AdsRemover>();
                adsRemover.Applied = true;
                break;
        }
    }

    private void OnTapToContinueClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
