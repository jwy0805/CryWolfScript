using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TokenService : ITokenService
{
    private const string AccessTokenKey = "cry_wolf_access_token";
    private const string RefreshTokenKey = "cry_wolf_refresh_token";
    
    private readonly ISecretService _secretService;
    
    [Inject]
    public TokenService(ISecretService secretService)
    {
        _secretService = secretService;
    }
        
    public void SaveAccessToken(string accessToken)
    {
        PlayerPrefs.SetString(AccessTokenKey, accessToken);
        PlayerPrefs.Save();
    }
        
    public string GetAccessToken()
    {
        return PlayerPrefs.GetString(AccessTokenKey, string.Empty);
    }
        
    public void SaveRefreshToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            // 의도적으로 비우는 경우라면 삭제 처리
            _secretService.Delete(RefreshTokenKey);
            return;
        }

        var ok = _secretService.Put(RefreshTokenKey, refreshToken);    }
        
    public string GetRefreshToken()
    {
        var token = _secretService.Get(RefreshTokenKey);
        return token ?? string.Empty;    }
        
    public void ClearTokens()
    {
        PlayerPrefs.DeleteKey(AccessTokenKey);
        PlayerPrefs.Save();
        
        _secretService.Delete(RefreshTokenKey);
    }
}