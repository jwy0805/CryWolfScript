using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        
        _loginViewModel.OnDirectLoginAuthFailed += ClearPasswordText;
        _loginViewModel.OnDirectLoginAuthFailed += ShowPasswordErrorMessage;
        _loginViewModel.OnDirectLoginNetworkFailed += ShowNetworkErrorMessage;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            await BindObjectsAsync();
            InitButtonEvents();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    protected override async Task BindObjectsAsync()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<TMP_InputField>(typeof(TextInputs));
        Bind<Toggle>(typeof(Toggles));
        
        await Managers.Localization.UpdateTextAndFont(_textDict);
        await Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.EmailInput));
        await Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.PasswordInput));
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
    
    private async Task OnLoginClicked(PointerEventData data)
    {
        _loginViewModel.UserAccount = GetTextInput((int)TextInputs.EmailInput).text;
        _loginViewModel.Password = GetTextInput((int)TextInputs.PasswordInput).text;
        await _loginViewModel.TryDirectLogin();
    }   
    
    private async Task SignUpClicked(PointerEventData data)
    {
        await _loginViewModel.SignUp();
    }
    
    private void OnForgetPasswordClicked(PointerEventData data)
    {
        _loginViewModel.ForgotPassword();
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }

    private async Task ShowPasswordErrorMessage()
    {
        var message = await Managers.Localization.GetLocalizedText("notify_sign_up_error_message_invalid_password");
        await Managers.UI.ShowErrorPopup(message);
    }
    
    private async Task ShowNetworkErrorMessage()
    {
        var message = await Managers.Localization.GetLocalizedText("notify_sign_up_unexpected_error_message");
        await Managers.UI.ShowErrorPopup(message);
    }
    
    private Task ClearPasswordText()
    {
        GetTextInput((int)TextInputs.PasswordInput).text = "";
        return Task.CompletedTask;
    }

    private void OnDestroy()
    {
        _loginViewModel.OnDirectLoginAuthFailed -= ClearPasswordText;
        _loginViewModel.OnDirectLoginAuthFailed -= ShowPasswordErrorMessage;
        _loginViewModel.OnDirectLoginNetworkFailed -= ShowNetworkErrorMessage;
    }
}
