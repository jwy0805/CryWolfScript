using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_NoticePopup : UI_Popup
{
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public string TitleText { get; set; }
    public string ContentText { get; set; }

    enum Buttons
    {
        EnterButton,
    }

    enum Texts
    {
        NoticePopupTitleText,
        NoticePopupContentText,
        EnterText,
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            BindObjects();
            InitButtonEvents();
            await InitUIAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        
        var str = Managers.Localization.GetLocalizedText("enter_text");
        _textDict["EnterText"].GetComponent<TextMeshProUGUI>().text = str;
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.EnterButton).onClick.AddListener(ClosePopupUI);
    }

    protected override async Task InitUIAsync()
    {
        var title = _textDict["NoticePopupTitleText"].GetComponent<TextMeshProUGUI>();
        var content = _textDict["NoticePopupContentText"].GetComponent<TextMeshProUGUI>();
        var enter = _textDict["EnterText"].GetComponent<TextMeshProUGUI>();
        var updateTitleTask = Managers.Localization.UpdateFont(title, FontType.BlackLined);
        var updateContentTask = Managers.Localization.UpdateFont(content);
        var updateEnterTask = Managers.Localization.UpdateFont(enter, FontType.BlackLined);
        
        title.text = TitleText;
        content.text = ContentText;
        
        await Task.WhenAll(updateTitleTask, updateContentTask, updateEnterTask);
    }

    private void ClosePopupUI()
    {
        Managers.UI.ClosePopupUI();
    }
}
