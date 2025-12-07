using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Zenject;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class UI_ProductReservedInfoPopup : UI_Popup
{
    private ShopViewModel _shopVm;
    
    private ProductInfo _productInfo;

    public GameObject FrameObject { get; set; }
    public Vector2 FrameSize { get; set; }
    
    private enum Buttons
    {
        ExitButton,
        BuyButton,
    }

    private enum Images
    {
        
    }

    private enum Texts
    {
        TextNum,
        TextPrice,
        TextName,
        TextInfo,
    }
    
    [Inject]
    public void Construct(ShopViewModel shopViewModel)
    {
        _shopVm = shopViewModel;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            BindObjects();
            InitButtonEvents();
            GetProductInfo();
            await InitUIAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.BuyButton).gameObject.BindEvent(OnBuyButtonClicked);
    }

    protected override async Task InitUIAsync()
    {
        await SetContents();
        await SetInfoText();
    }
    
    private void GetProductInfo()
    {
        _productInfo = _shopVm.SelectedProduct;
    }

    private async Task SetContents()
    {
        var contentsPanel = Util.FindChild(gameObject, "Content", true);

        for (var i = 0; i < _shopVm.ReservedProductsToBeClaimed.Count; i++)
        {
            var composition = _shopVm.ReservedProductsToBeClaimed[i];
            var key = _shopVm.ReservedProductKeys[i];
            var framePath = $"UI/Lobby/Deck/ProductDetailInfo";
            var productFrame = await Managers.Resource.Instantiate(framePath, contentsPanel.transform);
            var frame = Util.FindChild(productFrame, "Frame", true);
            var layoutElement = productFrame.GetOrAddComponent<LayoutElement>();
            layoutElement.preferredHeight = Screen.height * 0.08f;

            var path = composition.ProductType switch
            {
                ProductType.None => $"UI/Shop/NormalizedProducts/{((ProductId)composition.CompositionId).ToString()}",
                ProductType.Spinel => "UI/Shop/NormalizedProducts/SpinelChest",
                ProductType.Gold => "UI/Shop/NormalizedProducts/GoldBasket",
                _ => null
            };
            
            var product = await Managers.Resource.Instantiate(path, frame.transform);
            var productRect = product.GetComponent<RectTransform>();
            var size = productFrame.GetComponent<RectTransform>().sizeDelta.y * 0.9f;
            productRect.sizeDelta = new Vector2(size, size);
            
            // Set description text
            var text = Util.FindChild(productFrame, "TextDescription", true).GetComponent<TextMeshProUGUI>();
            text.text = await Managers.Localization.BindLocalizedText(text, key);
        }
    }

    private async Task SetInfoText()
    {
        var composition = _productInfo.Compositions.FirstOrDefault(c => c.ProductId == _productInfo.ProductId);
        var str = _productInfo.Category == ProductCategory.GoldPackage ? "" : "X";
        var productText = GetText((int)Texts.TextName);
        productText.text = await Managers.Localization.BindLocalizedText(productText, _productInfo.ProductCode);
        GetText((int)Texts.TextNum).text = str + composition?.Count;
        GetText((int)Texts.TextPrice).text = _productInfo.Price.ToString();
        
        var infoText = GetText((int)Texts.TextInfo);
        var key = $"product_info_{_productInfo.ProductCode}";
        infoText.text = await Managers.Localization.BindLocalizedText(infoText, key);
    }
    
    private async void OnBuyButtonClicked(PointerEventData data)
    {
        try
        {
            await _shopVm.BuyProduct();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
