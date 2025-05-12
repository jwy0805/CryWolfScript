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
        
        InitSpecialPackage();
        InitBeginnerPackages();
        
        InitReservedPackages();
        InitDailyProducts();
        InitPackages(_shopVm.GoldPackages, _goldStorePanel);
        InitPackages(_shopVm.SpinelPackages, _spinelStorePanel);
        InitGoldItems();
        InitSpinelItems();
    }

    private void InitSpecialPackage()
    {
        foreach (var productInfo in _shopVm.SpecialPackages)
        {
            var itemName = ((ProductId)productInfo.Id).ToString();
            var item = itemName == ((ProductId)1).ToString()
                ? InitiateProduct(itemName, productInfo, _specialPackagePanel.GetChild(0))
                : InitiateProduct(itemName, productInfo, _specialPackagePanel.GetChild(1));
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(OnProductClicked);
        }
    }
    
    private void InitBeginnerPackages()
    {
        foreach (var productInfo in _shopVm.BeginnerPackages)
        {
            var itemName = ((ProductId)productInfo.Id).ToString();
            var item = InitiateProduct(itemName, productInfo, _beginnerPackagePanel);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(OnProductClicked);
        }
    }

    private void InitReservedPackages()
    {
        foreach (var productInfo in _shopVm.ReservedSales)
        {
            var itemName = ((ProductId)productInfo.Id).ToString();
            var item = InitiateProduct(itemName, productInfo, _reservedSalePanel);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            // var frameObject = Util.FindChild(item, "ItemIcon", true, true);
            
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(OnReservedSalesClicked);
        }
    }
    
    private void InitPackages(IEnumerable<ProductInfo> packages, Transform panel)
    {
        foreach (var productInfo in packages)
        {
            var itemName = ((ProductId)productInfo.Id).ToString();
            var item = InitiateProduct(itemName, productInfo, panel);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            // var frameObject = Util.FindChild(item, "ItemIcon", true, true);
            
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(OnProductClicked);
        }
    }
    
    private void InitGoldItems()
    {
        foreach (var productInfo in _shopVm.GoldItems)
        {
            var itemName = ((ProductId)productInfo.Id).ToString();
            var item = InitiateProduct(itemName, productInfo, _goldPackagePanel);
            var countText = Util.FindChild(item, "TextNum", true, true);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            // var frameObject = Util.FindChild(item, "ItemIcon", true, true);

            countText.GetComponent<TextMeshProUGUI>().text = productInfo.Compositions
                .FirstOrDefault(c => c.Id == productInfo.Id)?
                .Count.ToString();
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(OnProductClicked);
        }
    }
    
    private void InitSpinelItems()
    {
        foreach (var productInfo in _shopVm.SpinelItems)
        {
            var itemName = ((ProductId)productInfo.Id).ToString();
            var item = InitiateProduct(itemName, productInfo, _spinelPackagePanel);
            var countText = Util.FindChild(item, "TextNum", true, true);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            
            countText.GetComponent<TextMeshProUGUI>().text = productInfo.Compositions
                .FirstOrDefault(c => c.Id == productInfo.Id)?
                .Count.ToString();
            priceText.GetComponent<TextMeshProUGUI>().text = "KRW " + productInfo.Price.ToString("N0");
            item.BindEvent(OnProductClicked);
        }
    }
    
    private void InitDailyProducts()
    {
        foreach (var dailyProductInfo in _shopVm.DailyProducts)
        {
            var productInfo = dailyProductInfo.ProductInfo;
            var composition = productInfo.Compositions;
            var itemName = ((ProductId)productInfo.Id).ToString();
            var item = InitiateDailyProduct(itemName, dailyProductInfo, _dailyProductPanel);
            var countText = Util.FindChild(item, "TextNum", true, true);
            var priceText = Util.FindChild(item, "TextPrice", true, true);

            if (composition.Count == 1 && composition.First().Type == ProductType.Unit && composition.First().Count > 1)
            {
                if (dailyProductInfo.NeedAds)
                {
                    countText.SetActive(false);
                }
                else
                {
                    // If the product is a unit and has a count greater than 1
                    countText.GetComponent<TextMeshProUGUI>().text = $"x{composition.First().Count.ToString()}";
                }
            }
            else
            {
                countText.SetActive(false);
            }

            if (productInfo.Price != 0)
            {
                priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            }

            if (dailyProductInfo.Slot < 3)
            {
                item.BindEvent(OnProductClicked);
            }
            else
            {
                item.BindEvent(data => OnAdsProductClicked(data, dailyProductInfo));
            }
        }
    }

    private GameObject InitiateProduct(string prefabPath, ProductInfo productInfo, Transform parent)
    {
        var panel = Managers.Resource.Instantiate($"UI/Shop/{prefabPath}", parent);
        var product = panel.GetOrAddComponent<GameProduct>();
        product.Init();
        product.ProductInfo = productInfo;
        
        return panel;
    }

    private GameObject InitiateDailyProduct(string itemName, DailyProductInfo dailyProductInfo, Transform parent)
    {
        var framePath = _shopVm.GetDailyProductFramePath(itemName, dailyProductInfo);
        var frame = Managers.Resource.Instantiate(framePath, parent);
        var productInfo = dailyProductInfo.ProductInfo;
        var product = frame.GetOrAddComponent<GameProduct>();
        product.Init();
        product.ProductInfo = productInfo;
        
        // Bind Product Image
        string iconPath;

        if (dailyProductInfo.NeedAds)
        {
            iconPath = "UI/Shop/NormalizedProducts/Ads";
            var iconObject = Managers.Resource.Instantiate(iconPath, frame.transform);
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.65f);
            iconRect.anchorMax = new Vector2(0.5f, 0.65f);
            iconRect.sizeDelta = new Vector2(216, 216);
            iconObject.transform.SetSiblingIndex(3);
        }
        else
        {
            if (productInfo.Compositions.Count == 1 && productInfo.Compositions.First().Type == ProductType.Unit)
            {
                var unitId = productInfo.Compositions.First().CompositionId;
                var unit = Managers.Data.UnitInfoDict[unitId];
                var iconObject = Managers.Resource.GetCardResources<UnitId>(unit, frame.transform);
                var iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.6f);
                iconRect.anchorMax = new Vector2(0.5f, 0.6f);
                iconRect.sizeDelta = new Vector2(135, 216);
                iconObject.transform.SetSiblingIndex(3);
            }
            else
            {
                iconPath = $"UI/Shop/NormalizedProducts/{itemName}";
                var iconObject = Managers.Resource.Instantiate(iconPath, frame.transform);
                var iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.6f);
                iconRect.anchorMax = new Vector2(0.5f, 0.6f);
                iconObject.transform.SetSiblingIndex(3);
            }
        }
        
        return frame;
    }
}
