using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class LobbyShopWidget
{
    private readonly IUserService _userService;
    private readonly ShopViewModel _shopVm;
    private readonly IPaymentService _paymentService;
    
    private Transform _specialPackagePanel;
    private Transform _beginnerPackagePanel;
    private Transform _reservedSalePanel;
    private Transform _dailyProductPanel;
    private Transform _goldStorePanel;
    private Transform _spinelStorePanel;
    private Transform _goldPackagePanel;
    private Transform _spinelPackagePanel;
    private AdsRemover _adsRemover;
    private Button _refreshButton;
    private TextMeshProUGUI _refreshButtonTimerText;
    private bool _spinelItemsInitialized;
    
    private readonly Func<Task> _initUserInfo;
    
    public LobbyShopWidget(IUserService userService, 
        ShopViewModel shopVm,
        IPaymentService paymentService, 
        Func<Task> initUserInfo)
    {
        _userService = userService;
        _shopVm = shopVm;
        _paymentService = paymentService;
        _initUserInfo = initUserInfo;
        
        ReleaseEvents();
        BindEvents();
    }

    public void BindViews(
        Transform specialPackage,
        Transform beginnerPackage,
        Transform reservedSale,
        Transform dailyProduct,
        Transform goldStore,
        Transform spinelStore,
        Transform goldPackage,
        Transform spinelPackage,
        AdsRemover adsRemover,
        Button refreshButton,
        TextMeshProUGUI refreshButtonTimerText)
    {
        _specialPackagePanel = specialPackage;
        _beginnerPackagePanel = beginnerPackage;
        _reservedSalePanel = reservedSale;
        _dailyProductPanel = dailyProduct;
        _goldStorePanel = goldStore;
        _spinelStorePanel = spinelStore;
        _goldPackagePanel = goldPackage;
        _spinelPackagePanel = spinelPackage;
        _adsRemover = adsRemover;
        _refreshButton = refreshButton;
        _refreshButtonTimerText = refreshButtonTimerText;
    }

    public async Task InitShop()
    {
        await _shopVm.Initialize();
        await Task.WhenAll(
            InitSpecialPackage(),
            InitBeginnerPackages(),
            InitReservedPackages(),
            InitDailyProducts(),
            InitPackages(_shopVm.GoldPackages, _goldStorePanel),
            InitPackages(_shopVm.SpinelPackages, _spinelStorePanel),
            InitGoldItems(),
            InitDailyPanelObjects(),
            InitSubscriptionObjects());
        
        // Spinel만 IAP 준비되면 붙인다
        HookSpinelInitAfterIap();
    }
    
    private void HookSpinelInitAfterIap()
    {
        // 이미 초기화 완료면 즉시 실행
        if (_paymentService.IsInitialized)
        {
            _ = InitSpinelItemsOnce();
            return;
        }

        _paymentService.OnIapReady -= OnIapInitializedForSpinel; 
        _paymentService.OnIapReady += OnIapInitializedForSpinel;
    }
    
    private void OnIapInitializedForSpinel()
    {
        _paymentService.OnIapReady -= OnIapInitializedForSpinel;
        _ = InitSpinelItemsOnce();
    }
    
    private async Task InitSpecialPackage()
    {
        foreach (var productInfo in _shopVm.SpecialPackages)
        {
            var itemName = ((ProductId)productInfo.ProductId).ToString();
            var item = itemName == ((ProductId)1).ToString()
                ? await InitiateProduct(itemName, productInfo, _specialPackagePanel.GetChild(0))
                : await InitiateProduct(itemName, productInfo, _specialPackagePanel.GetChild(1));
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(data => OnProductClicked(data));
        }
    }
    
    private async Task InitBeginnerPackages()
    {
        foreach (var productInfo in _shopVm.BeginnerPackages)
        {
            var itemName = ((ProductId)productInfo.ProductId).ToString();
            var item = await InitiateProduct(itemName, productInfo, _beginnerPackagePanel);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(data => OnProductClicked(data));
        }
    }

    private async Task InitReservedPackages()
    {
        foreach (var productInfo in _shopVm.ReservedSales)
        {
            var itemName = ((ProductId)productInfo.ProductId).ToString();
            var item = await InitiateProduct(itemName, productInfo, _reservedSalePanel);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            // var frameObject = Util.FindChild(item, "ItemIcon", true, true);
            
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(OnReservedSalesClicked);
        }
    }
    
    private async Task InitPackages(IEnumerable<ProductInfo> packages, Transform panel)
    {
        foreach (var productInfo in packages)
        {
            var itemName = ((ProductId)productInfo.ProductId).ToString();
            var item = await InitiateProduct(itemName, productInfo, panel);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            // var frameObject = Util.FindChild(item, "ItemIcon", true, true);
            
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(data => OnProductClicked(data));
        }
    }
    
    private async Task InitGoldItems()
    {
        foreach (var productInfo in _shopVm.GoldItems)
        {
            var itemName = ((ProductId)productInfo.ProductId).ToString();
            var item = await InitiateProduct(itemName, productInfo, _goldPackagePanel);
            var countText = Util.FindChild(item, "TextNum", true, true);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            // var frameObject = Util.FindChild(item, "ItemIcon", true, true);

            countText.GetComponent<TextMeshProUGUI>().text = productInfo.Compositions
                .FirstOrDefault(c => c.ProductId == productInfo.ProductId)?
                .Count.ToString();
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(data => OnProductClicked(data));
        }
    }
    
    private async Task InitSpinelItemsOnce()
    {
        if (_spinelItemsInitialized) return;
        _spinelItemsInitialized = true;

        await InitSpinelItems();
        BindLocalizedPrices(); // IAP 완료 후이므로 여기서 가격 바인딩까지 같이
    }
    
    private async Task InitSpinelItems()
    {
        foreach (var productInfo in _shopVm.SpinelItems)
        {
            var itemName = ((ProductId)productInfo.ProductId).ToString();
            var item = await InitiateProduct(itemName, productInfo, _spinelPackagePanel);
            var countText = Util.FindChild(item, "TextNum", true, true);
            if (countText != null)
            {
                var count = productInfo.Compositions
                    .FirstOrDefault(c => c.ProductId == productInfo.ProductId)?
                    .Count ?? 0;

                var tmp = countText.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = count.ToString();
            }

            var button = item.GetOrAddComponent<Button>();
            button.onClick.RemoveAllListeners();

            var productCode = productInfo.ProductCode; 
            button.onClick.AddListener(() => _shopVm.BuyCashProduct(productCode));
        }
    }

    private void BindLocalizedPrices()
    {
        foreach (Transform child in _spinelPackagePanel)
        {
            var product = child.GetComponent<GameProduct>();
            if (product == null) continue;

            var priceText = Util.FindChild(child.gameObject, "TextPrice", true, true);
            if (priceText == null) continue;

            var tmp = priceText.GetComponent<TextMeshProUGUI>();
            if (tmp == null) continue;

            // 텍스트가 클릭을 가로채지 않도록(자주 발생)
            tmp.raycastTarget = false;

            var code = product.ProductInfo?.ProductCode;
            if (string.IsNullOrEmpty(code))
            {
                tmp.text = "-";
                continue;
            }

            var localizedPrice = _paymentService.GetLocalizedPrice(code);
            tmp.text = string.IsNullOrEmpty(localizedPrice) ? "-" : localizedPrice;
        }
    }
    
    private async Task InitDailyProducts()
    {
        foreach (var dailyProductInfo in _shopVm.DailyProducts)
        {
            var productInfo = dailyProductInfo.ProductInfo;
            var composition = productInfo.Compositions;
            var itemName = ((ProductId)productInfo.ProductId).ToString();
            var item = await InitiateDailyProduct(itemName, dailyProductInfo, _dailyProductPanel);
            var countText = Util.FindChild(item, "TextNum", true, true);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            
            if (dailyProductInfo.NeedAds)
            {
                countText.SetActive(false);
            }
            else
            {
                if (composition.Count == 1 && composition.First().ProductType == ProductType.Unit && composition.First().Count > 1)
                {
                    // If the product is a unit and has a count greater than 1
                    countText.GetComponent<TextMeshProUGUI>().text = $"x{composition.First().Count.ToString()}";
                }
                else if (productInfo.Compositions.First().ProductType == ProductType.Gold || 
                         productInfo.Compositions.First().ProductType == ProductType.Spinel)       
                {
                    countText.GetComponent<TextMeshProUGUI>().text = $"x{composition.First().Count.ToString()}";
                }
                else
                {
                    countText.SetActive(false);
                }
            }

            if (productInfo.Price != 0)
            {
                priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            }

            if (dailyProductInfo.Bought)
            {
                await SoldOutDailyProduct(dailyProductInfo.Slot);
            }
            else
            {
                if (dailyProductInfo.NeedAds)
                {
                    item.BindEventOne(data => OnAdsProductClicked(data, dailyProductInfo));
                }
                else
                {
                    item.BindEventOne(OnDailyProductClicked);
                }
            }
        }
    }
    
    // Ads remover 같이 구독형 상품인 경우는 기존 상품과 다른 방식으로 처리
    private async Task InitDailyPanelObjects()
    {
        await SetRefreshTimer();
    }

    public async Task InitSubscriptionObjects()
    {
        await InitAdsRemover();
    }

    private Task InitAdsRemover()
    {
        var go = _adsRemover.gameObject;
        if (!go.TryGetComponent<ProductSimple>(out var product)) product = go.AddComponent<ProductSimple>();
        var priceText = Util.FindChild(_adsRemover.gameObject, "TextPrice", true, true);
        
        _adsRemover.Applied = _userService.User.SubscribeAdsRemover;
        priceText.GetComponent<TextMeshProUGUI>().text = _shopVm.AdsRemover.Price.ToString();
        product.ProductInfo = _shopVm.AdsRemover;
        product.Init();
        
        _adsRemover.gameObject.BindEventOne(OnAdsRemoverClicked);
        
        return Task.CompletedTask;
    }
    
    private async Task SetRefreshTimer()
    {
        _refreshButton.gameObject.BindEventOne(OnRefreshDailyProductsClicked);

        var go = _refreshButton.gameObject;
        if (!go.TryGetComponent<TimerSeconds>(out var timer)) timer = go.AddComponent<TimerSeconds>();
        timer.TimerText = _refreshButtonTimerText;
        timer.AdsImage = Util.FindChild(go, "Icon", true).GetComponent<Image>();
        
        await _shopVm.InitDailyProductRefreshTime();
        timer.LastRefreshTime = _shopVm.LastDailyProductRefreshTime;
    }
    
    private async Task<GameObject> InitiateProduct(string prefabPath, ProductInfo productInfo, Transform parent)
    {
        var panel = await Managers.Resource.Instantiate($"UI/Shop/{prefabPath}", parent);
        if (!panel.TryGetComponent<GameProduct>(out var product)) product = panel.AddComponent<GameProduct>();
        product.ProductInfo = productInfo;
        product.Init();
        
        return panel;
    }

    private async Task<GameObject> InitiateDailyProduct(
        string itemName, DailyProductInfo dailyProductInfo, Transform parent)
    {
        var framePath = _shopVm.GetDailyProductFramePath(itemName, dailyProductInfo);
        var frame = await Managers.Resource.Instantiate(framePath, parent);
        var productInfo = dailyProductInfo.ProductInfo;
        var product = frame.GetOrAddComponent<ProductSimple>();
        product.ProductInfo = productInfo;
        product.Init();
        
        // Bind Product Image
        string iconPath;
        GameObject iconObject;
        if (productInfo.Compositions.Count == 1 && productInfo.Compositions.First().ProductType == ProductType.Unit)
        {
            var unitId = productInfo.Compositions.First().CompositionId;
            var unit = Managers.Data.UnitInfoDict[unitId];
            iconObject = await Managers.Resource.GetCardResources<UnitId>(unit, frame.transform);
            iconObject.transform.SetSiblingIndex(3);
            
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(135, 216);
            iconRect.anchorMin = new Vector2(0.5f, 0.6f);
            iconRect.anchorMax = new Vector2(0.5f, 0.6f);
        }
        else if (productInfo.Compositions.First().ProductType == ProductType.Gold || 
                 productInfo.Compositions.First().ProductType == ProductType.Spinel)
        {
            iconObject = Util.FindChild(frame, "ItemIcon", true);
            
            var textNum = Util.FindChild(frame, "TextNum", true);
            textNum.SetActive(true);
        }
        else
        {
            iconPath = $"UI/Shop/NormalizedProducts/{itemName}";
            iconObject = await Managers.Resource.Instantiate(iconPath, frame.transform);
            iconObject.transform.SetSiblingIndex(3);
            
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(216, 216);
            iconRect.anchorMin = new Vector2(0.5f, 0.6f);
            iconRect.anchorMax = new Vector2(0.5f, 0.6f);
        }

        if (dailyProductInfo.NeedAds)
        {
            iconPath = "UI/Shop/NormalizedProducts/Ads";
            
            var adsObject = await Managers.Resource.Instantiate(iconPath, frame.transform);
            adsObject.transform.SetSiblingIndex(4);
            
            var iconRect = adsObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.65f);
            iconRect.anchorMax = new Vector2(0.5f, 0.65f);
            iconRect.sizeDelta = new Vector2(216, 216);
        }
        
        iconObject.AddComponent<DailyProductHelper>();
        iconObject.SetActive(!dailyProductInfo.NeedAds);
        
        return frame;
    }
    
    private async Task RevealDailyProduct(DailyProductInfo dailyProduct)
    {
        var slot = await _shopVm.RevealDailyProduct(dailyProduct);
        if (slot == -1)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            await Managers.Localization.UpdateNotifyPopupText(
                popup, "notify_network_error_title", "notify_network_error_message");
            return;
        }
        
        var childObject = _dailyProductPanel.GetChild(slot).gameObject;
        var adsImage = Util.FindChild(childObject, "Ads", true, true);
        var productImage = childObject.transform.GetComponentInChildren<DailyProductHelper>(true);
        childObject.BindEventOne(OnDailyProductClicked);
        adsImage.SetActive(false);
        productImage.gameObject.SetActive(true);
    }

    private async Task RefreshDailyProducts()
    {
        var result = await _shopVm.RefreshDailyProducts();
        if (!result)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            await Managers.Localization.UpdateNotifyPopupText(
                popup, "notify_network_error_title", "notify_network_error_message");
        }
        else
        {
            await SetRefreshTimer();
            await OnResetDailyProductUI();
        }
    }

    private async Task OnDailyPaymentSuccessHandler(int slot)
    {
        await SoldOutDailyProduct(slot);
        await _initUserInfo();
    }
    
    private async Task SoldOutDailyProduct(int slot)
    {
        const string soldOutFramePath = "UI/Shop/SoldOut";
        Managers.Resource.Destroy(_dailyProductPanel.GetChild(slot).gameObject);
        var soldOutPanel = await Managers.Resource.Instantiate(soldOutFramePath, _dailyProductPanel);
        soldOutPanel.transform.SetSiblingIndex(slot);
    }
    
    private async Task OnResetDailyProductUI()
    {
        Util.DestroyAllChildren(_dailyProductPanel);
        
        for (int i = 0; i < 5; i++)
        {
            if (_dailyProductPanel.childCount == 0) break;
            await Managers.Coroutine.WaitUntilNextFrameAsync();
        }       
        
        await InitDailyProducts();
    }

    private async Task ApplyAdsRemoverUI()
    {
        _userService.User.SubscribeAdsRemover = true;
        await InitAdsRemover();
    }
    
    private async Task OnProductClicked(PointerEventData data, string localizedPrice = null)
    {
        GameProduct product = null;
        var go = data.pointerPress.gameObject;
        if (go.TryGetComponent(out ProductSimple productSimple))
        {
            if (productSimple.IsDragging) return;
            var simplePopup = await Managers.UI.ShowPopupUI<UI_ProductInfoSimplePopup>();
            if (localizedPrice != null)
            {
                simplePopup.LocalizedPriceText = localizedPrice;
            }
            
            simplePopup.FrameObject = Object.Instantiate(go);
            simplePopup.FrameSize = go.GetComponent<RectTransform>().sizeDelta;
            product = productSimple;
            simplePopup.FrameObject.GetComponent<ProductSimple>().ProductInfo = product.ProductInfo;
        }
        
        if (go.TryGetComponent(out ProductPackage productPackage))
        {
            if (productPackage.IsDragging) return;
            var packagePopup = await Managers.UI.ShowPopupUI<UI_ProductInfoPopup>();
            packagePopup.FrameObject = Object.Instantiate(go);
            packagePopup.FrameSize = go.GetComponent<RectTransform>().sizeDelta;
            product = productPackage;
            packagePopup.FrameObject.GetComponent<ProductPackage>().ProductInfo = product.ProductInfo;
        }

        if (product == null) return;
        _shopVm.SelectedProduct = product.ProductInfo;
    }
    
    private async Task OnDailyProductClicked(PointerEventData data)
    {
        var go = data.pointerPress.gameObject;
        if (go.TryGetComponent(out ProductSimple productSimple))
        {
            if (productSimple.IsDragging) return;
            var simplePopup = await Managers.UI.ShowPopupUI<UI_ProductInfoSimplePopup>();
            simplePopup.IsDailyProduct = true;
            simplePopup.FrameObject = Object.Instantiate(go);
            simplePopup.FrameSize = go.GetComponent<RectTransform>().sizeDelta;
            simplePopup.FrameObject.GetComponent<ProductSimple>().ProductInfo = productSimple.ProductInfo;
            _shopVm.SelectedProduct = productSimple.ProductInfo;
        }
    }
    
    private async Task OnAdsRemoverClicked(PointerEventData data)
    {
        if (_userService.User.SubscribeAdsRemover) return;
        await OnProductClicked(data);
    }
    
    private async Task OnAdsProductClicked(PointerEventData data, DailyProductInfo dailyProductInfo)
    {
        var product = data.pointerPress.gameObject.GetComponent<GameProduct>();
        if (product == null || product.IsDragging) return;
        
        if (_userService.User.SubscribeAdsRemover)
        {
            await RevealDailyProduct(dailyProductInfo);
        }
        else
        {
            Managers.Ads.RevealedDailyProduct = dailyProductInfo;
            
            if (!Managers.Ads.IsRewardReady())
            {
                Managers.Ads.RequestRewardedPreload();  
                Managers.Ads.ShowLoadingOnly();         
                return;
            }
            
            Managers.Ads.ShowRewardVideo("Check_Daily_Product");
        }
    }

    private async Task OnRefreshDailyProductsClicked(PointerEventData data)
    {
        if (_userService.User.SubscribeAdsRemover)
        {
            await RefreshDailyProducts();
        }
        else
        {
            Managers.Ads.ShowRewardVideo("Refresh_Daily_Products");
        }
    }
    
    private async Task OnReservedSalesClicked(PointerEventData data)
    {
        var go = data.pointerPress.gameObject;
        if (go.TryGetComponent(out ProductPackage package) == false) return;
        if (package.IsDragging) return;
        var popup = await Managers.UI.ShowPopupUI<UI_ProductReservedInfoPopup>();
        var infoOrigin = Managers.Data.MaterialInfoDict[package.ProductInfo.Compositions[0].CompositionId];
        var info = new MaterialInfo { Id = infoOrigin.Id, Class = infoOrigin.Class };
        var parent = Util.FindChild(popup.gameObject, "Frame", true).transform;
        var size = popup.GetComponent<RectTransform>().sizeDelta.x * 0.42f;
        
        popup.FrameObject = await Managers.Resource.GetMaterialResources(info, parent);
        popup.FrameSize = new Vector2(size, size);
        _shopVm.SelectedProduct = package.ProductInfo;
    }
    
    public void OnRestorePurchaseClicked(PointerEventData data)
    {
        _paymentService.RestorePurchases();
    }
    
    private void BindEvents()
    {
        _shopVm.OnApplyAdsRemover += ApplyAdsRemoverUI;
        
        _paymentService.OnPaymentSuccess += InitSubscriptionObjects;
        _paymentService.OnDailyPaymentSuccess += OnDailyPaymentSuccessHandler;

        Managers.Ads.OnRewardedRevealDailyProduct += RevealDailyProduct;
        Managers.Ads.OnRewardedRefreshDailyProducts += RefreshDailyProducts;
    }

    private void ReleaseEvents()
    {
        _shopVm.OnApplyAdsRemover -= ApplyAdsRemoverUI;
        
        _paymentService.OnPaymentSuccess -= InitSubscriptionObjects;
        _paymentService.OnDailyPaymentSuccess -= OnDailyPaymentSuccessHandler;
        
        Managers.Ads.OnRewardedRevealDailyProduct -= RevealDailyProduct;
        Managers.Ads.OnRewardedRefreshDailyProducts -= RefreshDailyProducts;
    }

    public void DestroyShop()
    {
        Util.DestroyAllChildren(_specialPackagePanel.GetChild(0));
        Util.DestroyAllChildren(_specialPackagePanel.GetChild(1));
        Util.DestroyAllChildren(_beginnerPackagePanel);
        Util.DestroyAllChildren(_reservedSalePanel);
        Util.DestroyAllChildren(_dailyProductPanel);
        Util.DestroyAllChildren(_goldStorePanel);
        Util.DestroyAllChildren(_spinelStorePanel);
        Util.DestroyAllChildren(_goldPackagePanel);
        Util.DestroyAllChildren(_spinelPackagePanel);
    }
    
    public void Dispose()
    {
        ReleaseEvents();
    }
}
