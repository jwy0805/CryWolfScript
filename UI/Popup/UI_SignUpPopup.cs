using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_SignUpPopup : UI_Popup
{
    private enum Buttons
    {
        ExitButton,
        SignUpButton,
    }
    
    private enum TextInputs
    {
        Email,
        Password,
        PasswordConfirm,
    }

    private IWebService _webService;
    
    [Inject]
    public void Construct(IWebService webService)
    {
        _webService = webService;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
    }

    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
    
    private async void OnSignUpClicked(PointerEventData data)
    {
        var account = GetTextInput((int)TextInputs.Email).text;
        var password = GetTextInput((int)TextInputs.Password).text;
        var passwordConfirm = GetTextInput((int)TextInputs.PasswordConfirm).text;

        if (password != passwordConfirm)
        {
            var notify = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            notify.Failed = true;
            GetTextInput((int)TextInputs.PasswordConfirm).text = "";
        }
        else
        {
            var notify = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
            notify.Failed = false;
            
            var createAccountPacket = new CreateUserAccountPacketRequired { UserAccount = account, Password = password };
            var response = await _webService.SendWebRequestAsync<CreateUserAccountPacketResponse>(
                "UserAccount/CreateAccount", "POST", createAccountPacket);
            if (response.CreateOk == false) Debug.LogError("유저 정보 초기화 오류");
        }
    }
    
    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_InputField>(typeof(TextInputs));
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
        GetButton((int)Buttons.SignUpButton).gameObject.BindEvent(OnSignUpClicked);
    }
}
