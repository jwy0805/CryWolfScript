using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_IOS && !UNITY_EDITOR
using Unity.Advertisement.IosSupport;
#endif
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable InconsistentNaming

public class PolicyConsentManager
{
    private TaskCompletionSource<bool> _tcs;
    private const string PolicyKey = "PolicyConsent";
    private const string AgeGateKey = "AgeGateConsent";
    private const string Under13Key = "IsUnder13";
    private const string AttKey = "AttConsent";

    private readonly SemaphoreSlim _gate = new(1, 1);

    public bool PolicyConsent => PlayerPrefs.GetInt(PolicyKey, 0) == 1;
    public bool AgeGateDone => PlayerPrefs.GetInt(AgeGateKey, 0) == 1;
    public bool IsUnder13 => PlayerPrefs.GetInt(Under13Key, 0) == 1;
    
    public bool? AttConsent => PlayerPrefs.HasKey(AttKey) ? PlayerPrefs.GetInt(AttKey) == 1 : null;
    
    private bool IsPolicyDone()
    {
        return PolicyConsent && AgeGateDone && PlayerPrefs.HasKey(Under13Key);
    }
    
    public void SetPolicyDone(bool isUnder13)
    {
        PlayerPrefs.SetInt(PolicyKey, 1);
        PlayerPrefs.SetInt(AgeGateKey, 1);
        PlayerPrefs.SetInt(Under13Key, isUnder13 ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void SetAttConsent(bool v)
    {
        PlayerPrefs.SetInt(AttKey, v ? 1 : 0);
        PlayerPrefs.Save();
    }

    public async Task EnsureConsentsAndInitAdsAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (!IsPolicyDone())
            {
                var popup = await Managers.UI.ShowPopupUI<UI_PolicyPopup>();
                var result = await popup.WaitResultAsync();

                SetPolicyDone(result.IsUnder13);
            }
            
#if UNITY_IOS && !UNITY_EDITOR
            var under13 = IsUnder13;

            if (!under13)
            {
                var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                
                if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                {
                    await Managers.Ads.RequestAttAsync(timeoutSeconds: 10);
                }
                else
                {
                    if (AttConsent == null)
                    {
                        var authorized = status == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
                        SetAttConsent(authorized);
                    }
                }
            }
            else
            {
                if (AttConsent == null)
                    SetAttConsent(false);
            }
#endif

            Managers.Ads.FetchAdvertisingUserId();
            Managers.Ads.InitLevelPlay();
        }
        finally
        {
            _gate.Release();
        }
    }
}
