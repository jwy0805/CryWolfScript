using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenService : ITokenService
{
    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";
        
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
        PlayerPrefs.SetString(RefreshTokenKey, refreshToken);
        PlayerPrefs.Save();
    }
        
    public string GetRefreshToken()
    {
        return PlayerPrefs.GetString(RefreshTokenKey, string.Empty);
    }
        
    public void ClearTokens()
    {
        PlayerPrefs.DeleteKey(AccessTokenKey);
        PlayerPrefs.DeleteKey(RefreshTokenKey);
        PlayerPrefs.Save();
    }
}