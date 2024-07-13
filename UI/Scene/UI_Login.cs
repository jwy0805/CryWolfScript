using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Login : UI_Scene
{
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

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        SetButtonEvents();
        SetUI();
    }

    private void OnSignUpClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_SignUpPopup>();
    }
    
    private void OnLoginClicked(PointerEventData data)
    {
        string account = GetTextInput((int)TextInputs.Email).text;
        string password = GetTextInput((int)TextInputs.Password).text;
        
        var packet = new LoginUserAccountPacketRequired { UserAccount = account, Password = password };
        Managers.Web.SendPostRequest<LoginUserAccountPacketResponse>("UserAccount/Login", packet, response =>
        {
            account = "";
            password = "";
            
            if (response.LoginOk == false) return;
            Managers.Scene.LoadScene(Define.Scene.MainLobby);
            Managers.Clear();
            Managers.Token.SaveAccessToken(response.AccessToken);
            Managers.Token.SaveRefreshToken(response.RefreshToken);
        });
    }

    private void OnGoogleClicked(PointerEventData data)
    {
        
    }

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
}
