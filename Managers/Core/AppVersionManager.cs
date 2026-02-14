using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class AppVersionManager
{
    private string _currentVersion 
    #if UNITY_EDITOR
        = "1.0.2";
    #else
        = Application.version;
    #endif

    public async Task<AppCheckResponse> CheckAsync()
    {
#if UNITY_IOS
        var platform = "ios";
#elif UNITY_ANDROID
        var platform = "android";
#else
        var platform = Application.platform == RuntimePlatform.IPhonePlayer ? "ios" : "android";
#endif
        
        var query = new Dictionary<string, string>
        {
            { "platform", platform },
            { "current", _currentVersion },
        };

        var headers = new Dictionary<string, string>
        {
            { "Accept-Language", Managers.Localization.Language2Letter }
        };

        var res = await ServiceResolver.WebService.GetAsync<AppCheckResponse>("App/Check", query, headers);
        return res;
    }
}
