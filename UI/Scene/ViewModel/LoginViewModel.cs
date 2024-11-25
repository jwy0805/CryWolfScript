using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

/* Last Modified : 24. 10. 04
 * Version : 1.012
 */

public class LoginViewModel
{
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    
    public event Action OnLoginFailed;

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
    }
    
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
                    _tokenService.SaveAccessToken(response.AccessToken);
                    _tokenService.SaveRefreshToken(response.RefreshToken);
                    
                    Managers.Scene.LoadScene(Define.Scene.MainLobby);
                }
                else
                {
                    // Login Failed
                    OnLoginFailed?.Invoke();
                    // TODO: Show error notice popup
                }
            });
    }

    public void SignIn()
    {
        Managers.UI.ShowPopupUI<UI_SignInPopup>();
    }
    
    public void SignUp()
    {
        Managers.UI.ShowPopupUI<UI_SignUpPopup>();
    }

    public void ForgotPassword()
    {
        
    }
}
