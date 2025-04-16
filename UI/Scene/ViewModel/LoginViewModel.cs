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


/* Last Modified : 25. 03. 26
 * Version : 1.013
 */

public class LoginViewModel : IDisposable
{
    // External service interfaces (Dependency Injection)
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    
    private GoogleAuth _googleAuth;
    private string _googleIdToken;
    
    private IAppleAuthManager _appleAuthManager;
    
    // Apple login properties
    public string AppleToken { get; set; }
    public string AppleError { get; set; }

    public event Action OnDirectLoginFailed;
    public event Action OnResetGoogleButton;
    public event Action OnResetAppleButton;
    
    public string UserAccount
    {
        get => User.Instance.UserAccount;
        set => User.Instance.UserAccount = value;
    }
    public string Password { get; set; }
    public bool RememberMe { get; set; }
    public bool ReadPolicy { get; set; }
    public bool ReadTerms { get; set; }
    
    [Inject]
    public LoginViewModel(IUserService userService, IWebService webService, ITokenService tokenService)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
        
        InitAuth();
        
        #if UNITY_IOS && !UNITY_EDITOR
        InitAppleAuth();    
        #endif
    }

    #region Initialization
    
    private void InitAuth()
    {
        _googleAuth = new GoogleAuth();
        _googleAuth.TryResume(OnGoogleSignIn, OnGetGoogleTokenResponse);
        
        Debug.Log("Google Auth Initialized");
    }

    private void InitAppleAuth()
    {
        var deserializer = new PayloadDeserializer();
        _appleAuthManager = new AppleAuthManager(deserializer);
        
        Debug.Log("Apple Auth Initialized");
    }
    
    #endregion
    
    #region Direct Login
    
    // This method is called when the login button is clicked, not using SOCIAL LOGIN such as google, facebook, apple
    public void TryDirectLogin()
    {
        var packet = new LoginUserAccountPacketRequired { UserAccount = UserAccount, Password = Password };
        _ = _webService.SendWebRequest<LoginUserAccountPacketResponse>(
            "UserAccount/Login", UnityWebRequest.kHttpVerbPOST, packet, response =>
            {
                if (response.LoginOk)
                {
                    // Login Success
                    HandleLoginSuccess(response.AccessToken, response.RefreshToken);
                }
                else
                {
                    // Login FailedGoogleService-Info
                    OnDirectLoginFailed?.Invoke();
                    // TODO: Show error notice popup
                }
            });
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
            AppleToken = idToken;

            _ = HandleAppleSignInSuccessAsync(AppleToken);
        }
        else
        {
            Debug.LogError("Sign-in with Apple error. Credential is null or not IAppleIDCredential.");
            AppleError = "Retrieving Apple ID Token failed.";
        }
    }
    
    private void OnAppleSignInError(IAppleError error)
    {
        Debug.Log($"Sign-in with Apple error. Message: {error}");
        AppleError = "Retrieving Apple Id Token failed.";
    }
    
    private async Task HandleAppleSignInSuccessAsync(string idToken)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithAppleAsync(idToken);
            Debug.Log("SignIn is successful.");
            
            // Handle login success
            var appleLoginPacket = new LoginApplePacketRequired { IdToken = idToken };
            var task = _webService.SendWebRequestAsync<LoginApplePacketResponse>(
                "UserAccount/LoginApple", "PUT", appleLoginPacket);
            await task;
            
            var response = task.Result;
            if (response != null)
            {
                HandleLoginSuccess(response.AccessToken, response.RefreshToken);
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
        
        OnResetAppleButton?.Invoke();
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
        
        var jwt = new JWT(tokenResponse.IdToken);
        _googleIdToken = tokenResponse.IdToken;
        Debug.Log($"JSON Web Token (JWT) Payload: {jwt.Payload}");
        jwt.ValidateSignature(_googleAuth.ClientId, OnValidateGoogleSignature);
    }

    private void OnValidateGoogleSignature(bool success, string error)
    {
        if (success == false)
        {
            Debug.LogError($"Google token response failed: {error}");
            return;
        }
        
        _ = HandleGoogleSignInSuccessAsync(_googleIdToken);
        
        OnResetGoogleButton?.Invoke();
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
            HandleLoginSuccess(response.AccessToken, response.RefreshToken);
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
    
    private void HandleLoginSuccess(string accessToken, string refreshToken)
    {
        _tokenService.SaveAccessToken(accessToken);
        _tokenService.SaveRefreshToken(refreshToken);
        
        Managers.Scene.LoadScene(Define.Scene.MainLobby);
    }
    
    public void ForgotPassword()
    {
        
    }

    public void HandleAppleSignInSuccess()
    {
        // 서버 연동, 계정 처리 로직
    }
    
    public void HandleAppleSignInFailed()
    {
        
    }
    
    public void Dispose()
    {
        _googleIdToken = null;
    }
}
