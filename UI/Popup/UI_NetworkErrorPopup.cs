using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_NetworkErrorPopup : UI_Popup
{
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private enum Buttons
    {
        RetryButton
    }

    private enum Texts
    {
        NetworkErrorTitleText,
        NetworkErrorText,
        RetryText,
    }

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
    }

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        
        Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.RetryButton).gameObject.BindEvent(OnRetryClicked);
    }

    private void OnRetryClicked(PointerEventData data)
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            Managers.UI.ClosePopupUI();
        }
    }
}
