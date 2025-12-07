using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

public class WebService : IWebService
{
    private readonly ITokenService _tokenService;

    [Inject]
    public WebService(ITokenService tokenService)
    {
        _tokenService = tokenService;
        Debug.Log($"WebService Created {_tokenService.GetHashCode()}");
    }
    
    public async Task<T> SendWebRequestAsync<T>(string url, string method, object obj)
    {
        return await SendWebRequestInternalAsync<T>(url, method, obj, allowRetry: true);
    }

    public async Task SendWebRequest<T>(string url, string method, object obj, Action<T> responseAction)
    {
        var result = await SendWebRequestInternalAsync<T>(url, method, obj, allowRetry: true);
        responseAction?.Invoke(result);
    }

    private async Task<T> SendWebRequestInternalAsync<T>(string url, string method, object obj, bool allowRetry)
    {
        var sendUrl = $"{Managers.Network.BaseUrl}/api/{url}";
        byte[] jsonBytes = null;

        if (obj != null)
        {
            jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
        }

        using var uwr = new UnityWebRequest(sendUrl, method);
        uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        
        // AccessToken이 있으면 Authorization 헤더 추가
        var accessToken = _tokenService.GetAccessToken();
        if (!string.IsNullOrEmpty(accessToken))
        {
            uwr.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        }
        
        var operation = uwr.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }
        
        // Network, Data 처리 오류
        if (uwr.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogWarning($"[WebService] network/data error while sending request: {uwr.error} : {uwr.downloadHandler.text}, URL: {sendUrl}");
            return default;
        }
        
        // HTTP Error (4xx, 5xx)
        if (uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning($"[WebService] HTTP error while sending request: {uwr.error} : {uwr.downloadHandler.text}, URL: {sendUrl}");
            
            // 401 + 재시도 허용 -> 토큰 갱신 시도
            if (uwr.responseCode == 401 && allowRetry && _tokenService != null)
            {
                var refreshToken = _tokenService.GetRefreshToken();
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var refreshed = await TryRefreshTokenAsync(refreshToken);
                    if (refreshed)
                    {
                        return await SendWebRequestInternalAsync<T>(sendUrl, method, refreshToken, false);
                    }
                }
                
                _tokenService.ClearTokens();
                // 로그인 화면으로 이동
            }
            {
                Debug.LogWarning("[WebService] No retry for HTTP error.");
            }

            return default;
        }
        
        // 정상 응답
        var text = uwr.downloadHandler.text;
        if (string.IsNullOrWhiteSpace(text))
        {
            return default;
        }

        try
        {
            var resObj = JsonConvert.DeserializeObject<T>(text);
            return resObj;
        }
        catch (Exception e)
        {
            Debug.LogError($"[WebService] JSON deserialize error: {e}\n{text}");
            return default;
        }
    }

    private async Task<bool> TryRefreshTokenAsync(string refreshToken)
    {
        var sendUrl = $"{Managers.Network.BaseUrl}/api/UserAccount/RefreshToken";
        var required = new RefreshTokenRequired
        {
            RefreshToken = refreshToken,
        };
        
        var jsonStr = JsonConvert.SerializeObject(required);
        var jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        
        using var uwr = new UnityWebRequest(sendUrl, UnityWebRequest.kHttpVerbPOST);
        uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        var operation = uwr.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (uwr.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.DataProcessingError)
            return false;
        if (uwr.result == UnityWebRequest.Result.ProtocolError) return false;

        try
        {
            var text = uwr.downloadHandler.text;
            var res = JsonConvert.DeserializeObject<RefreshTokenResponse>(text);
            if (res == null || string.IsNullOrEmpty(res.AccessToken)) return false;
            
            _tokenService.SaveAccessToken(res.AccessToken);
            if (!string.IsNullOrEmpty(res.RefreshToken))
            {
                _tokenService.SaveRefreshToken(res.RefreshToken);
            }
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[WebService] RefreshToken JSON error: {e}");
            return false;
        }
    }
}
