using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
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
        SignUpEmailVerificationText,
    }
    
    [Inject]
    public void Construct(IWebService webService, LoginViewModel loginViewModel)
    {
        _webService = webService;
        _loginViewModel = loginViewModel;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            await BindObjectsAsync();
            InitButtonEvents();
            InitUI();
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
        
        await Managers.Localization.UpdateTextAndFont(_textDict);
        await Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.EmailInput));
        await Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.PasswordInput));
        await Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.PasswordConfirmInput));
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
        GetButton((int)Buttons.VerifyButton).gameObject.BindEvent(OnEmailVerificationClicked);
        
        GetTextInput((int)TextInputs.PasswordInput).onEndEdit.AddListener(OnPasswordInputEnd);
        GetTextInput((int)TextInputs.PasswordConfirmInput).onEndEdit.AddListener(OnPasswordConfirmInputEnd);
    }

    protected override void InitUI()
    {
        _textDict["SignUpWarningText"].gameObject.SetActive(false);
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }

    private async void OnPasswordInputEnd(string text)
    {
        try
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
                        await Managers.Localization.UpdateTextAndFont(warningText, 
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
                var warningKey = "sign_up_warning_text_password_length";
                warningText.SetActive(true);
                await Managers.Localization.UpdateTextAndFont(warningText.gameObject, warningKey);
            }
        
            if (hasNumber && hasLetter && hasSpecialChar)
            {
                _textDict["SignUpWarningText"].gameObject.SetActive(false);
            }
            else
            {
                var warningText = _textDict["SignUpWarningText"].gameObject;
                var warningKey = "sign_up_warning_text_invalid_password";
                warningText.SetActive(true);
                await Managers.Localization.UpdateTextAndFont(warningText.gameObject, warningKey);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    private async void OnPasswordConfirmInputEnd(string text)
    {
        try
        {
            var warningText = _textDict["SignUpWarningText"].gameObject;
            var warningKey = "sign_up_warning_text_password_not_match";
            
            if (text != GetTextInput((int)TextInputs.PasswordInput).text)
            {
                warningText.gameObject.SetActive(true);
                await Managers.Localization.UpdateTextAndFont(warningText.gameObject, warningKey);
            }
            else
            {
                OnPasswordInputEnd(text); 
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    private async void OnEmailVerificationClicked(PointerEventData data)
    {
        try
        {
            var account = GetTextInput((int)TextInputs.EmailInput).text;
            var password = GetTextInput((int)TextInputs.PasswordInput).text;
            var passwordConfirm = GetTextInput((int)TextInputs.PasswordConfirmInput).text;

            if (password != passwordConfirm) return;
            
            var validateAccountPacket = new ValidateNewAccountPacketRequired { UserAccount = account, Password = password };
            var timeoutTask = Task.Delay(5000);
            var task = _webService.SendWebRequestAsync<ValidateNewAccountPacketResponse>(
                "UserAccount/ValidateNewAccount", UnityWebRequest.kHttpVerbPOST, validateAccountPacket);
            
            GetButton((int)Buttons.VerifyButton).interactable = false;

            var completedTask = await Task.WhenAny(task, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                await Managers.Localization.UpdateNotifyPopupText(popup, 
                    "notify_network_error_message",
                    "notify_network_error_title");
            }
            
            var response = await task;
            
            if (response.ValidateOk)
            {
                var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                await Managers.Localization.UpdateNotifyPopupText(popup, 
                    "notify_validation_email_sent_message",
                    "notify_validation_email_sent_title");
            }
            else
            {
                var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();

                switch (response.ErrorCode)
                {
                    case 0:
                        break;
                    case 1:
                        await Managers.Localization.UpdateNotifyPopupText(popup,
                            "notify_sign_up_error_message_email_in_use",
                            "notify_sign_up_error_title");
                        break;
                    case 2:
                        await Managers.Localization.UpdateNotifyPopupText(popup,
                            "notify_sign_up_error_message_invalid_password",
                            "notify_sign_up_error_title");
                        break;
                    default:
                        break;
                }
            }
        }
        catch (Exception)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            await Managers.Localization.UpdateNotifyPopupText(popup,
                "notify_sign_up_unexpected_error_message",
                "notify_sign_up_unexpected_error_title");
        }
    }
}
