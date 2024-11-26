using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_FriendsListPopup : UI_Popup
{
    private enum Images
    {
        AlertImage,
    }
    
    private enum Buttons
    {
        InviteButton,
        ExitButton,
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
        Bind<Button>(typeof(Buttons));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.InviteButton).gameObject.BindEvent(OnInviteClicked);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
    }

    protected override void InitUI()
    {
        GetImage((int)Images.AlertImage).gameObject.SetActive(false);
    }

    private void OnInviteClicked(PointerEventData data)
    {
        
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
