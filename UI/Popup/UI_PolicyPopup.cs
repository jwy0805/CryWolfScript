using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Debug = System.Diagnostics.Debug;

public class UI_PolicyPopup : UI_Popup
{
    private IWebService _webService;
    private ITokenService _tokenService;
    private MainLobbyViewModel _mainlobbyVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
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
    public void Construct(IWebService webService, ITokenService tokenService, MainLobbyViewModel mainLobbyVm)
    {
        _webService = webService;
        _tokenService = tokenService;
        _mainlobbyVm = mainLobbyVm;
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
        Bind<Toggle>(typeof(Toggles));
        Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.GoingPolicyButton).gameObject.BindEvent(OnGoingPolicyClicked); 
        GetButton((int)Buttons.GoingTermsButton).gameObject.BindEvent(OnGoingTermsClicked);
        GetButton((int)Buttons.YesButton).gameObject.BindEvent(OnYesClicked);
        GetButton((int)Buttons.NoButton).gameObject.BindEvent(OnNoClicked);
        
        var policyToggle = GetToggle((int)Toggles.PolicyToggle);
        policyToggle.onValueChanged.AddListener(value => Managers.Policy.ReadPolicy = value);
        policyToggle.isOn = true;
        
        var termsToggle = GetToggle((int)Toggles.TermsToggle);
        termsToggle.onValueChanged.AddListener(value => Managers.Policy.ReadTerms = value);
        termsToggle.isOn = true;
        
        var ageToggle = GetToggle((int)Toggles.AgeToggle);
        ageToggle.onValueChanged.AddListener(value => Managers.Policy.AgeUnder13 = value);
        ageToggle.isOn = false;
    }
    
    private void OnGoingPolicyClicked(PointerEventData data)
    {
        var url = Managers.Network.BaseUrl + "/privacy";
        Application.OpenURL(url);
    }
    
    private void OnGoingTermsClicked(PointerEventData data)
    {
        var url = Managers.Network.BaseUrl + "/terms";
        Application.OpenURL(url);
    }

    public void SetYesCallback(Action callback)
    {
        _yesCallback = callback;
    }
    
    private async Task OnYesClicked(PointerEventData data)
    {
        try
        {
            if (Managers.Policy.ReadPolicy == false || Managers.Policy.ReadTerms == false)
            {
                var text = _textDict["PolicyWarningText"].gameObject;
                text.SetActive(true);
                Managers.Localization.UpdateTextAndFont(text.gameObject, "policy_warning_text_must_agree");
                return;
            }

            Managers.Policy.SetCoppaConsent(Managers.Policy.AgeUnder13);

            var packet = new PolicyAgreedPacketRequired
            {
                AccessToken = _tokenService.GetAccessToken(),
                PolicyAgreed = true
            };

            var task = await _webService.SendWebRequestAsync<PolicyAgreedPacketResponse>(
                "UserAccount/PolicyAgreed", "PUT", packet);

            if (task.PolicyAgreedOk)
            {
                Managers.UI.ClosePopupUI<UI_PolicyPopup>();
                Managers.Policy.SetPolicyConsent(true);
            }
            else
            {
                var popup = Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                var titleKey = "notify_network_error_title";
                var messageKey = "notify_network_error_message";
                Managers.Localization.UpdateNotifyPopupText(popup, titleKey, messageKey);
            }
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
