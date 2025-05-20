using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.iOS;
using System.Runtime.InteropServices;
using Unity.Services.LevelPlay;
using Unity.VisualScripting;
using LevelPlayConfiguration = Unity.Services.LevelPlay.LevelPlayConfiguration;

#if UNITY_IOS
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

    public event Func<DailyProductInfo, Task> OnRewardedRevealDailyProduct;
    public event Func<Task> OnRewardedRefreshDailyProducts;
    public event Action OnAdFailed;

    public DailyProductInfo RevealedDailyProduct { get; set; }
    
#if UNITY_IOS
    public Task RequestAttAsync()
    {
        // Return if ATT status is already determined
        var current = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        if (current != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            if (Managers.Policy.GetAttConsent() == null)
            {
                Managers.Policy.SetAttConsent(current == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED);
            }
            else
            {
                Managers.Policy.SetAttConsent(current == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED);
            }
            
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource<bool>();
        CoroutineRunner.instance.StartCoroutine(WaitForAtt());
        return tcs.Task;

        IEnumerator WaitForAtt()
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
            // Wait until the user has made a choice
            while (ATTrackingStatusBinding.GetAuthorizationTrackingStatus()
                   == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                yield return null;
            }

            // Save status to PlayerPrefs
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            Managers.Policy.SetAttConsent(status == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED);

            tcs.TrySetResult(true);
        }
    }

    public void FetchIdfa()
    {
        var adIdentifier = Device.advertisingIdentifier;
        _idfa = string.IsNullOrEmpty(adIdentifier) ? User.Instance.UserAccount : adIdentifier;
        Debug.Log($"[Ads] IDFA = {_idfa}");
    }
    
#elif UNITY_ANDROID && !UNITY_EDITOR
    public Task RequestAttAsync() => Task.CompletedTask;
    public void FetchIdfa() 
    {
        var gaid = IronSource.Agent.getAdvertiserId();
        if (string.IsNullOrEmpty(gaid))
        {
            Debug.Log($"[Ads] gaid is null or empty");
            gaid = User.Instance.UserAccount;
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
        var userIdfa = string.IsNullOrEmpty(_idfa) ? User.Instance.UserAccount : _idfa;
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
        IronSourceEvents.onSdkInitializationCompletedEvent -= OnLevelPlaySdkReady;
        IronSourceEvents.onSdkInitializationCompletedEvent += OnLevelPlaySdkReady;
        
        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitSuccess += OnLevelPlayInitialized;
        
        IronSourceRewardedVideoEvents.onAdReadyEvent -= OnRewardVideoReady;
        IronSourceRewardedVideoEvents.onAdReadyEvent += OnRewardVideoReady;
        IronSourceRewardedVideoEvents.onAdOpenedEvent -= RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent -= RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdAvailableEvent -= RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent -= RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent -= RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent -= RewardedVideoOnAdClickedEvent;
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
}
