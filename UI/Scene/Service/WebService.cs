using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
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
    }
    
    public void SendWebRequest(string url, string method, object obj)
    {
        var sendUrl = $"{Managers.Network.BaseUrl}/api/{url}";
        string jsonPayload = obj != null ? JsonConvert.SerializeObject(obj) : "";
    
        try
        {
            using var client = new HttpClient();
            // 종료 시 지연을 방지하기 위해 타임아웃을 짧게 설정 (1~2초)
            client.Timeout = TimeSpan.FromSeconds(2);
            
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var accessToken = _tokenService.GetAccessToken();
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }

            // 메서드에 따른 처리
            if (method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
                _ = client.PutAsync(sendUrl, content).Result;
            else if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                _ = client.PostAsync(sendUrl, content).Result;
            else if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                _ = client.GetAsync(sendUrl).Result;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[WebService] Synchronous request failed: {e.Message}");
        }
    }
    
    public async Task<T> SendWebRequestAsync<T>(string url, string method, object obj)
    {
        return await SendWebRequestInternalAsync<T>(url, method, obj, allowRetry: true);
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
    
    public async Task<T> GetAsync<T>(string path, Dictionary<string, string> query = null, Dictionary<string, string> headers = null)
    {
        return await GetInternalAsync<T>(path, query, headers, true);      
    }

    private async Task<T> GetInternalAsync<T>(
        string path,
        Dictionary<string, string> query,
        Dictionary<string, string> headers,
        bool allowRetry)
    {
        var baseUrl = $"{Managers.Network.BaseUrl}/api/{path}";
        var sendUrl = BuildUrl(baseUrl, query);
        
        using var uwr = new UnityWebRequest(sendUrl, UnityWebRequest.kHttpVerbGET);
        uwr.downloadHandler = new DownloadHandlerBuffer();

        if (headers != null)
        {
            foreach (var pair in headers)
            {
                if (string.IsNullOrWhiteSpace(pair.Key)) continue;
                uwr.SetRequestHeader(pair.Key, pair.Value ?? string.Empty);
            }
        }
        
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
        
        if (uwr.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogWarning($"[WebService] network/data error while sending GET request: {uwr.error} : {uwr.downloadHandler.text}, URL: {sendUrl}");
            return default;
        }

        if (uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning($"[WebService] GET HTTP error: {uwr.responseCode} {uwr.error} : {uwr.downloadHandler.text}, URL: {sendUrl}");

            if (uwr.responseCode == 401 && allowRetry && _tokenService != null)
            {
                var refreshToken = _tokenService.GetRefreshToken();
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var refreshed = await TryRefreshTokenAsync(refreshToken);
                    if (refreshed)
                    {
                        return await GetInternalAsync<T>(path, query, headers, false);
                    }
                }
                
                _tokenService.ClearTokens();
            }
            
            return default;
        }
        
        var text = uwr.downloadHandler.text;
        if (string.IsNullOrWhiteSpace(text)) return default;
        
        try
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
        catch (Exception e)
        {
            Debug.LogError($"[WebService] GET JSON deserialize error: {e}\n{text}");
            return default;
        }
    }

    private string BuildUrl(string baseUrl, Dictionary<string, string> query)
    {
        if (query == null || query.Count == 0) return baseUrl;
        
        var sb = new StringBuilder();
        sb.Append(baseUrl);
        sb.Append(baseUrl.Contains("?") ? "&" : "?");
        
        bool first = true;
        foreach (var pair in query)
        {
            if (string.IsNullOrWhiteSpace(pair.Key)) continue;
            if (!first) sb.Append("&");
            first = false;
            
            var key = UnityWebRequest.EscapeURL(pair.Key);
            var value = UnityWebRequest.EscapeURL(pair.Value ?? string.Empty);
            sb.Append(key).Append("=").Append(value);
        }

        var url = sb.ToString();
        if (url.EndsWith("?") || url.EndsWith("&"))
        {
            url = url[..^1];
        }

        return url;
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
