using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Services.LevelPlay;
using Unity.VisualScripting;
using LevelPlayConfiguration = Unity.Services.LevelPlay.LevelPlayConfiguration;

#if UNITY_IOS
using UnityEngine.iOS;
using Unity.Advertisement.IosSupport;
#endif

public class AdsManager
{
#if UNITY_ANDROID
    private const string AndroidAppKey = "21ca06945";
#elif UNITY_IOS
    private const string IOSAppKey = "21ca02fbd";
#endif

    private string _idfa = string.Empty;
    private bool _levelPlayInitialized;
    private bool _attRequested;

    public event Func<DailyProductInfo, Task> OnRewardedRevealDailyProduct;
    public event Func<Task> OnRewardedRefreshDailyProducts;
    public event Action OnAdFailed;

    public DailyProductInfo RevealedDailyProduct { get; set; }
    
#if UNITY_IOS
    public async Task RequestAttAsync()
    {
        if (_attRequested)
        {
            Debug.LogWarning("[Ads] ATT request already made.");
            return;
        }
        _attRequested = true;

        try
        {
            // Return if ATT status is already determined
            var current = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            if (current != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                if (Managers.Policy.GetAttConsent() == null)
                {
                    var result = current == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
                    Managers.Policy.SetAttConsent(result);
                    return;
                }

                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                void Request()
                {
                    ATTrackingStatusBinding.RequestAuthorizationTracking(result =>
                    {
                        var resultStatus =
                            result == (int)ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
                        Managers.Policy.SetAttConsent(resultStatus);
                        tcs.TrySetResult(true);
                    });
                }

                Request();

                // Time out
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var completed = await Task
                    .WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token)) == tcs.Task;

                if (!completed)
                {
                    var fallback = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                    var result = fallback == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
                    Managers.Policy.SetAttConsent(result);
                }
            }
        }
        finally
        {
            _attRequested = false;
        }
    }

    public void FetchIdfa()
    {
        var adIdentifier = Device.advertisingIdentifier;
        _idfa = string.IsNullOrEmpty(adIdentifier) ? User.Instance.UserInfo.UserAccount : adIdentifier;
        Debug.Log($"[Ads] IDFA = {_idfa}");
    }
    
// #elif UNITY_ANDROID && !UNITY_EDITOR
#elif UNITY_ANDROID
    public Task RequestAttAsync() => Task.CompletedTask;
    public void FetchIdfa() 
    {
        var gaid = IronSource.Agent.getAdvertiserId();
        if (string.IsNullOrEmpty(gaid))
        {
            Debug.Log($"[Ads] gaid is null or empty");
            gaid = User.Instance.UserInfo.UserAccount;
        }
        _idfa = gaid;
        Debug.Log($"[Ads] IDFA = {_idfa}");
    }
#endif

    private void ApplyRegulationFlags()
    {
        // // GDPR
        // IronSource.Agent.setConsent();
        // // CCPA
        // IronSource.Agent.setMetaData("do_not_sell", consent.CcpaOptout ? "true" : "false");
        // // LGPD
        // IronSource.Agent.setMetaData("lgpdConsent", consent.GdprConsent ? "true" : "false");
        
        // COPPA
        var coppaConsent = Managers.Policy.GetCoppaConsent();
        if (coppaConsent != null)
        {
            var coppa = coppaConsent.Value;
            IronSource.Agent.setMetaData("is_child_directed", coppa ? "true": "false");
        }
    }
    
    public void InitLevelPlay()
    {
        if (_levelPlayInitialized) return;
        
        ApplyRegulationFlags();
        var userIdfa = string.IsNullOrEmpty(_idfa) ? User.Instance.UserInfo.UserAccount : _idfa;
        var adFormats = new[] { com.unity3d.mediation.LevelPlayAdFormat.REWARDED };
        string appKey =
#if UNITY_ANDROID
            AndroidAppKey;
#else
            IOSAppKey;
#endif
        
        // AdsTest
        IronSource.Agent.setConsent(true);
        IronSource.Agent.setMetaData("is_test_suite", "enable");
        // AdsTest
        
        SetUpEvents();
        
        LevelPlay.Init(appKey, userIdfa, adFormats);
    }

    private void SetUpEvents()
    {
        TearDownEvents();
        IronSourceEvents.onSdkInitializationCompletedEvent += OnLevelPlaySdkReady;
        
        LevelPlay.OnInitSuccess += OnLevelPlayInitialized;
        
        IronSourceRewardedVideoEvents.onAdReadyEvent += OnRewardVideoReady;
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
    }

    public void ShowRewardVideo(string placementName)
    {
        Debug.Log("[Ads] ShowRewardVideo");

        var placement = IronSource.Agent.getPlacementInfo(placementName);
        if (placement != null)
        {
            if (IronSource.Agent.isRewardedVideoAvailable())
            {
                Debug.Log($"[Ads] Placement: {placementName}");
                IronSource.Agent.showRewardedVideo(placementName);
            }
            else
            {
                Debug.Log("[Ads] Reward video ad is not available.");
            }
        }
    }
    
    private void OnLevelPlaySdkReady()
    {
        Debug.Log("SDK Initialization Completed.");
        // AdsTest
        IronSource.Agent.launchTestSuite();
        // AdsTest
    }

    private void OnLevelPlayInitialized(LevelPlayConfiguration configuration)
    {
        Debug.Log("LevelPlay Initialization Completed.");
        _levelPlayInitialized = true;
    }
    
    private void OnRewardVideoReady(IronSourceAdInfo info)
    {
        
    }
    
    
    /// <summary>
    /// The Rewarded Video ad view has opened. Your activity will loose focus.
    /// </summary>
    private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo info)
    {
        
    }
    
    /// <summary>
    /// The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
    /// </summary>
    private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo info)
    {
        
    }
    
    /// <summary>
    /// Indicates that there’s an available ad.
    /// The adInfo object includes information about the ad that was loaded successfully.
    /// This replaces the RewardedVideoAvailabilityChangedEvent(true) event
    /// </summary>
    private void RewardedVideoOnAdAvailable(IronSourceAdInfo info)
    {
        
    }
    
    /// <summary>
    /// Indicates that no ads are available to be displayed.
    /// This replaces the RewardedVideoAvailabilityChangedEvent(false) event
    /// </summary>
    private void RewardedVideoOnAdUnavailable()
    {
        
    }
    
    /// <summary>
    /// The rewarded video ad was failed to show.
    /// </summary>
    private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo info)
    {
        Debug.Log($"[Ads] Reward video ad show failed: {error}");
        OnAdFailed?.Invoke();
    }
    
    /// <summary>
    /// The user completed to watch the video, and should be rewarded.
    /// The placement parameter will include the reward data.
    /// When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
    /// </summary>
    private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo info)
    {
        Debug.Log($"[Ads] Reward video ad rewarded: {placement}");

        switch (placement.ToString())
        {
            case "Check_Daily_Product":
                OnRewardedRevealDailyProduct?.Invoke(RevealedDailyProduct);
                break;
            case "Refresh_Daily_Products":
                OnRewardedRefreshDailyProducts?.Invoke();
                break;
            default:
                break;
        }
    }
    
    /// <summary>
    /// Invoked when the video ad was clicked.
    /// This callback is not supported by all networks, and we recommend using it only if
    /// it’s supported by all networks you included in your build.
    /// </summary>
    private void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo info)
    {
        Debug.Log($"[Ads] Reward video ad clicked: {placement}");
    }

    public void TearDownEvents()
    {
        IronSourceEvents.onSdkInitializationCompletedEvent -= OnLevelPlaySdkReady;

        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;

        IronSourceRewardedVideoEvents.onAdReadyEvent -= OnRewardVideoReady;
        IronSourceRewardedVideoEvents.onAdOpenedEvent -= RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent -= RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdAvailableEvent -= RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent -= RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent -= RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent -= RewardedVideoOnAdClickedEvent;
    }
}
