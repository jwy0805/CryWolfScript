using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class UI_NotifyPopup : UI_Popup
{
    public bool Failed { get; set; }
    
    private enum Buttons
    {
        NotifyButton,
    }
    
    private enum Texts
    {
        NotifyText,
    }

    private enum Images
    {
        WolfIcon,
    }

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
    }
    
    private void OnNotifyButtonClicked(PointerEventData data)
    {
        if (Failed) Managers.UI.ClosePopupUI();
        else Managers.UI.CloseAllPopupUI();
    }

    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.NotifyButton).gameObject.BindEvent(OnNotifyButtonClicked);
    }
    
    protected override void InitUI()
    {
        SetObjectSize(GetImage((int)Images.WolfIcon).gameObject, 0.25f);
        GetText((int)Texts.NotifyText).text = Failed ? "Create Failed" : "Create Success";
    }
}
