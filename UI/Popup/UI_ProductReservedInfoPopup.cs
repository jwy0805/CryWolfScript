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
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        GetProductInfo();
        InitUI();
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

    protected override void InitUI()
    {
        SetContents();
    }
    
    private void GetProductInfo()
    {
        _productInfo = _shopVm.SelectedProduct;
    }

    private void SetContents()
    {
        var contentsPanel = Util.FindChild(gameObject, "Content", true);

        foreach (var composition in _shopVm.ProductsToBeClaimed)
        {
            var framePath = $"UI/Deck/ProductDetailInfo";
            var productFrame = Managers.Resource.Instantiate(framePath, contentsPanel.transform);
            var frame = Util.FindChild(productFrame, "Frame", true);
            var layoutElement = productFrame.GetOrAddComponent<LayoutElement>();
            layoutElement.preferredHeight = Screen.height * 0.08f;

            var path = composition.Type switch
            {
                ProductType.None => $"UI/Shop/NormalizedProducts/{((ProductId)composition.CompositionId).ToString()}",
                ProductType.Spinel => "UI/Shop/NormalizedProducts/SpinelChest",
                ProductType.Gold => "UI/Shop/NormalizedProducts/GoldBasket",
                _ => null
            };
            
            var product = Managers.Resource.Instantiate(path, frame.transform);
            var productRect = product.GetComponent<RectTransform>();
            var size = productFrame.GetComponent<RectTransform>().sizeDelta.y * 0.9f;
            productRect.sizeDelta = new Vector2(size, size);
        }
    }
    
    private void OnBuyButtonClicked(PointerEventData data)
    {
        _shopVm.BuyProduct();
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
