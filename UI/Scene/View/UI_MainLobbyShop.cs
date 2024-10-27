using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* Last Modified : 24. 10. 18
 * Version : 1.016
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
        InitBeginnerPackage();
        InitReservedSales();
        InitDailyDeal();
        InitGoldPackage();
        InitSpinelPackage();
        InitGoldItems();
        InitSpinelItems();
    }

    private void InitSpecialPackage()
    {
        
    }
    
    private void InitBeginnerPackage()
    {
        
    }
    
    private void InitReservedSales()
    {
        
    }
    
    private void InitDailyDeal()
    {
        
    }
    
    private void InitGoldPackage()
    {
        foreach (var productInfo in _shopVm.GoldPackages)
        {
            var imageName = Util.ConvertStringToIconFormat(((ProductId)productInfo.Id).ToString());
            var item = InitiateProduct("GoldPackage", productInfo, _goldPackagePanel);
            var image = Util.FindChild(item, "Image_Package", true, true);
            var nameText = Util.FindChild(item, "Text_Name", true, true);
            var priceText = Util.FindChild(item, "Text_Cost", true, true);
            var productName = Util.ConvertStringToNameFormat(((ProductId)productInfo.Id).ToString());
            
            image.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>($"Sprites/ShopIcons/{imageName}");
            nameText.GetComponent<TextMeshProUGUI>().text = productName;
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            SetItemImageSize(item, _goldPackagePanel, "Image_Package");
        }
    }
    
    private void InitSpinelPackage()
    {
        foreach (var productInfo in _shopVm.SpinelPackages)
        {
            var imageName = Util.ConvertStringToIconFormat(((ProductId)productInfo.Id).ToString());
            var item = InitiateProduct("SpinelPackage", productInfo, _spinelPackagePanel);
            var image = Util.FindChild(item, "Image_Package", true, true);
            var nameText = Util.FindChild(item, "Text_Name", true, true);
            var priceText = Util.FindChild(item, "Text_Cost", true, true);
            var productName = Util.ConvertStringToNameFormat(((ProductId)productInfo.Id).ToString());
            
            image.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>($"Sprites/ShopIcons/{imageName}");
            nameText.GetComponent<TextMeshProUGUI>().text = productName;
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            SetItemImageSize(item, _spinelPackagePanel, "Image_Package");
        }
    }
    
    private void InitGoldItems()
    {
        foreach (var productInfo in _shopVm.GoldItems)
        {
            var imageName = Util.ConvertStringToIconFormat(((ProductId)productInfo.Id).ToString());
            var item = InitiateProduct("GoldItem", productInfo, _goldItemsPanel);
            var image = Util.FindChild(item, "Image_Gold", true, true);
            var countText = Util.FindChild(item, "Text_Gold", true, true);
            var priceText = Util.FindChild(item, "Text_Cost", true, true);
            
            image.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>($"Sprites/ShopIcons/{imageName}");
            countText.GetComponent<TextMeshProUGUI>().text = productInfo.Count.ToString();
            priceText.GetComponent<TextMeshProUGUI>().text = productInfo.Price.ToString();
            SetItemImageSize(item, _goldItemsPanel, "Image_Gold");
        }
    }
    
    private void InitSpinelItems()
    {
        foreach (var productInfo in _shopVm.SpinelItems)
        {
            var imageName = Util.ConvertStringToIconFormat(((ProductId)productInfo.Id).ToString());
            var item = InitiateProduct("SpinelItem", productInfo, _spinelItemsPanel);
            var image = Util.FindChild(item, "Image_Gem", true, true);
            var countText = Util.FindChild(item, "Text_Gem", true, true);
            var priceText = Util.FindChild(item, "Text_Cost", true, true);
            
            image.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>($"Sprites/ShopIcons/{imageName}");
            countText.GetComponent<TextMeshProUGUI>().text = productInfo.Count.ToString();
            priceText.GetComponent<TextMeshProUGUI>().text = "KRW" + productInfo.Price.ToString("N0");
            SetItemImageSize(item, _spinelItemsPanel, "Image_Gem");
        }
    }

    private GameObject InitiateProduct(string prefabPath, ProductInfo productInfo, Transform parent, 
        Action<PointerEventData> action = null)
    {
        var panel = Managers.Resource.Instantiate($"UI/Shop/{prefabPath}", parent);
        var product = panel.GetOrAddComponent<Product>();
        product.Id = productInfo.Id;
        product.Price = productInfo.Price;
        
        if (action != null) panel.BindEvent(action);
        
        return panel;
    }

    private void SetItemImageSize(GameObject item, Transform parent, string imageName, string glowName = "Glow")
    {
        var grid = parent.GetComponent<GridLayoutGroup>();
        var image = Util.FindChild(item, imageName, true, true);
        var glow = Util.FindChild(item, glowName, true, true);
        var imageRect = image.GetComponent<RectTransform>();
        var glowRect = glow.GetComponent<RectTransform>();
        
        imageRect.sizeDelta = new Vector2(grid.cellSize.x, grid.cellSize.x);
        glowRect.sizeDelta = new Vector2(grid.cellSize.x * 1.1f, grid.cellSize.x * 1.1f);
    }
}
