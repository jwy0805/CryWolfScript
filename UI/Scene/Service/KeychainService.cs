using System.Runtime.InteropServices;

/// <summary>
///     구현은 <c>Assets/Plugins/iOS/KeychainService.mm</c>에 기재됨
/// </summary>
/// <remarks>
///     <a href="https://developer.apple.com/documentation/security/keychain_services">Keychain Services</a>
/// </remarks>
public class KeychainService: ISecretService
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern int addItem(string dataType, string value);
    [DllImport("__Internal")] private static extern string getItem(string dataType);
    [DllImport("__Internal")] private static extern int deleteItem(string dataType);
#endif
    
    // KeychainService.mm에 정의된 함수를 호출
    #region ISecretService
    
    public bool Put(string key, string value)
    {
#if UNITY_IOS && !UNITY_EDITOR
        return addItem(key, value) == 0;
#else
        return false;
#endif
    }

    public string Get(string key)
    {
#if UNITY_IOS && !UNITY_EDITOR
        return getItem(key);
#else
        return null;
#endif
    }

    public bool Delete(string key)
    {
#if UNITY_IOS && !UNITY_EDITOR
        return deleteItem(key) == 0;
#else
        return false;
#endif
    }
    
    #endregion
}