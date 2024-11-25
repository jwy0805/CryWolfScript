using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_SignUpPopup : UI_Popup
{
    private IWebService _webService;
    private LoginViewModel _loginViewModel;
    
    private const string AllowedSpecialCharacters = "!@#$%^&*()-_=+[]{};:,.?";

    private enum Buttons
    {
        ExitButton,
        VerifyButton,
        GoingPolicyButton,
        GoingTermsButton,
    }
    
    private enum TextInputs
    {
        EmailInput,
        PasswordInput,
        PasswordConfirmInput,
    }

    private enum Texts
    {
        WarningText,
    }

    private enum Toggles
    {
        PolicyToggle,
        TermsToggle,
    }
    
    [Inject]
    public void Construct(IWebService webService, LoginViewModel loginViewModel)
    {
        _webService = webService;
        _loginViewModel = loginViewModel;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
    }

    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<TMP_InputField>(typeof(TextInputs));
        Bind<Toggle>(typeof(Toggles));
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
        GetButton((int)Buttons.VerifyButton).gameObject.BindEvent(OnEmailVerificationClicked);
        
        GetTextInput((int)TextInputs.PasswordInput).onEndEdit.AddListener(OnPasswordInputEnd);
        GetTextInput((int)TextInputs.PasswordConfirmInput).onEndEdit.AddListener(OnPasswordConfirmInputEnd);
        
        var policyToggle = GetToggle((int)Toggles.PolicyToggle);
        policyToggle.onValueChanged.AddListener(value => _loginViewModel.ReadPolicy = value);
        policyToggle.isOn = false;
        
        var termsToggle = GetToggle((int)Toggles.TermsToggle);
        termsToggle.onValueChanged.AddListener(value => _loginViewModel.ReadTerms = value);
        termsToggle.isOn = false;
    }

    protected override void InitUI()
    {
        GetText((int)Texts.WarningText).gameObject.SetActive(false);
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }

    private void OnPasswordInputEnd(string text)
    {
        bool hasNumber = false;
        bool hasSpecialChar = false;
        bool hasLetter = false;
            
        foreach (var c in text)
        {
            if (char.IsDigit(c))
            {
                hasNumber = true;
            }
            else if (char.IsLetter(c))
            {
                hasLetter = true;
            }
            else 
            {
                if (AllowedSpecialCharacters.Contains(c))
                {
                    hasSpecialChar = true;
                }
                else
                {
                    var warningText = GetText((int)Texts.WarningText);
                    warningText.gameObject.SetActive(true);
                    warningText.GetComponent<TextMeshProUGUI>().text = 
                        "The special characters allowed in the password are\n" + AllowedSpecialCharacters;
                    return;
                }
            }

            if (hasNumber && hasLetter && hasSpecialChar)
            {
                break;
            }
        }

        if (text.Length < 8)
        {
            var warningText = GetText((int)Texts.WarningText);
            warningText.gameObject.SetActive(true);
            warningText.GetComponent<TextMeshProUGUI>().text = "Invalid password.";
        }
        
        if (hasNumber && hasLetter && hasSpecialChar)
        {
            GetText((int)Texts.WarningText).gameObject.SetActive(false);
        }
        else
        {
            var warningText = GetText((int)Texts.WarningText);
            warningText.gameObject.SetActive(true);
            warningText.GetComponent<TextMeshProUGUI>().text = "Invalid password.";
        }
    }
    
    private void OnPasswordConfirmInputEnd(string text)
    {
        var warningText = GetText((int)Texts.WarningText);

        if (text != GetTextInput((int)TextInputs.PasswordInput).text)
        {
            warningText.gameObject.SetActive(true);
            warningText.GetComponent<TextMeshProUGUI>().text = "The password does not match";
        }
        else
        {
            OnPasswordInputEnd(text); 
        }
    }
    
    private async void OnEmailVerificationClicked(PointerEventData data)
    {
        if (_loginViewModel.ReadPolicy == false || _loginViewModel.ReadTerms == false)
        {
            var text = GetText((int)Texts.WarningText);
            text.gameObject.SetActive(true);
            text.GetComponent<TextMeshProUGUI>().text = "Must agree to the terms and policies";
            return;
        }
        
        var account = GetTextInput((int)TextInputs.EmailInput).text;
        var password = GetTextInput((int)TextInputs.PasswordInput).text;
        var passwordConfirm = GetTextInput((int)TextInputs.PasswordConfirmInput).text;

        if (password != passwordConfirm) return;

        try
        {
            var validateAccountPacket = new ValidateNewAccountPacketRequired { UserAccount = account, Password = password };
            var timeoutTask = Task.Delay(5000);
            var task = _webService.SendWebRequestAsync<ValidateNewAccountPacketResponse>(
                "UserAccount/ValidateNewAccount", "POST", validateAccountPacket);
            
            GetButton((int)Buttons.VerifyButton).interactable = false;

            var completedTask = await Task.WhenAny(task, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                popup.Title = "Network Error";
                popup.Text = "Time out - Please check your network connection.";
            }
            
            var response = await task;
            
            if (response.ValidateOk)
            {
                var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                popup.Title = "Validation Email Sent";
                popup.Text = "Please check your email to verify your account.";
            }
            else
            {
                var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                popup.Title = "Sign Up Error";

                switch (response.ErrorCode)
                {
                    case 0:
                        break;
                    case 1:
                        popup.Text = "The email address is already in use.";
                        break;
                    case 2:
                        popup.Text = "The password is invalid.";
                        break;
                    default:
                        break;
                }
            }
        }
        catch (Exception e)
        {
            var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            popup.Title = "Unexpected Error";
            popup.Text = $"An unexpected error occurred - {e}. Please try again later.";
        }
        
        // var createAccountPacket = new CreateUserAccountPacketRequired { UserAccount = account, Password = password };
        // var response = await _webService.SendWebRequestAsync<CreateUserAccountPacketResponse>(
        //     "UserAccount/CreateAccount", "POST", createAccountPacket);
        // if (response.CreateOk == false) Debug.LogError("유저 정보 초기화 오류");
    }
}
