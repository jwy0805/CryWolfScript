using System;
using System.Collections;
using System.Collections.Generic;
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

public class UI_ProductInfoPopup : UI_Popup
{
    private IUserService _userService;
    private ICardFactory _cardFactory;
    private ShopViewModel _shopVm;
    
    private GameObject _frame;
    private Image _icon;
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
        Frame,
        Icon,
    }

    private enum Texts
    {
        TextNum,
        TextPrice,
        TextName,
        TextInfo,
    }
    
    [Inject]
    public void Construct(IUserService userService, ICardFactory cardFactory, ShopViewModel shopViewModel)
    {
        _userService = userService;
        _cardFactory = cardFactory;
        _shopVm = shopViewModel;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            GetProductInfo();
            BindObjects();
            InitButtonEvents();
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
        
        _frame = GetImage((int)Images.Frame).gameObject;
        _icon = GetImage((int)Images.Icon);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.BuyButton).gameObject.BindEvent(OnBuyButtonClicked);
    }

    protected override async Task InitUIAsync()
    {
        var frameRect = FrameObject.GetComponent<RectTransform>();
        var iconPath = _productInfo.CurrencyType == CurrencyType.Spinel 
            ? "Sprites/ShopIcons/icon_spinel"
            : "Sprites/ShopIcons/icon_gold";
        
        _icon.sprite = await Managers.Resource.LoadAsync<Sprite>(iconPath);
        FrameObject.transform.SetParent(_frame.transform);
        frameRect.anchoredPosition = Vector2.zero;
        frameRect.sizeDelta = FrameSize;
        FrameObject.transform.localScale *= 0.85f;
        
        var icon = Util.FindChild(FrameObject, "Icon", true);
        var priceText = Util.FindChild(FrameObject, "TextPrice", true);
        if (icon != null && priceText != null)
        {
            icon.SetActive(false);
            priceText.SetActive(false);
        }
        
        var productText = GetText((int)Texts.TextName);
        productText.text = await Managers.Localization.BindLocalizedText(productText, _productInfo.ProductCode);
        GetText((int)Texts.TextPrice).text = _productInfo.Price.ToString();
        GetText((int)Texts.TextNum).gameObject.SetActive(false);
        await SetContents();
        // SetInfoText();
    }
    
    private void GetProductInfo()
    {
        _productInfo = _shopVm.SelectedProduct;
    }

    private async Task SetContents()
    {
        var contentsPanel = Util.FindChild(gameObject, "Content", true);
        var panelRect = contentsPanel.GetComponent<RectTransform>();
        panelRect.pivot = _productInfo.Compositions.Count > 4 ? new Vector2(0, 0.5f) : new Vector2(0.5f, 0.5f);
        
        var sortedCompositions = _productInfo.Compositions.All(ci => ci.ProductType == ProductType.Material)
            ? _productInfo.Compositions
                .OrderBy(ci => Managers.Data.MaterialInfoDict.TryGetValue(ci.ProductId, out var materialInfo) 
                    ? (int)materialInfo.Class : int.MaxValue).ToList()
            : _productInfo.Compositions;
        
        foreach (var composition in sortedCompositions)
        {
            var framePath = $"UI/Lobby/Deck/ProductInfo";
            var productFrame = await Managers.Resource.Instantiate(framePath, contentsPanel.transform);
            var countText = Util.FindChild(productFrame, "TextNum", true);
            var layoutElement = productFrame.AddComponent<LayoutElement>();
            var count = composition.Count;
            var minCount = composition.MinCount;
            var maxCount = composition.MaxCount;
            
            layoutElement.preferredWidth = Screen.width * 0.15f;
            layoutElement.preferredHeight = Screen.height * 0.15f;
            countText.GetComponent<TextMeshProUGUI>().text = count == 0 
                ? $"{minCount} - {maxCount}" : composition.Count.ToString();

            string productName;
            string path;
            GameObject product;
            switch (composition.ProductType)
            {
                case ProductType.Container:
                case ProductType.Subscription:
                    productName = ((ProductId)composition.CompositionId).ToString();
                    path = $"UI/Shop/NormalizedProducts/{productName}";
                    product = await Managers.Resource.Instantiate(path, productFrame.transform);
                    break;
                
                case ProductType.Unit:
                    productName = ((UnitId)composition.CompositionId).ToString();
                    Managers.Data.UnitInfoDict.TryGetValue(composition.CompositionId, out var unit);
                    path = $"UI/Shop/NormalizedProducts/Product{unit?.Class}";
                    product = await Managers.Resource.Instantiate(path, productFrame.transform);
                    var image = Util.FindChild(product, "CardUnit", true).GetComponent<Image>();
                    image.sprite = await Managers.Resource.LoadAsync<Sprite>($"Sprites/Portrait/{productName}");
                    break;
                
                case ProductType.Material:
                    Managers.Data.MaterialInfoDict.TryGetValue(composition.CompositionId, out var material);
                    product = await _cardFactory.GetMaterialResources(material, productFrame.transform);                  
                    break;
                    
                case ProductType.Enchant:
                case ProductType.Sheep:
                case ProductType.Character:
                case ProductType.Spinel:
                case ProductType.Gold:
                default:
                    path = _cardFactory.GetGoldPrefabPath(composition.Count);
                    product = await Managers.Resource.Instantiate(path, productFrame.transform);
                    break;
            }
            
            if (product.TryGetComponent(out RectTransform productRect) == false) continue;
            var width = layoutElement.preferredWidth;
            var rectSize = productRect.sizeDelta;
            
            product.transform.localScale = rectSize.x == 0 ? Vector3.one : width * Vector3.one / rectSize.x;
            productRect.anchoredPosition = Vector2.zero;
            productRect.anchorMin = new Vector2(0.5f, 0.55f);
            productRect.anchorMax = new Vector2(0.5f, 0.55f);
        }
    }
    
    private async Task SetInfoText()
    {
        var infoText = GetText((int)Texts.TextInfo);
        await Managers.Localization.BindLocalizedText(infoText, $"product_info_{_productInfo.ProductCode}");
    }
    
    private async void OnBuyButtonClicked(PointerEventData data)
    {
        try
        {
            if (!data.pointerPress.gameObject.GetComponent<Button>().interactable) return;
            data.pointerPress.gameObject.GetComponent<Button>().interactable = false;
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
