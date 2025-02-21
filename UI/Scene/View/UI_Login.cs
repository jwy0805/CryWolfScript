using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class UI_Login : UI_Scene
{
    private LoginViewModel _viewModel;
    private Dictionary<string, GameObject> _textDict = new();
    
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
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
    }

    #region SetUiSize

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        
        Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.SignUpButton).gameObject.BindEvent(OnSignUpClicked);
        GetButton((int)Buttons.AppleButton).gameObject.BindEvent(OnAppleClicked);
        GetButton((int)Buttons.GoogleButton).gameObject.BindEvent(OnGoogleClicked);
        GetButton((int)Buttons.GuestLoginButton).gameObject.BindEvent(OnGuestLoginClicked);
    }

    #endregion
    
    private void OnSignUpClicked(PointerEventData data)
    {
        _viewModel.SignIn();
    }
    
    private void OnLoginClicked(PointerEventData data)
    {
        
    }

    private void OnGoogleClicked(PointerEventData data)
    {
        
    }
    
    private void OnAppleClicked(PointerEventData data)
    {
#if UNITY_IOS && !UNITY_EDITOR
        // 버튼 클릭 시 ViewModel에 요청 전달 (추가 로직이 필요하다면)
        viewModel.RequestAppleSignIn();
        // 그리고 실제 네이티브 함수 호출
        startAppleSignIn();
#else
        Debug.Log("Apple Sign In은 iOS 기기에서만 동작합니다.");
#endif
    }
    
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void startAppleSignIn();
#endif

    public void OnAppleLoginSuccess(string userId)
    {
        _viewModel.HandleAppleSignInSuccess();
    }
    
    public void OnAppleLoginFailed(string error)
    {
        _viewModel.HandleAppleSignInFailed();
    }
    
    private void OnGuestLoginClicked(PointerEventData data)
    {
        
    }
}
