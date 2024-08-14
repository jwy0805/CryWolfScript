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
    
    private enum Buttons
    {
        LoginButton,
        SignUpButton,
        ForgotPasswordButton,
        AppleButton,
        GoogleButton,
        FacebookButton,
    }

    private enum TextInputs
    {
        Email,
        Password,
    }

    private enum Images
    {
        Background,
        AppleImage,
        GoogleImage,
        FacebookImage,
    }

    [Inject] // Initialize ViewModel
    public void Construct(LoginViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.OnLoginFailed -= ClearPasswordText;
        _viewModel.OnLoginFailed += ClearPasswordText;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        SetButtonEvents();
        SetUI();
        
        if (_viewModel == null)
        {
            Debug.LogError("ViewModel is null");
        }
    }

    private void OnSignUpClicked(PointerEventData data)
    {
        _viewModel.SignUp();
    }
    
    private void OnLoginClicked(PointerEventData data)
    {
        _viewModel.UserAccount = GetTextInput((int)TextInputs.Email).text;
        _viewModel.Password = GetTextInput((int)TextInputs.Password).text;
        _viewModel.TryDirectLogin();
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
    
    private void ClearPasswordText()
    {
        GetTextInput((int)TextInputs.Password).text = "";
    }

    #region SetUiSize

    protected override void SetBackgroundSize(RectTransform rectTransform)
    {
        Rect rect = rectTransform.rect;
        float canvasWidth = rect.width;
        float canvasHeight = rect.height;
        float backgroundHeight = canvasWidth * 1.2f;
        float nightSkyHeight = canvasHeight - backgroundHeight;
        
        RectTransform rtBackground = GameObject.Find("Background").GetComponent<RectTransform>();
        rtBackground.sizeDelta = new Vector2(canvasWidth, backgroundHeight);

        RectTransform rtNightSky = GameObject.Find("NightSky").GetComponent<RectTransform>();
        rtNightSky.sizeDelta = new Vector2(canvasWidth, nightSkyHeight);
    }

    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_InputField>(typeof(TextInputs));
        Bind<Image>(typeof(Images));
    }

    protected override void SetButtonEvents()
    {
        GetButton((int)Buttons.LoginButton).gameObject.BindEvent(OnLoginClicked);
        GetButton((int)Buttons.SignUpButton).gameObject.BindEvent(OnSignUpClicked);
    }
    
    protected override void SetUI()
    {
        SetBackgroundSize(gameObject.GetComponent<RectTransform>());
        
        SetObjectSize(GetImage((int)Images.AppleImage).gameObject, 1.0f);
        SetObjectSize(GetImage((int)Images.GoogleImage).gameObject, 1.0f);
        SetObjectSize(GetImage((int)Images.FacebookImage).gameObject, 1.0f);
    }

    #endregion
}
