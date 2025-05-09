using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_SignInPopup : UI_Popup
{
    private IWebService _webService;
    private LoginViewModel _loginViewModel;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private enum Buttons
    {
        ExitButton,
        LoginButton,
        SignUpButton,
        ForgotPasswordButton,
    }

    private enum Texts
    {
        SignInLoginTitleText,
        SignInRememberMeText,
        SignInEmailText,
        SignInPasswordText,
        SignInLoginButtonText,
        SignInSignUpButtonText,
        SignInForgotPasswordButtonText,
    }
    
    private enum TextInputs
    {
        EmailInput,
        PasswordInput,
    }

    private enum Toggles
    {
        RememberToggle,
    }
    
    [Inject]
    public void Construct(IWebService webService, LoginViewModel loginViewModel)
    {
        _webService = webService;
        _loginViewModel = loginViewModel;
        
        _loginViewModel.OnDirectLoginFailed += ClearPasswordText;
        _loginViewModel.OnDirectLoginFailed += ShowLoginErrorMessage;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
    }
    
    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<TMP_InputField>(typeof(TextInputs));
        Bind<Toggle>(typeof(Toggles));
        
        Managers.Localization.UpdateTextAndFont(_textDict);
        Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.EmailInput));
        Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.PasswordInput));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.LoginButton).gameObject.BindEvent(OnLoginClicked);
        GetButton((int)Buttons.SignUpButton).gameObject.BindEvent(SignUpClicked);
        GetButton((int)Buttons.ForgotPasswordButton).gameObject.BindEvent(OnForgetPasswordClicked);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
        
        var rememberToggle = GetToggle((int)Toggles.RememberToggle);
        rememberToggle.isOn = _loginViewModel.RememberMe;
        rememberToggle.onValueChanged.AddListener(value => _loginViewModel.RememberMe = value);
    }
    
    private void OnLoginClicked(PointerEventData data)
    {
        _loginViewModel.UserAccount = GetTextInput((int)TextInputs.EmailInput).text;
        _loginViewModel.Password = GetTextInput((int)TextInputs.PasswordInput).text;
        _loginViewModel.TryDirectLogin();
    }   
    
    private void SignUpClicked(PointerEventData data)
    {
        _loginViewModel.SignUp();
    }
    
    private void OnForgetPasswordClicked(PointerEventData data)
    {
        _loginViewModel.ForgotPassword();
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }

    private void ShowLoginErrorMessage()
    {
        // Managers.UI.ShowPopupUI<>();
    }
    
    private void ClearPasswordText()
    {
        GetTextInput((int)TextInputs.PasswordInput).text = "";
    }

    private void OnDestroy()
    {
        _loginViewModel.OnDirectLoginFailed -= ClearPasswordText;
        _loginViewModel.OnDirectLoginFailed -= ShowLoginErrorMessage;
    }
}
