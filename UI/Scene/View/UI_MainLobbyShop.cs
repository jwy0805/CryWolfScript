using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        _dailyDealPanel = GetImage((int)Images.DailyDealPanel).transform;
        _goldPackagePanel = GetImage((int)Images.GoldPackagePanel).transform;
        _spinelPackagePanel = GetImage((int)Images.SpinelPackagePanel).transform;
        _goldItemsPanel = GetImage((int)Images.GoldItemPanel).transform;
        _spinelItemsPanel = GetImage((int)Images.SpinelItemPanel).transform;
        
        await _shopVm.Initialize();
        
        InitSpecialPackage();
        InitPackages(_shopVm.BeginnerPackages, _beginnerPackagePanel);
        InitPackages(_shopVm.ReservedSales, _reservedSalePanel);
        InitDailyDeal();
        InitPackages(_shopVm.GoldPackages, _goldPackagePanel);
        InitPackages(_shopVm.SpinelPackages, _spinelPackagePanel);
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
            var frameObject = Util.FindChild(item, "ItemIcon", true, true);
            
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(data => OnProductClicked(data, frameObject));
        }
    }

    private void InitPackages(IEnumerable<ProductInfo> packages, Transform panel)
    {
        foreach (var productInfo in packages)
        {
            var itemName = ((ProductId)productInfo.Id).ToString();
            var item = InitiateProduct(itemName, productInfo, panel);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            var frameObject = Util.FindChild(item, "ItemIcon", true, true);
            
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(data => OnProductClicked(data, frameObject));
        }
    }
    
    private void InitDailyDeal()
    {
        
    }
    
    private void InitGoldItems()
    {
        foreach (var productInfo in _shopVm.GoldItems)
        {
            var itemName = ((ProductId)productInfo.Id).ToString();
            var item = InitiateProduct(itemName, productInfo, _goldItemsPanel);
            var countText = Util.FindChild(item, "TextNum", true, true);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            var frameObject = Util.FindChild(item, "ItemIcon", true, true);
            
            countText.GetComponent<TextMeshProUGUI>().text = productInfo.Compositions
                .FirstOrDefault(c => c.Id == productInfo.Id)?
                .Count.ToString();
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            item.BindEvent(data => OnProductClicked(data, frameObject));
        }
    }
    
    private void InitSpinelItems()
    {
        foreach (var productInfo in _shopVm.SpinelItems)
        {
            var itemName = ((ProductId)productInfo.Id).ToString();
            var item = InitiateProduct(itemName, productInfo, _spinelItemsPanel);
            var countText = Util.FindChild(item, "TextNum", true, true);
            var priceText = Util.FindChild(item, "TextPrice", true, true);
            
            countText.GetComponent<TextMeshProUGUI>().text = productInfo.Compositions
                .FirstOrDefault(c => c.Id == productInfo.Id)?
                .Count.ToString();
            priceText.GetComponent<TextMeshProUGUI>().text = "KRW " + productInfo.Price.ToString("N0");
        }
    }

    private GameObject InitiateProduct(string prefabPath, ProductInfo productInfo, Transform parent, 
        Action<PointerEventData> action = null)
    {
        var panel = Managers.Resource.Instantiate($"UI/Shop/{prefabPath}", parent);
        var product = panel.GetOrAddComponent<Product>();
        product.ProductInfo = productInfo;
        
        if (action != null) panel.BindEvent(action);
        
        return panel;
    }
}
