using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

/* Last Modified : 24. 10. 18
 * Version : 1.016
 */

public class ShopViewModel
{
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;

    public List<ProductInfo> GoldPackages;
    public List<ProductInfo> SpinelPackages;
    public List<ProductInfo> GoldItems;
    public List<ProductInfo> SpinelItems;
    
    [Inject]
    public ShopViewModel(IUserService userService, IWebService webService, ITokenService tokenService)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
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
        
        GoldPackages = productResponse.GoldPackages.OrderBy(pi => pi.Id).ToList();
        SpinelPackages = productResponse.SpinelPackages.OrderBy(pi => pi.Price).ToList();
        GoldItems = productResponse.GoldItems.OrderBy(pi => pi.Price).ToList();
        SpinelItems = productResponse.SpinelItems.OrderBy(pi => pi.Price).ToList();
    }
}
