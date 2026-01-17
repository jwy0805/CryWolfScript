using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
///     <em>에디터 이용 시에만 PlayerPrefs를 이용한다</em>
/// </summary>
/// <remarks><see cref="KeychainService" />, <see cref="EncryptedSharedPreferences" /></remarks>
public class SecretService : ISecretService
{
    private ISecretService _secretService;
    private bool _initTried;
    private readonly object _lock = new object();
    
    private ISecretService Service
    {
        get
        {
#if UNITY_EDITOR
            return null;
#else
            if (_secretService != null) return _secretService;

            lock (_lock)
            {
                if (_secretService != null) return _secretService;
                if (_initTried) return null;

                _initTried = true;
                _secretService = CreatePlatformSecretService();
                return _secretService;
            }
#endif
        }
    }    
    
    private ISecretService CreatePlatformSecretService()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return new KeychainService();

#elif UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            return AndroidEncryptedPrefsSecretService.TryCreate();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SecretService(Android) init failed: {e}");
            return null;
        }

#else
        return null;
#endif
    }
        
    public bool Put(string key, string value)
    {
#if UNITY_EDITOR
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
        return true;
#else
        var secretService = Service;
        if (secretService == null)
        {
            Debug.LogWarning("SecretService not initialized.");
            return false;
        }
        return secretService.Put(key, value);
#endif
    }

    public string Get(string key)
    {
#if UNITY_EDITOR
        return PlayerPrefs.GetString(key);
#else
        var secretService = Service;
        if (secretService == null)
        {
            Debug.LogWarning("SecretService not initialized.");
            return null;
        }
        return secretService.Get(key);
#endif
    }

    public bool Delete(string key)
    {
#if UNITY_EDITOR
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        return true;
#else
        var secretService = Service;
        if (secretService == null)
        {
            Debug.LogWarning("SecretService not initialized.");
            return false;
        }
        return secretService.Delete(key);
#endif
    }
}