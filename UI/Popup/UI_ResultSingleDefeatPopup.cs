using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ResultSingleDefeatPopup : UI_Popup
{
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private enum Images
    {
        
    }
    
    private enum Buttons
    {
        ContinueButton,
    }

    private enum Texts
    {
        DefeatText,
        ContinueText
    }

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
        StopGame();
    }
    
    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _ = Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ContinueButton).gameObject.BindEvent(OnContinueClicked);
    }

    private void StopGame()
    {
        Managers.Network.Disconnect();
    }
    
    private void OnContinueClicked(PointerEventData data)
    {
        Managers.Scene.LoadScene(Define.Scene.SinglePlay);
        Managers.UI.ClosePopupUI();
    }
}
