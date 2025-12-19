using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.VisualScripting;
using Unity.Services.LevelPlay;
using UnityEditor;

#if UNITY_IOS
using UnityEngine.iOS;
using Unity.Advertisement.IosSupport;
#endif

public class AdsManager
{
    private static bool _sInitEventsWired;
    
    private const string AndroidAppKey = "21ca06945";
    private const string IOSAppKey = "21ca02fbd";
    private const string RewardedAdUnitId = "venr949joqp63z47";

    private LevelPlayRewardedAd _rewardedAd;
    
    private string _idfa = string.Empty;
    private bool _levelPlayInitialized;
    private bool _attRequested;

    public event Func<DailyProductInfo, Task> OnRewardedRevealDailyProduct;
    public event Func<Task> OnRewardedRefreshDailyProducts;
    public event Action OnAdFailed;

    public DailyProductInfo RevealedDailyProduct { get; set; }
    
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticFlagsOnDomainReload()
    {
        _sInitEventsWired = false;
    }
#endif

#if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // 편집→플레이 전환/종료 시 안전하게 해제
        if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.ExitingEditMode)
            TearDownInitEvents();
    }
#endif
    
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
        Debug.Log($"[AdsManager] IDFA = {_idfa}");
    }
    
#else
    public Task RequestAttAsync() => Task.CompletedTask;
    public void FetchIdfa() 
    {
        var userId = User.Instance.UserInfo.UserAccount;
        _idfa = string.IsNullOrEmpty(userId) ? SystemInfo.deviceUniqueIdentifier : userId;
        Debug.Log($"[AdsManager] IDFA = {_idfa}");
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
            LevelPlay.SetMetaData("is_child_directed", coppa ? "true": "false");
        }
    }

    public void InitLevelPlay()
    {
        if (_levelPlayInitialized) return;
        
        ApplyRegulationFlags();
        var userIdfa = string.IsNullOrEmpty(_idfa) ? User.Instance.UserInfo.UserAccount : _idfa;
        string appKey =
#if UNITY_ANDROID
            AndroidAppKey;
#elif UNITY_IOS
            IOSAppKey;
#elif UNITY_EDITOR
            Application.platform switch
            {
                RuntimePlatform.WindowsEditor => AndroidAppKey,
                RuntimePlatform.OSXEditor => IOSAppKey,
                _ => throw new NotSupportedException("Unsupported platform for AdsManager in Editor.")
            };
#endif
        
        // AdsTest
        LevelPlay.SetMetaData("is_test_suite", "enable");
        // AdsTest

        WireInitEventsOnce();
        
        LevelPlay.Init(appKey, userIdfa);
    }
    
    private void WireInitEventsOnce()
    {
        if (_sInitEventsWired) return;

        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed  -= OnLevelPlayInitFailed;
        LevelPlay.OnInitSuccess += OnLevelPlayInitialized;
        LevelPlay.OnInitFailed  += OnLevelPlayInitFailed;

        _sInitEventsWired = true;
    }
    
    
    private void TearDownInitEvents()
    {
        if (!_sInitEventsWired) return;

        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed  -= OnLevelPlayInitFailed;

        _sInitEventsWired = false;
    }

    private void OnLevelPlayInitialized(LevelPlayConfiguration configuration)
    {
        Debug.Log("LevelPlay Initialization Completed.");
        _levelPlayInitialized = true;
        
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        LevelPlay.LaunchTestSuite();
        Debug.Log("[Ads] LevelPlay Test Suite Launched.");
        return;
#endif
        CreateAndWireRewarded();
        _rewardedAd.LoadAd();
    }
    
    private void OnLevelPlayInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"[Ads] LevelPlay Initialization Failed: {error}");
        OnAdFailed?.Invoke();
    }

    private void CreateAndWireRewarded()
    {
        if (string.IsNullOrEmpty(RewardedAdUnitId) || RewardedAdUnitId.Contains("PUT_"))
        {
            Debug.LogError("[Ads] RewardedAdUnitId is missing.");
            return;
        }

        _rewardedAd = new LevelPlayRewardedAd(RewardedAdUnitId);
        _rewardedAd.OnAdLoaded += OnRewardedLoaded;
        _rewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
        _rewardedAd.OnAdDisplayed += OnRewardedDisplayed;
        _rewardedAd.OnAdDisplayFailed += OnRewardedDisplayFailed;
        _rewardedAd.OnAdRewarded += OnRewardedRewarded;
        _rewardedAd.OnAdClosed += OnRewardedClosed;
        _rewardedAd.OnAdClicked += OnRewardedClicked;
        _rewardedAd.OnAdInfoChanged += OnRewardedInfoChanged;
    }
    
    public void ShowRewardVideo(string placementName)
    {
#if UNITY_EDITOR
        // Editor에서는 바로 보상 지급
        switch (placementName)
        {
            case "Check_Daily_Product":
                Debug.Log("[Ads] Editor simulated rewarded ad for Check_Daily_Product.");;
                OnRewardedRevealDailyProduct?.Invoke(RevealedDailyProduct);
                break;
            case "Refresh_Daily_Products":
                Debug.Log("[Ads] Editor simulated rewarded ad for Refresh_Daily_Products.");
                OnRewardedRefreshDailyProducts?.Invoke();
                break;
            default:
                Debug.Log("[Ads] Editor simulated rewarded ad with no specific placement.");
                break;
        }
#endif
        
        if (_rewardedAd == null)
        {
            Debug.LogWarning("[Ads] Rewarded ad is not initialized.");
            return;
        }

        if (_rewardedAd.IsAdReady() &&
            (string.IsNullOrEmpty(placementName) || !LevelPlayRewardedAd.IsPlacementCapped(placementName)))
        {
            if (string.IsNullOrEmpty(placementName))
            {
                _rewardedAd.ShowAd();
            }
            else
            {
                _rewardedAd.ShowAd(placementName);
            }
        }
        else
        {
            Debug.Log("[Ads] Reward video ad is not ready, loading ad...");
            _rewardedAd.LoadAd();
        }
    }

    private void OnRewardedLoaded(LevelPlayAdInfo adInfo)
    {
        
    }

    private void OnRewardedLoadFailed(LevelPlayAdError adError)
    {
        Debug.LogError($"[Ads] Rewarded ad failed to load: {adError}");
        Managers.Instance.StartCoroutine(RetryLoadAdCoroutine(5f));
    }

    private IEnumerator RetryLoadAdCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        _rewardedAd?.LoadAd();
    }

    private void OnRewardedDisplayed(LevelPlayAdInfo adInfo)
    {
        
    }
    
    private void OnRewardedDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError adError)
    {
        Debug.LogError($"[Ads] Rewarded ad failed to display: {adError}");
        OnAdFailed?.Invoke();
        _rewardedAd.LoadAd();
    }

    private void OnRewardedClosed(LevelPlayAdInfo adInfo)
    {
        // 광고가 닫힌 후 다음 노출 대비 로드
        _rewardedAd.LoadAd();
    }

    private void OnRewardedClicked(LevelPlayAdInfo adInfo)
    {
        
    }

    private void OnRewardedInfoChanged(LevelPlayAdInfo adInfo)
    {
        
    }

    private void OnRewardedRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log($"[Ads] Rewarded: placement={adInfo.PlacementName}, reward={reward?.Name}:{reward?.Amount}");

        switch (adInfo.PlacementName)
        {
            case "Check_Daily_Product":
                OnRewardedRevealDailyProduct?.Invoke(RevealedDailyProduct);
                break;
            case "Refresh_Daily_Products":
                OnRewardedRefreshDailyProducts?.Invoke();
                break;
        }
    }
    
    public void TearDownEvents()
    {
        TearDownInitEvents();

        if (_rewardedAd != null)
        {
            _rewardedAd.OnAdLoaded -= OnRewardedLoaded;
            _rewardedAd.OnAdLoadFailed -= OnRewardedLoadFailed;
            _rewardedAd.OnAdDisplayed -= OnRewardedDisplayed;
            _rewardedAd.OnAdDisplayFailed -= OnRewardedDisplayFailed;
            _rewardedAd.OnAdRewarded -= OnRewardedRewarded;
            _rewardedAd.OnAdClosed -= OnRewardedClosed;
            _rewardedAd.OnAdClicked -= OnRewardedClicked;
            _rewardedAd.OnAdInfoChanged -= OnRewardedInfoChanged;
        }
    }
}
