using UnityEngine;

#if UNITY_ANDROID && !UNITY_EDITOR
/// <summary>
///     사용되는 네이티브 코드는 <c>Assets/Plugins/Android/SecretManager.java</c>에 기재됨
/// </summary>
/// <remarks>
///     <a href="https://developer.android.com/reference/androidx/security/crypto/EncryptedSharedPreferences">EncryptedSharedPreferences</a>
/// </remarks>
internal sealed class AndroidEncryptedPrefsSecretService : ISecretService
{
    private readonly AndroidJavaObject _secretManager;

    private AndroidEncryptedPrefsSecretService(AndroidJavaObject secretManager)
    {
        _secretManager = secretManager;
    }

    public static ISecretService TryCreate()
    {
        // currentActivity는 null일 수 있으니 여기서 방어
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        if (activity == null)
        {
            Debug.LogError("UnityPlayer.currentActivity is null (too early?).");
            return null;
        }

        var context = activity.Call<AndroidJavaObject>("getApplicationContext");
        if (context == null)
        {
            Debug.LogError("ApplicationContext is null.");
            return null;
        }

        // 네 Java 클래스 패키지/클래스명이 정확히 일치해야 함
        var secretManager = new AndroidJavaObject("com.nikaera.SecretManager", context);
        return new AndroidEncryptedPrefsSecretService(secretManager);
    }

    public bool Put(string key, string value) => _secretManager.Call<bool>("put", key, value);
    public string Get(string key) => _secretManager.Call<string>("get", key);
    public bool Delete(string key) => _secretManager.Call<bool>("delete", key);
}
#endif