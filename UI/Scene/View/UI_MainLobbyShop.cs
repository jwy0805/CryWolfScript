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

/* Last Modified : 24. 10. 30
 * Version : 1.02
 */

// This class includes the shop logics for the main lobby UI. 
public partial class UI_MainLobby
{
    private async Task InitShop()
    {
        _specialPackagePanel = GetImage((int)Images.SpecialPackagePanel).transform;
        _beginnerPackagePanel = GetImage((int)Images.BeginnerPackagePanel).transform;
        _reservedSalePanel = GetImage((int)Images.ReservedSalePanel).transform;
        _dailyProductPanel = GetImage((int)Images.DailyDealProductPanel).transform;
        _goldStorePanel = GetImage((int)Images.GoldStorePanel).transform;
        _spinelStorePanel = GetImage((int)Images.SpinelStorePanel).transform;
        _goldPackagePanel = GetImage((int)Images.GoldPackagePanel).transform;
        _spinelPackagePanel = GetImage((int)Images.SpinelPackagePanel).transform;
        
        await _shopVm.Initialize();
        await Task.WhenAll(InitSpecialPackage(),
            InitBeginnerPackages(),
            InitReservedPackages(),
            InitDailyProducts(),
            InitPackages(_shopVm.GoldPackages, _goldStorePanel),
            InitPackages(_shopVm.SpinelPackages, _spinelStorePanel),
            InitGoldItems(),
            InitSpinelItems(),
            InitDailyPanelObjects(),
            InitSubscriptionObjects());
        
        Debug.Log("Shop initialized successfully.");
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
            item.BindEvent(OnProductClicked);
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
            item.BindEvent(OnProductClicked);
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
            item.BindEvent(OnProductClicked);
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
            item.BindEvent(OnProductClicked);
        }
    }
    
    private async Task InitSpinelItems()
    {
        foreach (var productInfo in _shopVm.SpinelItems)
        {
            var itemName = ((ProductId)productInfo.ProductId).ToString();
            var item = await InitiateProduct(itemName, productInfo, _spinelPackagePanel);
            var countText = Util.FindChild(item, "TextNum", true, true);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            
            countText.GetComponent<TextMeshProUGUI>().text = productInfo.Compositions
                .FirstOrDefault(c => c.ProductId == productInfo.ProductId)?
                .Count.ToString();
            priceText.GetComponent<TextMeshProUGUI>().text = _paymentService.GetLocalizedPrice(productInfo.ProductCode);
            item.BindEvent(OnProductClicked);
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

            if (dailyProductInfo.Slot < 3)
            {
                if (dailyProductInfo.Bought)
                {
                    await SoldOutDailyProduct(dailyProductInfo.Slot);
                }
                else
                {
                    item.BindEvent(OnDailyProductClicked);
                }
            }
            else
            {
                if (dailyProductInfo.Bought)
                {
                    await SoldOutDailyProduct(dailyProductInfo.Slot);
                }
                else
                {
                    item.BindEvent(data => OnAdsProductClicked(data, dailyProductInfo));
                }
            }
        }
    }

    // Ads remover 같이 구독형 상품인 경우는 기존 상품과 다른 방식으로 처리
    private async Task InitDailyPanelObjects()
    {
        await SetRefreshTimer();
    }

    private async Task InitSubscriptionObjects()
    {
        await Task.WhenAll(InitAdsRemover());
    }

    private async Task InitAdsRemover()
    {
        var adsRemoverObj = GetButton((int)Buttons.AdsRemover).gameObject;
        var product = adsRemoverObj.GetOrAddComponent<ProductSimple>();
        var adsRemover = adsRemoverObj.GetOrAddComponent<AdsRemover>();
        var priceText = Util.FindChild(adsRemoverObj, "TextPrice", true, true);
        
        adsRemover.Applied = User.Instance.SubscribeAdsRemover;
        priceText.GetComponent<TextMeshProUGUI>().text = _shopVm.AdsRemover.Price.ToString();
        product.ProductInfo = _shopVm.AdsRemover;
        product.Init();
        
        adsRemoverObj.BindEvent(OnAdsRemoverClicked);
    }

    private void OnActiveAdsRemover()
    {
        User.Instance.SubscribeAdsRemover = true;
        
        var adsRemover = GetButton((int)Buttons.AdsRemover).gameObject;
        var flag = Util.FindChild(adsRemover, "AppliedFlag", true, true);
        flag.SetActive(true);
    }
    
    private async Task SetRefreshTimer()
    {
        var refreshButton = GetButton((int)Buttons.DailyProductsRefreshButton).gameObject;
        refreshButton.BindEvent(OnRefreshDailyProductsClicked);
        
        var timer = refreshButton.GetOrAddComponent<TimerSeconds>();
        timer.TimerText = GetText((int)Texts.DailyProductsRefreshButtonTimeText);
        
        await _shopVm.InitDailyProductRefreshTime();
        timer.LastRefreshTime = _shopVm.LastDailyProductRefreshTime;
    }

    private async Task<GameObject> InitiateProduct(string prefabPath, ProductInfo productInfo, Transform parent)
    {
        var panel = await Managers.Resource.Instantiate($"UI/Shop/{prefabPath}", parent);
        var product = panel.GetOrAddComponent<GameProduct>();
        product.ProductInfo = productInfo;
        product.Init();
        
        return panel;
    }

    private async Task<GameObject> InitiateDailyProduct(string itemName, DailyProductInfo dailyProductInfo, Transform parent)
    {
        var framePath = _shopVm.GetDailyProductFramePath(itemName, dailyProductInfo);
        var frame = await Managers.Resource.Instantiate(framePath, parent);
        var productInfo = dailyProductInfo.ProductInfo;
        var product = frame.GetOrAddComponent<ProductSimple>();
        product.ProductInfo = productInfo;
        product.Init();
        
        // Bind Product Image
        string iconPath;

        if (dailyProductInfo.NeedAds)
        {
            iconPath = "UI/Shop/NormalizedProducts/Ads";
            var iconObject = await Managers.Resource.Instantiate(iconPath, frame.transform);
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.65f);
            iconRect.anchorMax = new Vector2(0.5f, 0.65f);
            iconRect.sizeDelta = new Vector2(216, 216);
            iconObject.transform.SetSiblingIndex(3);
        }
        else
        {
            if (productInfo.Compositions.Count == 1 && productInfo.Compositions.First().ProductType == ProductType.Unit)
            {
                var unitId = productInfo.Compositions.First().CompositionId;
                var unit = Managers.Data.UnitInfoDict[unitId];
                var iconObject = await Managers.Resource.GetCardResources<UnitId>(unit, frame.transform);
                var iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(135, 216);
                iconRect.anchorMin = new Vector2(0.5f, 0.6f);
                iconRect.anchorMax = new Vector2(0.5f, 0.6f);
                iconObject.transform.SetSiblingIndex(3);
            }
            else if (productInfo.Compositions.First().ProductType == ProductType.Gold || 
                     productInfo.Compositions.First().ProductType == ProductType.Spinel)
            {
                var textNum = Util.FindChild(frame, "TextNum", true);
                textNum.SetActive(true);
            }
            else
            {
                iconPath = $"UI/Shop/NormalizedProducts/{itemName}";
                var iconObject = await Managers.Resource.Instantiate(iconPath, frame.transform);
                var iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.6f);
                iconRect.anchorMax = new Vector2(0.5f, 0.6f);
                iconObject.transform.SetSiblingIndex(3);
            }
        }
        
        return frame;
    }
    
    private async Task RevealDailyProduct(DailyProductInfo dailyProduct)
    {
        var result = await _shopVm.RevealDailyProduct(dailyProduct);

        if (result == false)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        }
        else
        {
            var revealedProduct = _dailyProductPanel.GetChild(dailyProduct.Slot);
            var adsIcon = Util.FindChild(revealedProduct.gameObject, "Ads", true);
            if (adsIcon != null)
            {
                adsIcon.SetActive(false);
            }
            
            var productInfo = dailyProduct.ProductInfo;
            var itemName = ((ProductId)productInfo.ProductId).ToString();

            GameObject iconObject;
            RectTransform iconRect;
            if (productInfo.Compositions.Count == 1 && productInfo.Compositions.First().ProductType == ProductType.Unit)
            {
                var unitId = productInfo.Compositions.First().CompositionId;
                var unit = Managers.Data.UnitInfoDict[unitId];
                iconObject = await Managers.Resource.GetCardResources<UnitId>(unit, revealedProduct);
                iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(135, 216);
            }
            else if (productInfo.Compositions.First().ProductType == ProductType.Gold || 
                     productInfo.Compositions.First().ProductType == ProductType.Spinel)
            {
                var textNum = Util.FindChild(revealedProduct.gameObject, "TextNum", true, true);
                var iconPath = productInfo.Compositions.First().ProductType switch
                {
                    ProductType.Gold => Managers.Resource.GetGoldPrefabPath(productInfo.Compositions.First().Count),
                    _ => Managers.Resource.GetSpinelPrefabPath(productInfo.Compositions.First().Count),
                };
                
                textNum.SetActive(true);
                textNum.GetComponent<TextMeshProUGUI>().text = $"x{productInfo.Compositions.First().Count.ToString()}";
                iconObject = await Managers.Resource.Instantiate(iconPath, revealedProduct);
                iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(250, 250);
            }
            else
            {
                var iconPath = $"UI/Shop/NormalizedProducts/{itemName}";
                iconObject = await Managers.Resource.Instantiate(iconPath, revealedProduct);
                iconRect = iconObject.GetComponent<RectTransform>();
            }
            
            iconRect.anchorMin = new Vector2(0.5f, 0.6f);
            iconRect.anchorMax = new Vector2(0.5f, 0.6f);
            iconObject.transform.SetSiblingIndex(3);
        }
    }

    private async Task RefreshDailyProducts()
    {
        var result = await _shopVm.RefreshDailyProducts();

        if (result == false)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        }
        else
        {
            foreach (Transform child in _dailyProductPanel)
            {
                Managers.Resource.Destroy(child.gameObject);
            }
            
            await SetRefreshTimer();
            await InitDailyProducts();
        }
    }

    private async Task OnDailyPaymentSuccessHandler(int slot)
    {
        Debug.Log("[UI_MainLobby] Daily product purchase successful.");
        await Task.WhenAll(SoldOutDailyProduct(slot), InitUserInfo());
    }
    
    private async Task SoldOutDailyProduct(int slot)
    {
        const string soldOutFramePath = "UI/Shop/SoldOut";
        Managers.Resource.Destroy(_dailyProductPanel.GetChild(slot).gameObject);
        var soldOutPanel = await Managers.Resource.Instantiate(soldOutFramePath, _dailyProductPanel);
        soldOutPanel.transform.SetSiblingIndex(slot);
    }
}
