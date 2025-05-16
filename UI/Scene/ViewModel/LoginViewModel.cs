using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

#if UNITY_IOS && !UNITY_EDITOR
#endif
using Unity.Services.Authentication;
using Unity.Services.Core;
using Debug = UnityEngine.Debug;
using Assets.SimpleSignIn.Google.Scripts;

#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.


/* Last Modified : 25. 05. 08
 * Version : 1.021
 */

public class LoginViewModel : IInitializable, IDisposable
{
    // External service interfaces (Dependency Injection)
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    
    private GoogleAuth _googleAuth;
    private string _googleIdToken;
    
    private IAppleAuthManager _appleAuthManager;
    
    // Apple login properties
    private string _appleToken;

    public event Action OnDirectLoginFailed;
    public event Action OnRestoreButton;
    
    public bool ProcessingLogin { get; set; }
    
    public string UserAccount
    {
        get => User.Instance.UserAccount;
        set => User.Instance.UserAccount = value;
    }
    public string Password { get; set; }
    public bool RememberMe { get; set; }
    
    [Inject]
    public LoginViewModel(IUserService userService, IWebService webService, ITokenService tokenService)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
        
        Initialize();
    }

    #region Initialization
    
    public async void Initialize()          // ← async void 유지
    {
        try
        {
            // 1) Google Auth (동기)
            InitGoogleAuth();

#if UNITY_IOS && !UNITY_EDITOR
            // 2) UGS Core (비동기)  ─────────────────────────────
            await InitUgs().ConfigureAwait(false);

            // 3) Apple Auth (동기) ─────────────────────────────
            InitAppleAuth();                
#endif
        }
        catch (Exception ex)
        {
            // 모든 예외를 한 곳에서 처리해 로그 유실 방지
            Debug.LogException(ex);
        }
    }
    
    private void InitGoogleAuth()
    {
        _googleAuth = new GoogleAuth();
        _googleAuth.TryResume(OnGoogleSignIn, OnGetGoogleTokenResponse);
    }

    private void InitAppleAuth()
    {
        var deserializer = new PayloadDeserializer();
        _appleAuthManager = new AppleAuthManager(deserializer);
        
        Debug.Log("Apple Auth Initialized");
    }
    
    private async Task InitUgs()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Gaming Services Initialized");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unity Gaming Service Initializing Failed: {e}");
        }
    }    
    
    #endregion
    
    #region Direct Login
    
    // This method is called when the login button is clicked, not using SOCIAL LOGIN such as google, facebook, apple
    public async void TryDirectLogin()
    {
        try
        {
            var packet = new LoginUserAccountPacketRequired { UserAccount = UserAccount, Password = Password };
            var task = _webService.SendWebRequestAsync<LoginUserAccountPacketResponse>(
                "UserAccount/Login", UnityWebRequest.kHttpVerbPOST, packet);
            
            await task;
            
            var response = task.Result;
            if (response == null)
            {
                Debug.LogError("Direct login response is null");
                OnDirectLoginFailed?.Invoke();
                return;
            }
            
            HandleLoginSuccess(response.AccessToken, response.RefreshToken);
        }
        catch (Exception e)
        {
            Debug.LogError($"Direct login error: {e}");
        }
    }
    
    #endregion

    #region Guest Login

    public async void TryGuestLogin()
    {
        try
        {
            var guestId = SystemInfo.deviceUniqueIdentifier;
            var packet = new LoginGuestPacketRequired { GuestId = guestId };
            var task = _webService.SendWebRequestAsync<LoginGuestPacketResponse>(
                "UserAccount/LoginGuest", UnityWebRequest.kHttpVerbPOST, packet);
            
            await task;
            
            var response = task.Result;
            if (response.LoginOk)
            {
                HandleSignInSuccess(response.AccessToken, response.RefreshToken);
            }
            else
            {
                Debug.LogError("Guest login failed");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Guest login error: {e}");
        }
    }

    #endregion

    #region Social Login: Apple

    public void UpdateAppleAuthManager()
    {
        _appleAuthManager?.Update();
    }

    public void RequestAppleLogin()
    {
        if (_appleAuthManager == null)
        {
            InitAppleAuth();
            if (_appleAuthManager == null)
            {
                Debug.LogError("Apple Auth Manager is null");
                return;
            }
        }
        
        // Set the login arguments
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
        _appleAuthManager.LoginWithAppleId(loginArgs, OnAppleSignIn, OnAppleSignInError);
    }

    private void OnAppleSignIn(ICredential credential)
    {
        if (credential is IAppleIDCredential appleIdCredential)
        {
            var token = appleIdCredential.IdentityToken;
            var tokenLength = token.Length;
            var idToken = Encoding.UTF8.GetString(token, 0, tokenLength);
            
            Debug.Log($"Sign-in with Apple successfully done. IDToken: {idToken}");
            _appleToken = idToken;
            ProcessingLogin = false;

            _ = HandleAppleSignInSuccessAsync(_appleToken);
        }
        else
        {
            Debug.LogError("Sign-in with Apple error. Credential is null or not IAppleIDCredential.");
        }
        
        OnRestoreButton?.Invoke();
    }
    
    private void OnAppleSignInError(IAppleError error)
    {
        Debug.Log($"Sign-in with Apple error. Message: {error}");
        OnRestoreButton?.Invoke();
    }
    
    private async Task HandleAppleSignInSuccessAsync(string idToken)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithAppleAsync(idToken);
            
            // Handle login success
            var appleLoginPacket = new LoginApplePacketRequired { IdToken = idToken };
            var task = _webService.SendWebRequestAsync<LoginApplePacketResponse>(
                "UserAccount/LoginApple", "PUT", appleLoginPacket);
            await task;
            
            var response = task.Result;
            if (response != null)
            {
                HandleSignInSuccess(response.AccessToken, response.RefreshToken);
            }
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("Apple SignIn AuthenticationException: " + e);
        }
        catch (RequestFailedException e)
        {
            Debug.LogError("Apple SignIn RequestFailedException: " + e);
        }
    }

    private async Task UnlinkAppleAsync()
    {
        try
        {
            await AuthenticationService.Instance.UnlinkAppleAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    #endregion
    
    #region Social Login: Google
    
    public void RequestGoogleLogin()
    {
        _googleAuth.SignIn(OnGoogleSignIn);
    }

    private void OnGoogleSignIn(bool success, string error, GoogleUserInfo userInfo)
    {
        if (success == false)
        {
            Debug.LogError($"Google sign in failed: {error}");
            return;
        }
        
        _googleAuth.GetTokenResponse(OnGetGoogleTokenResponse);
    }

    private void OnGetGoogleTokenResponse(bool success, string error, TokenResponse tokenResponse)
    {
        if (success == false || tokenResponse == null)
        {
            Debug.LogError($"Failed to get token response: {error}");
            return;
        }
        
        ProcessingLogin = false;
        
        var jwt = new JWT(tokenResponse.IdToken);
        _googleIdToken = tokenResponse.IdToken;
        Debug.Log($"JSON Web Token (JWT) Payload: {jwt.Payload}");
        jwt.ValidateSignature(_googleAuth.ClientId, OnValidateGoogleSignature);
    }

    private void OnValidateGoogleSignature(bool success, string error)
    {
        OnRestoreButton?.Invoke();

        if (success == false)
        {
            Debug.LogError($"Google token response failed: {error}");
            return;
        }
        
        _ = HandleGoogleSignInSuccessAsync(_googleIdToken);
    }
    
    private async Task HandleGoogleSignInSuccessAsync(string idToken)
    {
        var googleLoginPacket = new LoginGooglePacketRequired { IdToken = idToken };
        var task = _webService.SendWebRequestAsync<LoginGooglePacketResponse>(
            "UserAccount/LoginGoogle", "PUT", googleLoginPacket);
        await task;
            
        var response = task.Result;
        if (response != null)
        {
            HandleSignInSuccess(response.AccessToken, response.RefreshToken);
        }
    }
    
    private void OnGoogleSignOut()
    {
        _googleAuth.SignOut(true);
    }
    
    #endregion

    public void SignIn()
    {
        Managers.UI.ShowPopupUI<UI_SignInPopup>();
    }
    
    public void SignUp()
    {
        Managers.UI.ShowPopupUI<UI_SignUpPopup>();
    }

    private void HandleSignInSuccess(string accessToken, string refreshToken)
    {
        SetInitialSettings();
        HandleLoginSuccess(accessToken, refreshToken);
    }
    
    private void SetInitialSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", 0.5f);
        PlayerPrefs.SetFloat("SfxVolume", 0.5f);
        Managers.Localization.SetLanguage(Application.systemLanguage.ToString());
        PlayerPrefs.SetInt("Notification", 1);
        
        PlayerPrefs.Save();
    }
    
    private void HandleLoginSuccess(string accessToken, string refreshToken)
    {
        _tokenService.SaveAccessToken(accessToken);
        _tokenService.SaveRefreshToken(refreshToken);
        
        Managers.Scene.LoadScene(Define.Scene.MainLobby);
    }
    
    public void ForgotPassword()
    {
        // TODO: Send password reset email
    }
    
    public void Dispose()
    {
        _googleIdToken = null;
    }
}
