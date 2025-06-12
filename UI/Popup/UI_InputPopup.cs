using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;

public class UI_InputPopup : UI_Popup
{
    private readonly Dictionary<string, GameObject> _textDict = new();
    private Func<string, Task> _inputAsyncCallback;
    
    // Title and Rule keys must be set before showing the popup
    public string TitleKey { get; set; }
    public string RuleKey { get; set; }
    
    private enum Buttons
    {
        ExitButton,
        EnterButton,
    }

    private enum TextInputs
    {
        InputText
    }

    private enum Texts
    {
        InputTitleText,
        RuleText,
        InputInfoText,
        WarningText,
        EnterText
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
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<TMP_InputField>(typeof(TextInputs));
        
        var enterText = GetText((int)Texts.EnterText).gameObject;
        Managers.Localization.UpdateInputFieldFont(GetTextInput((int)TextInputs.InputText));
        Managers.Localization.UpdateTextAndFont(enterText, "enter_text");
        Managers.Localization.UpdateTextAndFont(GetText((int)Texts.InputTitleText).gameObject, TitleKey);
        Managers.Localization.UpdateTextAndFont(GetText((int)Texts.RuleText).gameObject, RuleKey);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.EnterButton).gameObject.BindEvent(OnEnterClicked);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
    }

    protected override void InitUI()
    {
        GetText((int)Texts.WarningText).gameObject.SetActive(false);
    }
    
    public void SetInputAsyncCallback(Func<string, Task> callback)
    {
        _inputAsyncCallback = callback;
    }

    private async Task OnEnterClicked(PointerEventData data)
    {
        var inputText = GetTextInput((int)TextInputs.InputText).text;

        if (_inputAsyncCallback != null)
        {
            await _inputAsyncCallback.Invoke(inputText);
        }
        
        Managers.UI.ClosePopupUI();
    }

    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
