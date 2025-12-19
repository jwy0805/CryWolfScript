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

    private const string PendingKey = "IAP_Pending_Receipt";
    
    private readonly HashSet<ProductDefinition> _products = new()
    {
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_pile", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_fistful", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_pouch", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_basket", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_chest", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_vault", UnityEngine.Purchasing.ProductType.Consumable),
    };
    
    private Dictionary<string, Product> _storeProducts = new();

    public event Func<Task> OnIapReady;
    public event Func<Task> OnCashPaymentSuccess;
    public event Func<Task> OnPaymentSuccess;
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
            InitPurchasing();
            await InitUgs();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    private void InitPurchasing()
    {
        if (_storeController != null) return;
        
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProducts(_products);
        UnityPurchasing.Initialize(this, builder);
    }

    private async Task InitUgs()
    {
        await UnityServices.InitializeAsync();
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
            if (task.PaymentCode == VirtualPaymentCode.Product)
            {
                var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                popup.SetYesCallback(Managers.UI.CloseAllPopupUI);
                var titleKey = "notify_payment_success_title";
                var messageKey = "notify_payment_success_message";
                await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
                await Util.InvokeAll(OnPaymentSuccess);
            }
            else if (task.PaymentCode == VirtualPaymentCode.Subscription)
            {
                var popup = await Managers.UI.ShowPopupUI<UI_RewardSubscriptionPopup>();
                popup.ProductCode = productCode;
            }
        }
        else
        {
            var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            popup.SetYesCallback(Managers.UI.CloseAllPopupUI);
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
        popup.SetYesCallback(Managers.UI.CloseAllPopupUI);
        
        if (task.PaymentOk)
        {
            var titleKey = "notify_payment_success_title";
            var messageKey = "notify_payment_success_message";
            await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
            await Util.InvokeAll(OnDailyPaymentSuccess, task.Slot);
        }
        else
        {
            var titleKey = "notify_payment_failed_title";
            var messageKey = "notify_payment_failed_message";
            await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAP Initialized");
        _storeController = controller;
        _extensionProvider = extensions;

        _storeProducts.Clear();
        foreach (var product in controller.products.all)
        {
            _storeProducts[product.definition.id] = product;
            Debug.Log($"IAP Product Available: {product.definition.id}, Type: {product.definition.type}, Price: {product.metadata.localizedPriceString}");
        }

        _ = Util.InvokeAll(OnIapReady);
        _ = RetryPendingIfAnyAsync();
    }
    
    public string GetLocalizedPrice(string productCode)
    {
        if (_storeProducts.TryGetValue(productCode, out var product) &&
            product != null && product.availableToPurchase)
        {
            var s = product.metadata?.localizedPriceString;
            if (!string.IsNullOrEmpty(s)) return s;
        }
        return "—";
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
        Debug.Log($"[IAP] ProcessPurchase called. Product={product.definition.id}, TxId={product.transactionID}");
        _ = VerifyAndConfirmAsync(product);
        return PurchaseProcessingResult.Pending;
    }

    private async Task VerifyAndConfirmAsync(Product product)
    {
        Debug.Log($"[IAP] VerifyAndConfirmAsync start. Product={product.definition.id}, TxId={product.transactionID}");

        CashPaymentPacketResponse response = null;
        var reachedServer = false;

        try
        {
            response = await SendReceiptToServer(product.receipt, product);
            reachedServer = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"VerifyAndConfirmAsync error: {e}");
            reachedServer = false;
        }

        var shouldConfirm = false;
        var grantReward = false;
        
        if (!reachedServer)
        {
            // 서버/네트워크에 못 닿음 -> 확인 지연 -> pending 유지
            await ShowPurchaseVerifyingPopup();
            EnqueuePendingRetry(product);
            return;
        }

        if (response.PaymentOk 
            && response.ErrorCode is CashPaymentErrorCode.None or CashPaymentErrorCode.AlreadyProcessed)
        {
            // 정상 처리 or 이미 처리된 영수증
            shouldConfirm = true;
            grantReward = true;
        }
        else if (response.ErrorCode is CashPaymentErrorCode.InvalidReceipt or CashPaymentErrorCode.Unauthorized)
        {
            // 권한 문제 -> 재시도해도 의미x -> Confirm 하고 종료
            shouldConfirm = true;
            grantReward = false;
            await ShowPurchaseFailedPopup();
        }
        else
        {
            // 그 외 서버 내부 오류 등 -> pending 유지
            shouldConfirm = false;
            grantReward = false;
            await ShowPurchaseVerifyingPopup();
            EnqueuePendingRetry(product);
        }

        if (grantReward)
        {
            await ShowPurchaseSuccessPopup();
            await Util.InvokeAll(OnCashPaymentSuccess);
        }
        else
        {
            await ShowPurchaseFailedPopup(response.ErrorCode);
        }

        if (shouldConfirm && _storeController != null)
        {
            _storeController.ConfirmPendingPurchase(product);
        }
    }

    private void EnqueuePendingRetry(Product product)
    {
        var item = $"{product.transactionID}|||{product.definition.id}|||{product.receipt}";
        PlayerPrefs.SetString(PendingKey, item);
        PlayerPrefs.Save();
    }

    public async Task RetryPendingIfAnyAsync()
    {
        var s = PlayerPrefs.GetString(PendingKey, "");
        if (string.IsNullOrEmpty(s)) return;
        
        var parts = s.Split(new[] {"|||"},
            StringSplitOptions.None);
        if (parts.Length < 3) return;

        var txId = parts[0];
        var productId = parts[1];
        var receipt = parts[2];
        
        
    }
    
    private Task<CashPaymentPacketResponse> SendReceiptToServer(string receipt, Product purchaseProduct)
    {
        var packet = new CashPaymentPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Receipt = receipt,
            ProductCode = purchaseProduct.definition.id,
        };
        
        Debug.Log($"[IAP] Sending receipt to server. Product={packet.ProductCode}");

        return _webService.SendWebRequestAsync<CashPaymentPacketResponse>(
            "Payment/PurchaseSpinel", UnityWebRequest.kHttpVerbPUT, packet);
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
    
    private async Task ShowPurchaseFailedPopup(CashPaymentErrorCode errorCode = CashPaymentErrorCode.None)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        const string titleKey = "notify_payment_failed_title";
        var messageKey = errorCode switch
        {
            CashPaymentErrorCode.InvalidReceipt => "notify_payment_failed_invalid_receipt",
            CashPaymentErrorCode.Unauthorized => "notify_payment_failed_unauthorized",
            CashPaymentErrorCode.AlreadyProcessed => "notify_payment_failed_already_processed",
            CashPaymentErrorCode.InternalError => "notify_payment_failed_internal_error",
            _ => "notify_payment_failed_message"
        };
        
        await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
    }

    private async Task ShowPurchaseVerifyingPopup()
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        const string titleKey = "notify_payment_verifying_title";
        const string messageKey = "notify_payment_verifying_message";
        
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
