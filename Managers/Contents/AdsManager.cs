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
    
    private readonly SemaphoreSlim _attGate = new(1,1);
    private string _advertisingUserId = string.Empty;
    private bool _levelPlayInitialized;
    private bool _userRequestedShow;
    private string _lastRequestedPlacement = string.Empty;

    private enum NotifyState { None, Loading, Unavailable }
    private NotifyState _notifyState = NotifyState.None;
    private UI_NotifyPopup _activeNotifyPopup;
    
    public event Func<DailyProductInfo, Task> OnRewardedRevealDailyProduct;
    public event Func<Task> OnRewardedRefreshDailyProducts;

    public DailyProductInfo RevealedDailyProduct { get; set; }
    
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticFlagsOnDomainReload()
    {
        _sInitEventsWired = false;
    }
#endif
    
#if UNITY_IOS
    public async Task RequestAttAsync(int timeoutSeconds = 10)
    {
        Debug.Log("RequestAttAsync called");
        await _attGate.WaitAsync();
        try
        {
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            if (status != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                SaveAttStatus(status);
                return;
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            ATTrackingStatusBinding.RequestAuthorizationTracking(_ => { tcs.TrySetResult(true); });
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            try
            {
                await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            SaveAttStatus(status);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Ads] ATT request failed: {e.Message}");
            var fallback = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            SaveAttStatus(fallback);
        }
        finally
        {
            _attGate.Release();
        }
    }

    private void SaveAttStatus(ATTrackingStatusBinding.AuthorizationTrackingStatus status)
    {
        var authorized = status == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
        Managers.Policy.SetAttConsent(authorized);
        
        Debug.Log($"[AdsManager] ATT status = {status}, authorized = {authorized}");
    }

    public void FetchAdvertisingUserId()
    {
        var idfa = Device.advertisingIdentifier;
        _advertisingUserId = string.IsNullOrEmpty(idfa) ? User.Instance.UserInfo.UserAccount : idfa;
        if (string.IsNullOrEmpty(_advertisingUserId)) _advertisingUserId = SystemInfo.deviceUniqueIdentifier;
    }
    
#else
    public Task RequestAttAsync(int timeoutSeconds = 10) => Task.CompletedTask;

    public void FetchAdvertisingUserId()
    {
        var userId = User.Instance.UserInfo.UserAccount;
        _advertisingUserId = string.IsNullOrEmpty(userId) ? SystemInfo.deviceUniqueIdentifier : userId;
    }
#endif

    private void ApplyRegulationFlags()
    {
        var under13 = Managers.Policy.IsUnder13;
        LevelPlay.SetMetaData("is_child_directed", under13 ? "true" : "false");
    }

    public void InitLevelPlay()
    {
        if (_levelPlayInitialized) return;
        
        ApplyRegulationFlags();
        var userIdfa = string.IsNullOrEmpty(_advertisingUserId)
            ? User.Instance.UserInfo.UserAccount
            : _advertisingUserId;
        if (string.IsNullOrEmpty(userIdfa)) userIdfa = SystemInfo.deviceUniqueIdentifier;
        
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
#else
        "";
#endif
        
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        LevelPlay.SetMetaData("is_test_suite", "enable");
#endif        

        WireInitEventsOnce();
        
        LevelPlay.Init(appKey, userIdfa);
    }
    
    private static void WireInitEventsOnce()
    {
        if (_sInitEventsWired) return;

        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed  -= OnLevelPlayInitFailed;
        LevelPlay.OnInitSuccess += OnLevelPlayInitialized;
        LevelPlay.OnInitFailed  += OnLevelPlayInitFailed;

        _sInitEventsWired = true;
    }
    
    
    private static void TearDownInitEvents()
    {
        if (!_sInitEventsWired) return;

        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed  -= OnLevelPlayInitFailed;

        _sInitEventsWired = false;
    }

    private static void OnLevelPlayInitialized(LevelPlayConfiguration configuration)
    {
        Managers.Ads.LevelPlayInitialized(configuration);
    }

    private void LevelPlayInitialized(LevelPlayConfiguration configuration)
    {
        Debug.Log("LevelPlay Initialization Completed.");
        _levelPlayInitialized = true;
        
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        LevelPlay.LaunchTestSuite();
#endif
        
        CreateAndWireRewarded();
        _rewardedAd.LoadAd();
    }
    
    private static void OnLevelPlayInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"[Ads] LevelPlay Initialization Failed: {error}");
    }

    private void CreateAndWireRewarded()
    {
        if (_rewardedAd != null) return;
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
                Debug.Log("[Ads] Editor simulated rewarded ad for Check_Daily_Product.");
                _ = InvokeSafeAsync(OnRewardedRevealDailyProduct, RevealedDailyProduct);
                break;
            case "Refresh_Daily_Products":
                Debug.Log("[Ads] Editor simulated rewarded ad for Refresh_Daily_Products.");
                _ = InvokeSafeAsync(OnRewardedRefreshDailyProducts);
                break;
            default:
                Debug.Log("[Ads] Editor simulated rewarded ad with no specific placement.");
                break;
        }

        return;
#endif
        _userRequestedShow = true;
        _lastRequestedPlacement = placementName ?? string.Empty;
        
        if (_rewardedAd == null)
        {
            Debug.LogWarning("[Ads] Rewarded ad is not initialized.");
            _userRequestedShow = false;
            _ = ShowAdUnavailableAsync();
            return;
        }

        bool capped = !string.IsNullOrEmpty(placementName) && LevelPlayRewardedAd.IsPlacementCapped(placementName);
        if (capped)
        {
            _userRequestedShow = false;
            _ = ShowAdUnavailableAsync();
            return;
        }

        if (_rewardedAd.IsAdReady())
        {
            CloseNotifyIfAny();
            ShowRewardInternal(placementName);
            return;
        }

        // 아직 준비 안 됨 -> 로드 + 로딩 팝업
        _rewardedAd.LoadAd();
        _ = ShowAdLoadingNotifyAsync();
    }

    private void ShowRewardInternal(string placementName)
    {
        if (string.IsNullOrEmpty(placementName))
            _rewardedAd.ShowAd();
        else
            _rewardedAd.ShowAd(placementName);
    }
    
    private void OnRewardedLoaded(LevelPlayAdInfo adInfo)
    {
        if (!_userRequestedShow) return;

        var placement = _lastRequestedPlacement ?? string.Empty;
        var capped = !string.IsNullOrEmpty(placement) && LevelPlayRewardedAd.IsPlacementCapped(placement);

        if (capped)
        {
            _userRequestedShow = false;
            CloseNotifyIfAny();
            _ = ShowAdUnavailableAsync();
            return;
        }

        if (_rewardedAd != null && _rewardedAd.IsAdReady())
        {
            _userRequestedShow = false;
            CloseNotifyIfAny();
            ShowRewardInternal(placement);
        }
    }

    private void OnRewardedLoadFailed(LevelPlayAdError adError)
    {
        Debug.LogError($"[Ads] Rewarded ad failed to load: {adError}");
        
        if (_userRequestedShow)
        {
            _userRequestedShow = false;
            CloseNotifyIfAny();
            _ = ShowAdUnavailableAsync();
        }

        Managers.Instance.StartCoroutine(RetryLoadAdCoroutine(5f));
    }

    private IEnumerator RetryLoadAdCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        _rewardedAd?.LoadAd();
    }

    private void OnRewardedDisplayed(LevelPlayAdInfo adInfo)
    {
        _userRequestedShow = false;
        CloseNotifyIfAny();
    }
    
    private void OnRewardedDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError adError)
    {
        Debug.LogError($"[Ads] Rewarded ad failed to display: {adError}");

        _rewardedAd?.LoadAd();

        if (_userRequestedShow)
        {
            _userRequestedShow = false;
            CloseNotifyIfAny();
            _ = ShowAdUnavailableAsync();
        }
    }

    private void OnRewardedClosed(LevelPlayAdInfo adInfo)
    {
        // 광고가 닫힌 후 다음 노출 대비 로드
        _rewardedAd?.LoadAd();
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
                _ = InvokeSafeAsync(OnRewardedRevealDailyProduct, RevealedDailyProduct);
                break;
            case "Refresh_Daily_Products":
                _ = InvokeSafeAsync(OnRewardedRefreshDailyProducts);
                break;
        }
    }
    
    private static async Task InvokeSafeAsync(Func<DailyProductInfo, Task> evt, DailyProductInfo arg)
    {
        if (evt == null) return;
        try { await evt.Invoke(arg); }
        catch (Exception e) { Debug.LogException(e); }
    }

    private static async Task InvokeSafeAsync(Func<Task> evt)
    {
        if (evt == null) return;
        try { await evt.Invoke(); }
        catch (Exception e) { Debug.LogException(e); }
    }
    
    private Task ShowAdLoadingNotifyAsync() => ShowNotifyAsync(NotifyState.Loading, "notify_ads_loading");
    private Task ShowAdUnavailableAsync() => ShowNotifyAsync(NotifyState.Unavailable, "notify_ads_unavailable");
    
    private async Task ShowNotifyAsync(NotifyState state, string messageKey)
    {
        if (_notifyState == state && _activeNotifyPopup != null) return;

        // 상태 전환 시 기존 팝업 닫기
        CloseNotifyIfAny();

        _notifyState = state;

        try
        {
            var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            _activeNotifyPopup = popup;

            popup.SetYesCallback(CloseNotifyIfAny);
            popup.SetExitCallback(CloseNotifyIfAny);

            await Managers.Localization.UpdateNotifyPopupText(popup, messageKey);
        }
        catch
        {
            // 실패 시 상태만 원복
            _notifyState = NotifyState.None;
            _activeNotifyPopup = null;
            throw;
        }
    }

    private void CloseNotifyIfAny()
    {
        if (_activeNotifyPopup != null)
        {
            Managers.UI.ClosePopupUI(_activeNotifyPopup);
            _activeNotifyPopup = null;
        }

        _notifyState = NotifyState.None;    
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
        
        CloseNotifyIfAny();
        _levelPlayInitialized = false;
    }
}
