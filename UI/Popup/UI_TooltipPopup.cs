using System;
using System.Threading.Tasks;
using Febucci.UI;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_TooltipPopup : UI_Popup
{
    private TutorialViewModel _tutorialVm;
    
    private GameObject _continueButton;
    private GameObject _infoBubble;
    private TextMeshProUGUI _speechBubbleText;
    private TextMeshProUGUI _infoBubbleText;
    private bool _typing;
    
    public string DialogKey { get; set; }

    private enum Images
    {
        ContinueButtonLine,
    }

    private enum Buttons
    {
        ContinueButton,
    }

    private enum Texts
    {
        ContinueButtonText,
    }
    
    [Inject]
    public void Construct(TutorialViewModel tutorialVm)
    {
        _tutorialVm = tutorialVm;
    }

    protected override async void Init()
    {
        try
        {
            base.Init();
            
            await BindObjectsAsync();
            InitButtonEvents();
            await InitUIAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override async Task BindObjectsAsync()
    {
        Bind<Button>(typeof(Buttons)); 
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _continueButton = GetButton((int)Buttons.ContinueButton).gameObject;
        _infoBubble = Util.FindChild(gameObject, "InfoBubble");
        _speechBubbleText = Util.FindChild(gameObject, "SpeechBubbleText", true).GetComponent<TextMeshProUGUI>();
        _infoBubbleText = Util.FindChild(_infoBubble, "InfoBubbleText", true).GetComponent<TextMeshProUGUI>();
        
        var text = await Managers.Localization.BindLocalizedText(_infoBubbleText, DialogKey);
        _infoBubbleText.text = text;
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ContinueButton).gameObject.BindEvent(OnContinueClicked);
    }
    
    public void OnTypeStarted()
    {
        _typing = true;
    }

    public void OnTextShowed()
    {
        _typing = false;
    }
    
    private void OnContinueClicked(PointerEventData data)
    {
        if (_typing)
        {
            _speechBubbleText.GetComponent<TypewriterByCharacter>().SkipTypewriter();
            _typing = false;
        }
        else
        {
            Managers.UI.ClosePopupUI();
        }
    }
}
