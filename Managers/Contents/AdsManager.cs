using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.unity3d.mediation;
using UnityEngine;
using UnityEngine.iOS;
using System.Runtime.InteropServices;
using Unity.VisualScripting;

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
    private bool _rewardVideoReady;
    private bool _loading;

    public event Action OnRewarded;
    public event Action OnAdFailed;
    
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

    public void ApplyRegulationFlags()
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
        var adFormats = new[] { LevelPlayAdFormat.REWARDED };
        string appKey =
#if UNITY_ANDROID
            AndroidAppKey;
#else
            IOSAppKey;
#endif
        
        IronSource.Agent.setConsent(true);
        IronSource.Agent.setMetaData("is_test_suite", "enable");
        SetUpEvents();
        
        LevelPlay.Init(appKey, userIdfa, adFormats);
    }

    private void SetUpEvents()
    {
        IronSourceEvents.onSdkInitializationCompletedEvent -= OnLevelPlayReady;
        IronSourceEvents.onSdkInitializationCompletedEvent += OnLevelPlayReady;
        IronSourceRewardedVideoEvents.onAdReadyEvent -= OnAdReadyEvent;
        IronSourceRewardedVideoEvents.onAdReadyEvent += OnAdReadyEvent;
    }
    
    private void OnLevelPlayReady()
    {
        _levelPlayInitialized = true;
        Debug.Log($"SDK Initialization Completed.");
        IronSource.Agent.launchTestSuite();
    }
    
    private void OnAdReadyEvent(IronSourceAdInfo info)
    {
        _rewardVideoReady = true;
        _loading = false;
    }
}
