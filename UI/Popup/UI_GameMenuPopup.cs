using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_GameMenuPopup : UI_Popup
{
    private IWebService _webService;
    private ITokenService _tokenService;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private enum Buttons
    {
        ExitButton,
        SurrenderButton,
        SettingsButton
    }

    private enum Texts
    {
        GameMenuTitleText,
        GameMenuSurrenderText,
        GameMenuSettingsText
    }
    
    [Inject]
    public void Construct(IWebService webService, ITokenService tokenService)
    {
        _webService = webService;
        _tokenService = tokenService;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.SurrenderButton).gameObject.BindEvent(OnSurrenderClicked);
        GetButton((int)Buttons.SettingsButton).gameObject.BindEvent(OnSettingsClicked);
    }

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
    }

    private void OnSurrenderClicked(PointerEventData data)
    {
        var packet = new SurrenderPacketRequired { AccessToken = _tokenService.GetAccessToken() };
        _webService.SendWebRequestAsync<SurrenderPacketResponse>("Match/Surrender", "PUT", packet);
    }
    
    private void OnSettingsClicked(PointerEventData data)
    {
        
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
