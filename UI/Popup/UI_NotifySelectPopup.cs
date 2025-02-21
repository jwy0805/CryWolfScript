using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_NotifySelectPopup : UI_Popup
{
    public string TitleText { get; set; }
    public TMP_FontAsset TitleFont { get; set; }
    public int TitleFontSize { get; set; }
    public string MessageText { get; set; }
    public TMP_FontAsset MessageFont { get; set; }
    public int MessageFontSize { get; set; }
    public string ButtonText { get; set; }
    public TMP_FontAsset ButtonFont { get; set; }
    public int ButtonFontSize { get; set; }
    public Action YesCallback { get; set; }

    private enum Buttons
    {
        YesButton,
        NoButton,
        ExitButton,
    }

    private enum Texts
    {
        YesText,
        NoText,
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
        GetButton((int)Buttons.YesButton).gameObject.BindEvent(OnYesClicked);
        GetButton((int)Buttons.NoButton).gameObject.BindEvent(OnNoClicked);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
    }
    
    protected override void InitUI()
    {
        var yesText = GetText((int)Texts.YesText);
        yesText.text = Managers.Localization.GetLocalizedValue(yesText, "yes_text");
        if (ButtonFontSize != 0)
        {
            GetText((int)Texts.YesText).fontSize = ButtonFontSize;
        }

        var noText = GetText((int)Texts.NoText);
        noText.text = Managers.Localization.GetLocalizedValue(noText, "no_text");
        if (ButtonFontSize != 0)
        {
            GetText((int)Texts.NoText).fontSize = ButtonFontSize;
        }
    }
    
    public void SetYesCallback(Action callback)
    {
        YesCallback = callback;
    }

    private void OnYesClicked(PointerEventData data)
    {
        YesCallback?.Invoke();
    }
    
    private void OnNoClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI<UI_NotifySelectPopup>();
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
