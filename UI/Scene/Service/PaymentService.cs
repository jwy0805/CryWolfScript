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

public class PaymentService : IPaymentService, IDetailedStoreListener
{
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    private IStoreController _storeController;

    private readonly HashSet<ProductDefinition> _products = new()
    {
        new ProductDefinition("com.hamon.crywolf.consumable.abundant_harvest", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.beginning_of_the_legend1", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.beginning_of_the_legend2", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.beginners_spirit", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.beginners_luck", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.beginners_resolve", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.beginners_ambition", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.rainbow_egg_package", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_pile", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_fistful", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_pouch", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_basket", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_chest", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.spinel_vault", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.wooden_chest", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.golden_chest", UnityEngine.Purchasing.ProductType.Consumable),
        new ProductDefinition("com.hamon.crywolf.consumable.jeweled_chest", UnityEngine.Purchasing.ProductType.Consumable),
    };
    
    public event Action OnCashPaymentSuccess;
    public event Action OnPaymentSuccess;
    
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

    public async void BuyProduct(string productCode)
    {
        var packet = new VirtualPaymentPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            ProductCode = productCode
        };
        
        await _webService.SendWebRequestAsync<VirtualPaymentPacketResponse>(
            "Payment/Purchase", UnityWebRequest.kHttpVerbPOST, packet);
        
        var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        popup.Text = "Payment Success!";
        OnPaymentSuccess?.Invoke();
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

    private async void SendReceiptToServer(string receipt, UnityEngine.Purchasing.Product purchaseProduct)
    {
        var packet = new CashPaymentPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Receipt = receipt,
            ProductCode = purchaseProduct.definition.id,
        };
        var response = await _webService
            .SendWebRequestAsync<CashPaymentPacketResponse>("Payment/PurchaseSpinel", "POST", packet);

        if (response.PaymentOk)
        {
            _storeController.ConfirmPendingPurchase(purchaseProduct);
            var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            popup.Text = "Payment Success!";
            OnCashPaymentSuccess?.Invoke();
        }
        else
        { 
            var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
            popup.SetWarning("Failed to purchase item.");
        }
    }

    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed: {product.definition.id}, Reason: {failureReason}");
    }
    
    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogError($"Purchase failed: {product.definition.id}, Reason: {failureDescription.reason}, Message: {failureDescription.message}");
    }
}
