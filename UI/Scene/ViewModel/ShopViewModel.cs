using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

/* Last Modified : 24. 10. 30
 * Version : 1.02
 */

public class ShopViewModel
{
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    private readonly IPaymentService _paymentService;

    public List<ProductInfo> SpecialPackages;
    public List<ProductInfo> BeginnerPackages;
    public List<ProductInfo> ReservedSales;
    public List<ProductInfo> GoldPackages;
    public List<ProductInfo> SpinelPackages;
    public List<ProductInfo> GoldItems;
    public List<ProductInfo> SpinelItems;
    public List<DailyProductInfo> DailyProducts;
    
    public ProductInfo AdsRemover { get; set; }
    public ProductInfo SelectedProduct { get; set; }
    public DateTime LastDailyProductRefreshTime { get; set; }
    
    public List<CompositionInfo> ReservedProductsToBeClaimed { get; } = new()
    {
        new CompositionInfo { CompositionId = 60, ProductType = ProductType.None },
        new CompositionInfo { CompositionId = 61, ProductType = ProductType.None },
        new CompositionInfo { CompositionId = 4001, ProductType = ProductType.Gold },
        new CompositionInfo { CompositionId = 4002, ProductType = ProductType.Spinel },
        new CompositionInfo { CompositionId = 21, ProductType = ProductType.None },
        new CompositionInfo { CompositionId = 53, ProductType = ProductType.None },
    };

    public List<string> ReservedProductKeys { get; } = new()
    {
        "reserved_product_1",
        "reserved_product_2",
        "reserved_product_3",
        "reserved_product_4",
        "reserved_product_5",
        "reserved_product_6",
    };
    
    [Inject]
    public ShopViewModel(
        IUserService userService, 
        IWebService webService,
        ITokenService tokenService,
        IPaymentService paymentService)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
        _paymentService = paymentService;
        _paymentService.Init();
    }
    
    public async Task Initialize()
    {
        var productPacket = new InitProductPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken()
        };
        
        var productResponse = await _webService.SendWebRequestAsync<InitProductPacketResponse>(
            "Payment/InitProducts", UnityWebRequest.kHttpVerbPOST, productPacket);
        if (productResponse.GetProductOk == false) return;
        
        SpecialPackages = productResponse.SpecialPackages.OrderByDescending(pi => pi.Price).ToList();
        BeginnerPackages = productResponse.BeginnerPackages.OrderBy(pi => pi.Price).ToList();
        ReservedSales = productResponse.ReservedSales.OrderBy(pi => pi.Price).ToList();
        GoldPackages = productResponse.GoldPackages.OrderBy(pi => pi.ProductId).ToList();
        SpinelPackages = productResponse.SpinelPackages.OrderBy(pi => pi.Price).ToList();
        GoldItems = productResponse.GoldItems.OrderBy(pi => pi.Price).ToList();
        SpinelItems = productResponse.SpinelItems.OrderBy(pi => pi.Price).ToList();
        DailyProducts = productResponse.DailyProducts.OrderBy(pi => pi.Slot).ToList();
        AdsRemover = productResponse.AdsRemover;
    }

    public string GetDailyProductFramePath(string itemName, DailyProductInfo dailyProductInfo)
    {
        string framePath;
        var productInfo = dailyProductInfo.ProductInfo;
        
        if (productInfo.Price == 0)
        {
            // Free Product
            if (dailyProductInfo.NeedAds)
            {
                framePath = "UI/Shop/DailyProducts/CardFrameSquire";
            }
            else
            {
                if (productInfo.Compositions.Exists(c => c.ProductType == ProductType.Spinel))
                {
                    framePath = itemName switch
                    {
                        "Spinel50" => "UI/Shop/DailyProducts/Spinel50",
                        _ => "UI/Shop/DailyProducts/Spinel10"
                    };
                }
                else
                {
                    framePath = itemName switch
                    {
                        "Gold1000" => "UI/Shop/DailyProducts/Gold1000",
                        _ => "UI/Shop/DailyProducts/Gold100"
                    };
                }
            }
        }
        else
        {
            framePath = dailyProductInfo.Class switch
            {
                UnitClass.NobleKnight => "UI/Shop/DailyProducts/CardFrameNobleKnight",
                UnitClass.Baron => "UI/Shop/DailyProducts/CardFrameBaron",
                _ => "UI/Shop/DailyProducts/CardFrameKnight",
            };
        }

        return framePath;
    }
    
    public async Task BuyProduct()
    {
        if (SelectedProduct == null) return;
        if (SelectedProduct.CurrencyType == CurrencyType.Cash)
        {
            _paymentService.BuyCashProduct(SelectedProduct.ProductCode);
        }
        else
        {
            await _paymentService.BuyProductAsync(SelectedProduct.ProductCode);
        }
    }

    public void BuyDailyProduct()
    {
        if (SelectedProduct == null) return;
        _paymentService.BuyDailyProductAsync(SelectedProduct.ProductCode);
    }
    
    public async Task<bool> RevealDailyProduct(DailyProductInfo dailyProduct)
    {
        var packet = new RevealDailyProductPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Slot = dailyProduct.Slot,
        };
        
        var task = _webService.SendWebRequestAsync<RevealDailyProductPacketResponse>(
            "Payment/RevealDailyProduct", UnityWebRequest.kHttpVerbPUT, packet);
        await task;

        return task.Result.RevealDailyProductOk;
    }
    
    public async Task<bool> RefreshDailyProducts()
    {
        var packet = new RefreshDailyProductPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
        };
        
        var task = await _webService.SendWebRequestAsync<RefreshDailyProductPacketResponse>(
            "Payment/RefreshDailyProduct", UnityWebRequest.kHttpVerbPUT, packet);
        
        if (task.RefreshDailyProductOk)
        {
            DailyProducts = task.DailyProducts.OrderBy(pi => pi.Slot).ToList();
            LastDailyProductRefreshTime = DateTime.UtcNow;
        }

        return task.RefreshDailyProductOk;
    }
    
    public async Task ClaimProductFromMailbox(bool claimAll, MailInfo mailInfo = null)
    {
        var packet = new ClaimProductPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            CurrentState = RewardPopupType.None,
            ClaimAll = claimAll,
            MailId = mailInfo?.MailId ?? 0
        };
        
        var res = await _webService.SendWebRequestAsync<ClaimProductPacketResponse>(
            "Payment/ClaimProduct", UnityWebRequest.kHttpVerbPUT, packet);

        await HandleClaimPacketResponse(res);
    }

    public async Task CardSelected(CompositionInfo composition)
    {
        var packet = new SelectProductPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            SelectedCompositionInfo = composition,
        };
        
        var res = await _webService.SendWebRequestAsync<ClaimProductPacketResponse>(
            "Payment/SelectProduct", UnityWebRequest.kHttpVerbPUT, packet);

        await HandleClaimPacketResponse(res);
    }

    public async Task ClaimFixedAndDisplay()
    {
        var packet = new DisplayClaimedProductPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
        };
        
        var res = await _webService.SendWebRequestAsync<ClaimProductPacketResponse>(
            "Payment/DisplayClaimedProduct", UnityWebRequest.kHttpVerbPUT, packet);
        
        await HandleClaimPacketResponse(res);
    }

    private async Task HandleClaimPacketResponse(ClaimProductPacketResponse res)
    {
        if (res.ClaimOk)
        {
            Managers.UI.CloseAllPopupUI();

            switch (res.RewardPopupType)
            {
                case RewardPopupType.None:
                    break;
                case RewardPopupType.Item:
                    if (res.CompositionInfos.Count != 0)
                    {
                        var itemPopup = await Managers.UI.ShowPopupUI<UI_RewardItemPopup>();
                        itemPopup.CompositionInfos = res.CompositionInfos;
                    }
                    break;
                case RewardPopupType.Select:
                    if (res.ProductInfos.Count != 0)
                    {
                        var selectPopup = await Managers.UI.ShowPopupUI<UI_RewardSelectPopup>();
                        selectPopup.ProductInfo = res.ProductInfos.First();
                        selectPopup.CompositionInfos = res.CompositionInfos;
                    }                    
                    break;
                case RewardPopupType.Open:
                    if (res.RandomProductInfos.Count != 0)
                    {
                        var openPopup = await Managers.UI.ShowPopupUI<UI_RewardOpenPopup>();
                        openPopup.OriginalProductInfos = res.RandomProductInfos;
                        openPopup.CompositionInfos = res.CompositionInfos;
                    }
                    break;
            }
        }
    }
}
