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
        var iconPath = _productInfo.CurrencyType == CurrencyType.Spinel 
            ? "Sprites/ShopIcons/icon_spinel"
            : "Sprites/ShopIcons/icon_gold";
        var composition = _productInfo.Compositions.FirstOrDefault(c => c.ProductId == _productInfo.ProductId);
        var str = _productInfo.Category == ProductCategory.GoldPackage ? "" : "X";
        var frameRect = FrameObject.GetComponent<RectTransform>();
        
        _icon.sprite = await Managers.Resource.LoadAsync<Sprite>(iconPath);
        FrameObject.transform.SetParent(_frame.transform);
        frameRect.anchoredPosition = Vector2.zero;
        frameRect.sizeDelta = FrameSize;
        FrameObject.transform.localScale *= 0.85f;
        
        var productText = GetText((int)Texts.TextName);
        productText.text = await Managers.Localization.BindLocalizedText(productText, _productInfo.ProductCode);
        GetText((int)Texts.TextNum).text = str + composition?.Count;
        GetText((int)Texts.TextPrice).text = _productInfo.Price.ToString();

        if (IsDailyProduct)
        {
            GetText((int)Texts.TextNum).gameObject.SetActive(false);
        }
    }
    
    private void GetProductInfo()
    {
        _productInfo = _shopVm.SelectedProduct;
    }
    
    private void OnBuyButtonClicked(PointerEventData data)
    {
        if (IsDailyProduct)
        {
            _shopVm.BuyDailyProduct();
        }
        else
        {
            _shopVm.BuyProduct();
        }
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
