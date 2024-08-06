using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoginViewModel
{
    public string UserAccount { get; set; }
    public string Password { get; set; }
    public Action OnLoginFailed;
    
    public LoginViewModel()
    {
        ServiceLocator.RegisterService(new UserService());
        ServiceLocator.RegisterService(new WebService());
        ServiceLocator.RegisterService(new TokenService());
    }
    
    // This method is called when the login button is clicked, not using SOCIAL LOGIN such as google, facebook, apple
    public void TryDirectLogin()
    {
        var packet = new LoginUserAccountPacketRequired { UserAccount = UserAccount, Password = Password };
        _ = ServiceLocator.GetService<WebService>().SendWebRequest<LoginUserAccountPacketResponse>(
            "UserAccount/Login", UnityWebRequest.kHttpVerbPOST, packet, OnDirectLogin);
    }

    private void OnDirectLogin(LoginUserAccountPacketResponse response)
    {
        // Login Failed
        if (response.LoginOk == false)
        {
            OnLoginFailed?.Invoke();
            // TODO: Show error notice popup
            return;
        }
        
        // Login Success
        Managers.Scene.LoadScene(Define.Scene.MainLobby);
        Managers.Clear();
            
        ServiceLocator.GetService<UserService>().UserAccount = UserAccount;
        ServiceLocator.GetService<TokenService>().SaveAccessToken(response.AccessToken);
        ServiceLocator.GetService<TokenService>().SaveRefreshToken(response.RefreshToken);
    }

    public void SignUp()
    {
        Managers.UI.ShowPopupUI<UI_SignUpPopup>();
    }
}
