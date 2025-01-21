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
        FacebookButton,
        GuestLoginButton,
    }

    private enum Texts
    {
        GoogleButtonText,
        FacebookButtonText,
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
        
    }
    
    private void OnFacebookClicked(PointerEventData data)
    {
        
    }
    
    private void OnGuestLoginClicked(PointerEventData data)
    {
        
    }
}
