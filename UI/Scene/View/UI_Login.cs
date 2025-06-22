using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_Login : UI_Scene
{
    private LoginViewModel _viewModel;

    private readonly Dictionary<string, GameObject> _textDict = new();

    private enum Buttons
    {
        SignUpButton,
        AppleButton,
        GoogleButton,
        GuestLoginButton,
    }

    private enum Texts
    {
        GoogleButtonText,
        AppleButtonText,
        SignUpButtonText,
        GuestLoginButtonText,
    }

    private enum Images
    {

    }

    [Inject] // Initialize ViewModel
    public void Construct(LoginViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    protected override async void Init()
    {
        try
        {
            base.Init();
            await BindObjectsAsync();
            InitButtonEvents();
            InitEvents();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private void Update()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _viewModel.UpdateAppleAuthManager();
#endif
    }

    #region SetUiSize

    protected override async Task BindObjectsAsync()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        
        await Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.SignUpButton).gameObject.BindEvent(OnSignUpClicked);
        GetButton((int)Buttons.AppleButton).gameObject.BindEvent(OnAppleClicked);
        GetButton((int)Buttons.GoogleButton).gameObject.BindEvent(OnGoogleClicked);
        GetButton((int)Buttons.GuestLoginButton).gameObject.BindEvent(OnGuestLoginClicked);
    }

    private void InitEvents()
    {
        _viewModel.OnRestoreButton += RestoreButton;
    }
    
    #endregion
    
    private async Task OnSignUpClicked(PointerEventData data)
    {
        await _viewModel.SignIn();
    }

    private void OnGuestLoginClicked(PointerEventData data)
    {
        _viewModel.TryGuestLogin();
    }
    
    private void OnGoogleClicked(PointerEventData data)
    {
        if (_viewModel.ProcessingLogin) return;
        _viewModel.ProcessingLogin = true;
        _viewModel.RequestGoogleLogin();
        data.pointerPress.gameObject.GetComponent<Button>().interactable = false;
    }

    private async Task OnAppleClicked(PointerEventData data)
    {
#if UNITY_ANDROID
        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        const string titleKey = "notify_sign_in_error_title";
        const string messageKey = "notify_apple_login_not_supported_message";
        await Managers.Localization.UpdateNotifyPopupText(popup, titleKey, messageKey);
        return;
#endif
        if (_viewModel.ProcessingLogin) return;
        _viewModel.ProcessingLogin = true;
        _viewModel.RequestAppleLogin();
        data.pointerPress.gameObject.GetComponent<Button>().interactable = false;
        await Task.Delay(100);
    }

    private void RestoreButton()
    {
        GetButton((int)Buttons.GoogleButton).gameObject.SetActive(true);
        GetButton((int)Buttons.AppleButton).gameObject.SetActive(true);
    }
    
    private void OnDestroy()
    {
        _viewModel.OnRestoreButton -= RestoreButton;
    }
}
