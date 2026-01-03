using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;
using Debug = System.Diagnostics.Debug;

public class UI_PolicyPopup : UI_Popup
{
    private IWebService _webService;
    private ITokenService _tokenService;
    
    private readonly Dictionary<string, GameObject> _textDict = new();

    private TaskCompletionSource<PolicyPopupResult> _resultTcs;
    private bool _agreedPolicy = true;
    private bool _agreedTerms = true;
    private bool _isUnder13 = false;
    
    private Action _yesCallback; // Only yes callback is used because applying no will quit the app.

    private enum Buttons
    {
        GoingPolicyButton,
        GoingTermsButton,
        YesButton,
        NoButton,
    }

    private enum Texts
    {
        PolicyTitleText,
        PolicyInfoText,
        PolicyPrivacyPolicyText,
        PolicyTermOfServiceText,
        PolicyAgeText,
        PolicyWarningText,
        YesText,
        NoText,
    }
    
    private enum Toggles
    {
        PolicyToggle,
        TermsToggle,
        AgeToggle,
    }

    [Inject]
    public void Construct(IWebService webService, ITokenService tokenService)
    {
        _webService = webService;
        _tokenService = tokenService;
    }
    
    public Task<PolicyPopupResult> WaitResultAsync()
    {
        _resultTcs = new TaskCompletionSource<PolicyPopupResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        return _resultTcs.Task;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
        InitDefaultStates();
    }

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<Toggle>(typeof(Toggles));
        
        _ = Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.GoingPolicyButton).gameObject.BindEvent(OnGoingPolicyClicked); 
        GetButton((int)Buttons.GoingTermsButton).gameObject.BindEvent(OnGoingTermsClicked);
        GetButton((int)Buttons.YesButton).gameObject.BindEvent(OnYesClicked);
        GetButton((int)Buttons.NoButton).gameObject.BindEvent(OnNoClicked);
        
        var policyToggle = GetToggle((int)Toggles.PolicyToggle);
        policyToggle.onValueChanged.AddListener(value => _agreedPolicy = value);
        
        var termsToggle = GetToggle((int)Toggles.TermsToggle);
        termsToggle.onValueChanged.AddListener(value => _agreedTerms = value);
        
        var ageToggle = GetToggle((int)Toggles.AgeToggle);
        ageToggle.onValueChanged.AddListener(isOver13 => _isUnder13 = !isOver13);
    }

    protected override void InitUI()
    {
        _textDict["PolicyWarningText"].gameObject.SetActive(false);
    }

    private void InitDefaultStates()
    {
        _agreedPolicy = true;
        _agreedTerms = true;
        
        var ageToggle = GetToggle((int)Toggles.AgeToggle);
        ageToggle.isOn = true;
        _isUnder13 = !ageToggle.isOn;
        
        var policyToggle = GetToggle((int)Toggles.PolicyToggle);
        policyToggle.isOn = true;
        _agreedPolicy = policyToggle.isOn;
        
        var termsToggle = GetToggle((int)Toggles.TermsToggle);
        termsToggle.isOn = true;
        _agreedTerms = termsToggle.isOn;
    }
    
    private void OnGoingPolicyClicked(PointerEventData data)
    {
        Application.OpenURL(Managers.Network.BaseUrl + "/privacy");
    }
    
    private void OnGoingTermsClicked(PointerEventData data)
    {
        Application.OpenURL(Managers.Network.BaseUrl + "/terms");
    }
    
    private async Task OnYesClicked(PointerEventData data)
    {
        try
        {
            if (!_agreedPolicy || !_agreedTerms)
            {
                var textObject = _textDict["PolicyWarningText"].gameObject;
                textObject.SetActive(true);
                await Managers.Localization.UpdateTextAndFont(textObject, "policy_warning_text_must_agree");
                return;
            }
            
            var packet = new PolicyAgreedPacketRequired
            {
                AccessToken = _tokenService.GetAccessToken(),
                PolicyAgreed = true
            };

            var res = await _webService.SendWebRequestAsync<PolicyAgreedPacketResponse>(
                "UserAccount/PolicyAgreed", UnityWebRequest.kHttpVerbPOST, packet);

            if (!res.PolicyAgreedOk)
            {
                var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                await Managers.Localization.UpdateNotifyPopupText(
                    popup, "notify_network_error_message", "notify_network_error_title");
                return;
            }
            
            _resultTcs?.TrySetResult(new PolicyPopupResult(_agreedPolicy, _agreedTerms, _isUnder13));

            Managers.UI.ClosePopupUI<UI_PolicyPopup>();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }
    
    private void OnNoClicked(PointerEventData data)
    {
#if Unity_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
        Application.Quit();
#elif UNITY_IOS
        Process.GetCurrentProcess().Kill();
#else
        Application.Quit();
#endif
    }
}
