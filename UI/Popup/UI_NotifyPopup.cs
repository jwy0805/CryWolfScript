using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class UI_NotifyPopup : UI_Popup
{
    private Func<Task> _yesAsyncCallback;
    private Action _yesCallback;
    private Action _exitCallback;

    public string TitleText { get; set; }
    public TMP_FontAsset TitleFont { get; set; }
    public int TitleFontSize { get; set; }
    public string MessageText { get; set; }
    public TMP_FontAsset MessageFont { get; set; }
    public int MessageFontSize { get; set; }
    public string ButtonText { get; set; }
    public TMP_FontAsset ButtonFont { get; set; }
    public int ButtonFontSize { get; set; }
    public bool Failed { get; set; }
    
    private enum Buttons
    {
        ConfirmButton,
        ExitButton,
    }
    
    private enum Texts
    {
        NotifyTitle,
        NotifyText,
        ConfirmButtonText,
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
        GetButton((int)Buttons.ConfirmButton).gameObject.BindEvent(OnNotifyClicked);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
    }
    
    protected override void InitUI()
    {
        GetText((int)Texts.NotifyTitle).text = TitleText;
        GetText((int)Texts.NotifyTitle).font = TitleFont;
        if (TitleFontSize != 0)
        {
            GetText((int)Texts.NotifyTitle).fontSize = TitleFontSize;
        }
        
        GetText((int)Texts.NotifyText).text = MessageText;
        GetText((int)Texts.NotifyText).font = MessageFont;
        if (MessageFontSize != 0)
        {
            GetText((int)Texts.NotifyText).fontSize = MessageFontSize;
        }
        
        GetText((int)Texts.ConfirmButtonText).text = ButtonText;
        GetText((int)Texts.ConfirmButtonText).font = ButtonFont;
        if (ButtonFontSize != 0)
        {
            GetText((int)Texts.ConfirmButtonText).fontSize = ButtonFontSize;
        }
    }
    
    public void SetYesCallback(Action callback)
    {
        _yesCallback = callback;
    }

    public void SetYesCallback(Func<Task> callback)
    {
        _yesAsyncCallback = callback;
    }

    public void SetExitCallback(Action callback)
    {
        _exitCallback = callback;
    }
    
    private void OnNotifyClicked(PointerEventData data)
    {
        _yesCallback?.Invoke();
        _yesAsyncCallback?.Invoke();
        Managers.UI.ClosePopupUI();
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        _exitCallback?.Invoke();
        Managers.UI.ClosePopupUI();
    }

    private void OnDestroy()
    {
        _yesCallback = null;
        _yesAsyncCallback = null;
        _exitCallback = null;
    }
}
