using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_GameMenuPopup : UI_Popup
{
    private IWebService _webService;
    private ITokenService _tokenService;
    
    private enum Buttons
    {
        ExitButton,
        SurrenderButton,
        OptionButton
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
        
        Bind<Button>(typeof(Buttons));
        
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.SurrenderButton).gameObject.BindEvent(OnSurrenderClicked);
        GetButton((int)Buttons.OptionButton).gameObject.BindEvent(OnOptionClicked);
    }

    private void OnSurrenderClicked(PointerEventData data)
    {
        var packet = new SurrenderPacketRequired { AccessToken = _tokenService.GetAccessToken() };
        _webService.SendWebRequestAsync<SurrenderPacketResponse>("Match/Surrender", "PUT", packet);
    }
    
    private void OnOptionClicked(PointerEventData data)
    {
        
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
