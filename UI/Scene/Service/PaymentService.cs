using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Core;
using UnityEngine.Networking;
using UnityEngine.Purchasing.Extension;
using Zenject;

/* Last Modified : 25. 04. 22
 * Version : 1.02
 */

public class PaymentService : IPaymentService, IDetailedStoreListener
{
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    private IStoreController _storeController;

    private readonly HashSet<ProductDefinition> _products = new()
    {
        // new ProductDefinition("com.hamon.crywolf.consumable.over_power", UnityEngine.Purchasing.ProductType.Consumable),
        // new ProductDefinition("com.hamon.crywolf.consumable.beginning_of_the_legend1", UnityEngine.Purchasing.ProductType.Consumable),
        // new ProductDefinition("com.hamon.crywolf.consumable.beginning_of_the_legend2", UnityEngine.Purchasing.ProductType.Consumable),
        // new ProductDefinition("com.hamon.crywolf.consumable.beginners_spirit", UnityEngine.Purchasing.ProductType.Consumable),
        // new ProductDefinition("com.hamon.crywolf.consumable.beginners_luck", UnityEngine.Purchasing.ProductType.Consumable),
        // new ProductDefinition("com.hamon.crywolf.consumable.beginners_resolve", UnityEngine.Purchasing.ProductType.Consumable),
        // new ProductDefinition("com.hamon.crywolf.consumable.beginners_ambition", UnityEngine.Purchasing.ProductType.Consumable),
        // new ProductDefinition("com.hamon.crywolf.consumable.rainbow_egg_package", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_pile", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_fistful", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_pouch", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_basket", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_chest", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_vault", UnityEngine.Purchasing.ProductType.Consumable),
        // new ProductDefinition("com.hamon.crywolf.consumable.wooden_chest", UnityEngine.Purchasing.ProductType.Consumable),
        // new ProductDefinition("com.hamon.crywolf.consumable.golden_chest", UnityEngine.Purchasing.ProductType.Consumable),
        // new ProductDefinition("com.hamon.crywolf.consumable.jeweled_chest", UnityEngine.Purchasing.ProductType.Consumable),
    };
    
    public event Action OnCashPaymentSuccess;
    public event Action OnPaymentSuccess;
    public event Action<int> OnDailyPaymentSuccess;
    
    [Inject]
    public PaymentService(IWebService webService, ITokenService tokenService)
    {
        _webService = webService;
        _tokenService = tokenService;
    }
    
    public async void Init()
    {
        try
        {
            await UnityServices.InitializeAsync();
            InitPurchasing();
        }
        catch (Exception e)
        {
            Debug.LogError($"PaymentService Init Error: {e}");
        }
    }
    
    private void InitPurchasing()
    {
        if (_storeController != null) return;
        
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProducts(_products);
        UnityPurchasing.Initialize(this, builder);
    }

    public void BuyCashProduct(string productCode)
    {
        _storeController?.InitiatePurchase(productCode);
    }

    public async Task BuyProductAsync(string productCode)
    {
        var packet = new VirtualPaymentPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            ProductCode = productCode
        };
        
        await _webService.SendWebRequestAsync<VirtualPaymentPacketResponse>(
            "Payment/Purchase", UnityWebRequest.kHttpVerbPUT, packet);
        
        var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        var titleKey = "notify_payment_success_title";
        var messageKey = "notify_payment_success_message";
        Managers.Localization.UpdateNotifyPopupText(popup, titleKey, messageKey);
        OnPaymentSuccess?.Invoke();
    }

    public async Task BuyDailyProductAsync(string productCode)
    {
        var packet = new DailyPaymentPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            ProductCode = productCode
        };

        var task = await _webService.SendWebRequestAsync<DailyPaymentPacketResponse>(
            "Payment/PurchaseDaily", UnityWebRequest.kHttpVerbPUT, packet);

        var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        var titleKey = "notify_payment_success_title";
        var messageKey = "notify_payment_success_message";
        Managers.Localization.UpdateNotifyPopupText(popup, titleKey, messageKey);

        if (task.PaymentOk)
        {
            OnDailyPaymentSuccess?.Invoke(task.Slot);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAP Initialized");
        _storeController = controller;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"IAP Initialization Failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"IAP Initialization Failed: {error}, {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        var product = purchaseEvent.purchasedProduct;
        var receipt = product.receipt;
        SendReceiptToServer(receipt, product);
    
        return PurchaseProcessingResult.Pending;
    }

    private async void SendReceiptToServer(string receipt, Product purchaseProduct)
    {
        try
        {
            var packet = new CashPaymentPacketRequired
            {
                AccessToken = _tokenService.GetAccessToken(),
                Receipt = receipt,
                ProductCode = purchaseProduct.definition.id,
            };
            var response = await _webService
                .SendWebRequestAsync<CashPaymentPacketResponse>(
                    "Payment/PurchaseSpinel", UnityWebRequest.kHttpVerbPUT, packet);

            if (response.PaymentOk)
            {
                _storeController.ConfirmPendingPurchase(purchaseProduct);
                var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                const string titleKey = "notify_payment_success_title";
                const string messageKey = "notify_payment_success_message";
                Managers.Localization.UpdateNotifyPopupText(popup, titleKey, messageKey);
                OnCashPaymentSuccess?.Invoke();
            }
            else
            { 
                var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                const string titleKey = "notify_payment_failed_title";
                const string messageKey = "notify_payment_failed_message";
                Managers.Localization.UpdateNotifyPopupText(popup, titleKey, messageKey);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed: {product.definition.id}, Reason: {failureReason}");
    }
    
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogError($"Purchase failed: {product.definition.id}, Reason: {failureDescription.reason}, Message: {failureDescription.message}");
    }
}
