using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Core;
using UnityEngine.Networking;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Rendering;
using Zenject;

/* Last Modified : 25. 04. 22
 * Version : 1.02
 */

public class PaymentService : IPaymentService, IDetailedStoreListener
{
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    private IStoreController _storeController;
    private IExtensionProvider _extensionProvider;

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
    public event Func<int, Task> OnDailyPaymentSuccess;
    
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
        
        var task = await _webService.SendWebRequestAsync<VirtualPaymentPacketResponse>(
            "Payment/Purchase", UnityWebRequest.kHttpVerbPUT, packet);

        if (task.PaymentOk)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            var titleKey = "notify_payment_success_title";
            var messageKey = "notify_payment_success_message";
            await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
            OnPaymentSuccess?.Invoke();
        }
        else
        {
            var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            var titleKey = "notify_payment_failed_title";
            var messageKey = "notify_payment_failed_message";
            await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
        }
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

        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        var titleKey = "notify_payment_success_title";
        var messageKey = "notify_payment_success_message";
        await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);

        if (task.PaymentOk)
        {
            OnDailyPaymentSuccess?.Invoke(task.Slot);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAP Initialized");
        _storeController = controller;
        _extensionProvider = extensions;
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
        _ = VerifyAndConfirmAsync(purchaseEvent.purchasedProduct);
        return PurchaseProcessingResult.Pending;
    }

    private async Task VerifyAndConfirmAsync(Product product)
    {
        try
        {
            var ok = await SendReceiptToServer(product.receipt, product);
            _storeController.ConfirmPendingPurchase(product);
            if (ok)
            {
                await ShowPurchaseSuccessPopup();
                OnCashPaymentSuccess?.Invoke();
            }
            else
            {
                await ShowPurchaseFailedPopup();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            _storeController.ConfirmPendingPurchase(product);
            await ShowPurchaseFailedPopup();
        }
    }
    
    private async Task<bool> SendReceiptToServer(string receipt, Product purchaseProduct)
    {
        var packet = new CashPaymentPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Receipt = receipt,
            ProductCode = purchaseProduct.definition.id,
        };
        var response = await _webService.SendWebRequestAsync<CashPaymentPacketResponse>(
            "Payment/PurchaseSpinel", UnityWebRequest.kHttpVerbPUT, packet);
        
        return response is { PaymentOk: true };
    }

    public void RestorePurchases()
    { 
#if UNITY_IOS
        if (_storeController == null)
        {
            Debug.LogError("StoreController is null");
            return;
        }

        var apple = _extensionProvider.GetExtension<IAppleExtensions>();
        apple.RestoreTransactions(result =>
        {
            if (result)
            {
                Debug.Log("Restore transactions completed successfully");
            }
            else
            {
                Debug.LogError("Restore transactions failed");
            }
        });
#endif
    }
    
    private async Task ShowPurchaseSuccessPopup()
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        const string titleKey = "notify_payment_success_title";
        const string messageKey = "notify_payment_success_message";
        await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
    }
    
    private async Task ShowPurchaseFailedPopup()
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        const string titleKey = "notify_payment_failed_title";
        const string messageKey = "notify_payment_failed_message";
        await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
    }
    
    public async void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        try
        {
            Debug.LogWarning($"Purchase failed: {product.definition.id}, Reason: {failureReason}");
            await ShowPurchaseFailedPopup();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    public async void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        try
        {
            Debug.LogError($"Purchase failed: {product.definition.id}, Reason: {failureDescription.reason}, Message: {failureDescription.message}");
            await ShowPurchaseFailedPopup();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
}
