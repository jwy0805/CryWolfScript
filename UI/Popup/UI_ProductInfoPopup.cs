using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_ProductInfoPopup : UI_Popup
{
    private ShopViewModel _shopVm;
    private IUserService _userService;
    
    private GameObject _frame;
    private Image _icon;
    private ProductInfo _productInfo;
    
    public GameObject FrameObject { get; set; }

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
        
        _frame = GetImage((int)Images.Frame).gameObject;
        _icon = GetImage((int)Images.Icon);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.BuyButton).gameObject.BindEvent(OnBuyButtonClicked);
    }

    protected override void InitUI()
    {
        var iconPath = _productInfo.CurrencyType == CurrencyType.Spinel 
            ? "Sprites/ShopIcons/icon_spinel"
            : "Sprites/ShopIcons/icon_gold";
        
        _icon.sprite = Managers.Resource.Load<Sprite>(iconPath);
        FrameObject.transform.SetParent(_frame.transform);
        FrameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        
        GetText((int)Texts.TextName).text = ((ProductId)_productInfo.Id).ToString();
        // GetText((int)Texts.TextNum).text = _productInfo.Compositions.ToString();
        GetText((int)Texts.TextPrice).text = _productInfo.Price.ToString();
    }
    
    private void GetProductInfo()
    {
        _productInfo = _shopVm.SelectedProduct;
    }
    
    private void OnBuyButtonClicked(PointerEventData data)
    {
        
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
