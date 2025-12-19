using System;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public class PolicyConsentManager
{
    private TaskCompletionSource<bool> _tcs;
    private const string PolicyKey = "PolicyConsent";
    private const string CoppaKey = "CoppaConsent";
    private const string CcpaKey = "CcpaConsent";
    private const string AttKey = "AttConsent";
    
    public bool ReadPolicy { get; set; }
    public bool ReadTerms { get; set; }
    public bool AgeUnder13 { get; set; }
    
    public bool CheckPolicyConsent()
    {
        var regionCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
        bool policyChecked = false;

        if (regionCode == "US")
        {
            if (GetCoppaConsent() != null && GetPolicyConsent() != null)
            {
                policyChecked = true;
            }
        }
        else
        {
            if (GetPolicyConsent() != null)
            {
                policyChecked = true;
            }
        }

        return policyChecked;
    }
    
    public bool CheckAttConsent()
    {
#if UNITY_IOS
        return GetAttConsent() != null;
#endif
        return true;
    }

    public async Task RequestConsents(bool policyFinished, bool attFinished)
    {
        if (policyFinished == false)
        {
            await ShowPolicyPopupAsync();
        }
        Debug.Log("[Manager] Requesting consents...");

#if UNITY_IOS
        if (attFinished == false)
        {
            await Managers.Ads.RequestAttAsync();
        }
        Debug.Log("[Manager] Requesting att...");
#endif
    }

    private async Task<bool> ShowPolicyPopupAsync()
    {
        _tcs = new TaskCompletionSource<bool>();
        var popup = await Managers.UI.ShowPopupUI<UI_PolicyPopup>();
        
        popup.SetYesCallback(() =>
        {
            SetPolicyConsent(true);
            _tcs.SetResult(true);
        });
        
        return await _tcs.Task;
    }
    
    public void SetPolicyConsent(bool consent)
    {
        PlayerPrefs.SetInt(PolicyKey, consent ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void SetCoppaConsent(bool consent)
    {
        PlayerPrefs.SetInt(CoppaKey, consent ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void SetCcpaConsent(bool consent)
    {
        PlayerPrefs.SetInt(CcpaKey, consent ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void SetAttConsent(bool consent)
    {
        PlayerPrefs.SetInt(AttKey, consent ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public bool? GetPolicyConsent()
    {
        if (PlayerPrefs.HasKey(PolicyKey))
            return PlayerPrefs.GetInt(PolicyKey) == 1;
        return null;
    }
    
    public bool? GetCoppaConsent()
    {
        if (PlayerPrefs.HasKey(CoppaKey))
            return PlayerPrefs.GetInt(CoppaKey) == 1;
        return null;
    }
    
    public bool? GetCcpaConsent()
    {
        if (PlayerPrefs.HasKey(CcpaKey))
            return PlayerPrefs.GetInt(CcpaKey) == 1;
        return null;
    }
    
    public bool? GetAttConsent()
    {
        if (PlayerPrefs.HasKey(AttKey))
            return PlayerPrefs.GetInt(AttKey) == 1;
        return null;
    }
}
