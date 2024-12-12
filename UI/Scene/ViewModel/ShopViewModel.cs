using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    
    public ProductInfo SelectedProduct { get; set; }
    public List<CompositionInfo> ProductsToBeClaimed { get; } = new()
    {
        new CompositionInfo { CompositionId = 60, Type = ProductType.None },
        new CompositionInfo { CompositionId = 61, Type = ProductType.None },
        new CompositionInfo { CompositionId = 4001, Type = ProductType.Gold },
        new CompositionInfo { CompositionId = 4002, Type = ProductType.Spinel },
        new CompositionInfo { CompositionId = 21, Type = ProductType.None },
        new CompositionInfo { CompositionId = 53, Type = ProductType.None },
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
        
        var productTask = _webService.SendWebRequestAsync<InitProductPacketResponse>(
            "Payment/InitProducts", UnityWebRequest.kHttpVerbPOST, productPacket);

        await productTask;
        
        var productResponse = productTask.Result;
        if (productResponse.GetProductOk == false) return;
        
        SpecialPackages = productResponse.SpecialPackages.OrderByDescending(pi => pi.Price).ToList();
        BeginnerPackages = productResponse.BeginnerPackages.OrderBy(pi => pi.Price).ToList();
        ReservedSales = productResponse.ReservedSales.OrderBy(pi => pi.Price).ToList();
        GoldPackages = productResponse.GoldPackages.OrderBy(pi => pi.Id).ToList();
        SpinelPackages = productResponse.SpinelPackages.OrderBy(pi => pi.Price).ToList();
        GoldItems = productResponse.GoldItems.OrderBy(pi => pi.Price).ToList();
        SpinelItems = productResponse.SpinelItems.OrderBy(pi => pi.Price).ToList();
    }

    public void BuyProduct()
    {
        if (SelectedProduct == null) return;
        if (SelectedProduct.CurrencyType == CurrencyType.Cash)
        {
            _paymentService.BuyCashProduct(SelectedProduct.ProductCode);
        }
        else
        {
            _paymentService.BuyProduct(SelectedProduct.ProductCode);
        }
    }
}
