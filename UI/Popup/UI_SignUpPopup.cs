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
    
    private readonly Dictionary<string, GameObject> _textDict = new();
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
        SignUpTitleText,
        SignUpWarningText,
        SignUpEmailText,
        SignUpPasswordText,
        SignUpPasswordConfirmText,
        SignUpPasswordRuleText,
        SignUpPrivacyPolicyText,
        SignUpTermOfServiceText,
        SignUpPolicyInfoText,
        SignUpEmailVerificationText,
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
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<TMP_InputField>(typeof(TextInputs));
        Bind<Toggle>(typeof(Toggles));
        
        Managers.Localization.UpdateTextAndFont(_textDict);
        Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.EmailInput));
        Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.PasswordInput));
        Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.PasswordConfirmInput));
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
        _textDict["SignUpWarningText"].gameObject.SetActive(false);
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
                    var warningText = _textDict["SignUpWarningText"].gameObject;
                    warningText.SetActive(true);
                    Managers.Localization.UpdateTextAndFont(warningText, 
                        "sign_up_warning_text_allowed_special_characters", 
                        $"\n{AllowedSpecialCharacters}");
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
            var warningText = _textDict["SignUpWarningText"].gameObject;
            warningText.SetActive(true);
            Managers.Localization.UpdateTextAndFont(warningText.gameObject, "sign_up_warning_text_password_length");
        }
        
        if (hasNumber && hasLetter && hasSpecialChar)
        {
            _textDict["SignUpWarningText"].gameObject.SetActive(false);
        }
        else
        {
            var warningText = _textDict["SignUpWarningText"].gameObject;
            warningText.SetActive(true);
            Managers.Localization.UpdateTextAndFont(warningText.gameObject, "sign_up_warning_text_invalid_password");
        }
    }
    
    private void OnPasswordConfirmInputEnd(string text)
    {
        var warningText = _textDict["SignUpWarningText"].gameObject;

        if (text != GetTextInput((int)TextInputs.PasswordInput).text)
        {
            warningText.gameObject.SetActive(true);
            Managers.Localization.UpdateTextAndFont(warningText.gameObject, "sign_up_warning_text_password_not_match");
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
            var text = _textDict["SignUpWarningText"].gameObject;
            text.SetActive(true);
            Managers.Localization.UpdateTextAndFont(text.gameObject, "sign_up_warning_text_must_agree");
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
                Managers.Localization.UpdateNotifyPopupText(popup, 
                    "notify_network_error_title", 
                    "notify_network_error_message");
            }
            
            var response = await task;
            
            if (response.ValidateOk)
            {
                var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                Managers.Localization.UpdateNotifyPopupText(popup, 
                    "notify_validation_email_sent_title", 
                    "notify_validation_email_sent_message");
            }
            else
            {
                var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();

                switch (response.ErrorCode)
                {
                    case 0:
                        break;
                    case 1:
                        Managers.Localization.UpdateNotifyPopupText(popup,
                            "notify_sign_up_error_title", 
                            "notify_sign_up_error_message_email_in_use");
                        break;
                    case 2:
                        Managers.Localization.UpdateNotifyPopupText(popup,
                            "notify_sign_up_error_title", 
                            "notify_sign_up_error_message_invalid_password");
                        break;
                    default:
                        break;
                }
            }
        }
        catch (Exception)
        {
            var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            Managers.Localization.UpdateNotifyPopupText(popup, 
                "notify_sign_up_unexpected_error_title", 
                "notify_sign_up_unexpected_error_message");
        }
    }
}
