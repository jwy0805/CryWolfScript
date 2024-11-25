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
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public bool Failed { get; set; }
    
    private enum Buttons
    {
        ConfirmButton,
    }
    
    private enum Texts
    {
        NotifyTitle,
        NotifyText,
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
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ConfirmButton).gameObject.BindEvent(OnNotifyButtonClicked);
    }
    
    protected override void InitUI()
    {
        GetText((int)Texts.NotifyTitle).text = Title;
        GetText((int)Texts.NotifyText).text = Text;
    }
    
    private void OnNotifyButtonClicked(PointerEventData data)
    {
        if (Failed) Managers.UI.ClosePopupUI();
        else Managers.UI.CloseAllPopupUI();
    }
}
