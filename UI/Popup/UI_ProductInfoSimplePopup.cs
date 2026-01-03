using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

/* Last Modified : 24. 10. 30
 * Version : 1.02
 */
public class UI_ProductInfoSimplePopup : UI_Popup
{
    private ShopViewModel _shopVm;
    private IUserService _userService;
    
    private GameObject _frame;
    private Image _icon;
    private ProductInfo _productInfo;
    
    public bool IsDailyProduct { get; set; }
    public string LocalizedPriceText { get; set; } = string.Empty;
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
    public void Construct(IUserService userService, ShopViewModel shopViewModel)
    {
        _userService = userService;
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
        var iconPath = SetIconPath(_productInfo.CurrencyType, _productInfo.Category);
        var composition = _productInfo.Compositions.FirstOrDefault(c => c.ProductId == _productInfo.ProductId);
        var str = _productInfo.Category == ProductCategory.GoldPackage ? "" : "X";
        var frameRect = FrameObject.GetComponent<RectTransform>();
        
        _icon.sprite = await Managers.Resource.LoadAsync<Sprite>(iconPath);
        var iconRect = _icon.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(iconRect.sizeDelta.x, _productInfo.CurrencyType == CurrencyType.Spinel ? 59 : 70);
        
        FrameObject.transform.SetParent(_frame.transform);
        frameRect.anchoredPosition = Vector2.zero;
        frameRect.sizeDelta = FrameSize;
        FrameObject.transform.localScale *= 0.85f;
        
        var productText = GetText((int)Texts.TextName);
        productText.text = await Managers.Localization.BindLocalizedText(productText, _productInfo.ProductCode);
        GetText((int)Texts.TextNum).text = str + composition?.Count;
        GetText((int)Texts.TextPrice).text = LocalizedPriceText == string.Empty 
            ? _productInfo.Price.ToString()
            : LocalizedPriceText;

        if (IsDailyProduct)
        {
            GetText((int)Texts.TextNum).gameObject.SetActive(false);
        }
    }

    private string SetIconPath(CurrencyType currencyType, ProductCategory category)
    {
        const string spinelIconPath = "Sprites/ShopIcons/icon_spinel";
        const string goldIconPath = "Sprites/ShopIcons/icon_gold";

        return category switch
        {
            ProductCategory.DailyDeal => goldIconPath,
            ProductCategory.ReservedSale => spinelIconPath,
            _ => currencyType == CurrencyType.Spinel ? spinelIconPath : goldIconPath
        };
    }
    
    private void GetProductInfo()
    {
        _productInfo = _shopVm.SelectedProduct;
    }
    
    private async void OnBuyButtonClicked(PointerEventData data)
    {
        try
        {
            if (!data.pointerPress.gameObject.GetComponent<Button>().interactable) return;
            data.pointerPress.gameObject.GetComponent<Button>().interactable = false;
            if (IsDailyProduct)
            {
                _shopVm.BuyDailyProduct();
            }
            else
            {
                await _shopVm.BuyProduct();
            }
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
