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
    private bool _levelPlayInitialized = false;
    
#if UNITY_IOS
    public Task RequestAttAsync()
    {
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() !=
            ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource<bool>();
        CoroutineRunner.instance.StartCoroutine(WaitForAtt());

        return tcs.Task;

        IEnumerator WaitForAtt()
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
            while (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
                   ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                yield return null;
            tcs.TrySetResult(true);
        }
    }

    public void FetchIdfa()
    {
        _idfa = Device.advertisingIdentifier;
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
    
    public void InitLevelPlay()
    {
        var userIdfa = string.IsNullOrEmpty(_idfa) ? User.Instance.UserAccount : _idfa;
        var adFormats = new[] { LevelPlayAdFormat.REWARDED };
        string appKey =
#if UNITY_ANDROID
            AndroidAppKey;
#else
            IOSAppKey;
#endif
        
        IronSource.Agent.setConsent(true);
        IronSourceEvents.onSdkInitializationCompletedEvent -= OnLevelPlayReady;
        IronSourceEvents.onSdkInitializationCompletedEvent += OnLevelPlayReady;
        IronSource.Agent.setMetaData("is_test_suite", "enable");
        
        LevelPlay.Init(appKey, userIdfa, adFormats);
    }
    
    private void OnLevelPlayReady()
    {
        _levelPlayInitialized = true;
        Debug.Log($"SDK Initialization Completed.");
        IronSource.Agent.launchTestSuite();
    }

    public bool TryShowRewarded()
    {
        if (_levelPlayInitialized == false) return false;

        return true;
    }
}
