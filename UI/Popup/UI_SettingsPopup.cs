using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;

public class UI_SettingsPopup : UI_Popup
{
    private IUserService _userService;
    private IWebService _webService;
    private ITokenService _tokenService;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private Slider _musicSlider;
    private Slider _sfxSlider;
    
    private enum Images
    {
        MusicHandle,
        SfxHandle,
        NotificationHandle,
        Dimed,
        FlagImage,
        NotificationFill,
    }
    
    private enum Buttons
    {
        LanguageButton,
        AddReferrerButton,
        LinkSocialButton,
        LogoutButton,
        DeleteAccountButton,
        ExitButton,
    }

    private enum Texts
    {
        SettingsTitleText,
        SettingsMusicText,
        SettingsSfxText,
        SettingsLanguageText,
        LanguageText,
        SettingsNotificationText,
        NotificationText,
        SettingsAddReferrerText,
        SettingsLinkSocialText,
        SettingsLogoutText,
        SettingsDeleteAccountText,
    }

    [Inject]
    public void Construct(IUserService userService, IWebService webService, ITokenService tokenService)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            await BindObjectsAsync();
            InitButtonEvents();
            InitSliderCallbacks();
            InitUI();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override async Task BindObjectsAsync()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        
        _musicSlider = Util.FindChild<Slider>(gameObject, "MusicSlider", true);
        _sfxSlider = Util.FindChild<Slider>(gameObject, "SfxSlider", true);
        
        await Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.LanguageButton).gameObject.BindEvent(OnLanguageClicked);
        GetButton((int)Buttons.AddReferrerButton).gameObject.BindEvent(OnAddReferrerClicked);
        GetButton((int)Buttons.LinkSocialButton).gameObject.BindEvent(OnLinkSocialClicked);
        GetButton((int)Buttons.LogoutButton).gameObject.BindEvent(OnLogoutClicked);
        GetButton((int)Buttons.DeleteAccountButton).gameObject.BindEvent(OnDeleteAccountClicked);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
        
        GetImage((int)Images.Dimed).gameObject.BindEvent(OnExitClicked);
        GetImage((int)Images.NotificationHandle).gameObject.BindEvent(OnNotificationHandleClicked);
        
        var musicHandle = GetButton((int)Images.MusicHandle).gameObject;
        var sfxHandle = GetButton((int)Images.SfxHandle).gameObject;
        musicHandle.BindEvent(data => UpdateMusicVolume(data, _musicSlider), Define.UIEvent.Drag);
        sfxHandle.BindEvent(data => UpdateMusicVolume(data, _sfxSlider), Define.UIEvent.Drag);
    }

    private void InitSliderCallbacks()
    {
        _musicSlider.onValueChanged.AddListener(val =>
        {
            Managers.Sound.SetMusicVolume(val);
        });
        
        _sfxSlider.onValueChanged.AddListener(val =>
        {
            Managers.Sound.SetSfxVolume(val);
        });
    }
    
    protected override void InitUI()
    {
        _musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        _sfxSlider.value = PlayerPrefs.GetFloat("SfxVolume", 0.5f);
        
        Managers.Sound.SetMusicVolume(_musicSlider.value, save:false);
        Managers.Sound.SetSfxVolume(_sfxSlider.value, save:false);
        
        var notificationFill = GetImage((int)Images.NotificationFill);
        notificationFill.fillAmount = PlayerPrefs.GetInt("Notification", 1) == 1 ? 1 : 0;
        
        _ = UpdateFlag();
    }
    
    public async Task UpdateFlag()
    {
        var flagPath = $"Sprites/Icons/IconFlag/Small/icon_flag_{Managers.Localization.Language2Letter}";
        var flagImage = GetImage((int)Images.FlagImage);
        flagImage.sprite = await Managers.Resource.LoadAsync<Sprite>(flagPath);
    }

    private void UpdateMusicVolume(PointerEventData data, Slider slider)
    {
        var sliderRect = slider.transform as RectTransform;
        if (sliderRect == null) return;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                sliderRect, data.position, data.pressEventCamera, out var localPoint))
        {
            slider.value = Mathf.InverseLerp(sliderRect.rect.xMin, sliderRect.rect.xMax, localPoint.x);
        }
    }

    private void OnNotificationHandleClicked(PointerEventData data)
    {
        var notificationHandle = GetImage((int)Images.NotificationHandle);
        var notificationFill = GetImage((int)Images.NotificationFill);
        var notificationText = GetText((int)Texts.NotificationText);
        var handleRect = notificationHandle.GetComponent<RectTransform>();
        
        if (PlayerPrefs.GetInt("Notification", 1) == 0)
        {
            handleRect.anchoredPosition = new Vector2(57, 0);
            notificationFill.gameObject.SetActive(true);
            notificationText.text = "ON";
            PlayerPrefs.SetInt("Notification", 1);
        }
        else
        {
            handleRect.anchoredPosition = new Vector2(-57, 0);
            notificationFill.gameObject.SetActive(false);
            notificationText.text = "OFF";
            PlayerPrefs.SetInt("Notification", 0);
        }
    }
    
    private async Task OnLanguageClicked(PointerEventData data)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_LanguagePopup>();
        popup.SettingsPopup = this;
    }

    private async Task OnAddReferrerClicked(PointerEventData data)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        const string titleKey = "empty_text";
        const string messageKey = "notify_preparing_message";
        await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
    }
    
    private async Task OnLinkSocialClicked(PointerEventData data)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        const string titleKey = "empty_text";
        const string messageKey = "notify_preparing_message";
        await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
    }
    
    private async Task OnLogoutClicked(PointerEventData data)
    {
        var packet = new LogoutPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
        };
        
        var task = await _webService.SendWebRequestAsync<LogoutPacketResponse>(
            "UserAccount/Logout", UnityWebRequest.kHttpVerbDELETE, packet);

        if (task.LogoutOk)
        {
            _tokenService.ClearTokens();
            _userService.User.Clear();
            
            Managers.UI.ClosePopupUI();
            Managers.Scene.LoadScene(Define.Scene.Login);
            Managers.Clear();
        }
    }
    
    private async Task OnDeleteAccountClicked(PointerEventData data)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifySelectPopup>();
        const string titleKey = "notify_delete_account_title";
        const string messageKey = "notify_delete_account_message";
        await Managers.Localization.UpdateNotifySelectPopupText(popup, messageKey, titleKey);
        
        popup.SetYesCallbackF(async () =>
        {
            var packet = new DeleteUserAccountPacketRequired
            {
                AccessToken = _tokenService.GetAccessToken(),
            };
            
            var task = _webService.SendWebRequestAsync<DeleteUserAccountPacketResponse>(
                "UserAccount/DeleteAccount", UnityWebRequest.kHttpVerbDELETE, packet);
            
            await task;
            
            if (task.Result.DeleteOk)
            {
                _tokenService.ClearTokens();
                _userService.User.Clear();
                Managers.Clear();
                Managers.Scene.LoadScene(Define.Scene.Login);
            }
            else
            {
                var notifyPopup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
                const string title = "notify_network_error_title";
                const string message = "notify_network_error_message";
                await Managers.Localization.UpdateNotifyPopupText(notifyPopup, message, title);
            }
        });
        
        popup.SetNoCallBack(() =>
        {
            Managers.UI.ClosePopupUI();
        });
    }
    
    public async Task ChangeLanguage()
    {
        await Managers.Localization.UpdateChangedTextAndFont(_textDict, Managers.Localization.Language2Letter);
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        PlayerPrefs.Save();
        Managers.UI.ClosePopupUI();
    }
}
