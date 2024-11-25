using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GameResultPopup : UI_Popup
{
    private enum Images
    {
        
    }
    
    private enum Buttons
    {
        EnterButton,
    }

    private enum Texts
    {
        
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
    }
    
    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.EnterButton).gameObject.BindEvent(OnEnterButton);
    }

    private void OnEnterButton(PointerEventData data)
    {
        Managers.Network.Disconnect();
        Managers.Scene.LoadScene(Define.Scene.MainLobby);
    }
}
