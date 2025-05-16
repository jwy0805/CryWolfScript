using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SettingsPopup : UI_Popup
{
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

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitSliderCallbacks();
        InitUI();
    }

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        
        _musicSlider = Util.FindChild<Slider>(gameObject, "MusicSlider", true);
        _sfxSlider = Util.FindChild<Slider>(gameObject, "SfxSlider", true);
        
        Managers.Localization.UpdateTextAndFont(_textDict);
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
            PlayerPrefs.SetFloat("musicVolume", val);
        });
        
        _sfxSlider.onValueChanged.AddListener(val =>
        {
            PlayerPrefs.SetFloat("sfxVolume", val);
        });
    }
    
    protected override void InitUI()
    {
        _musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 0.5f);
        _sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume", 0.5f);
        var notificationFill = GetImage((int)Images.NotificationFill);
        notificationFill.fillAmount = PlayerPrefs.GetInt("notification", 1) == 1 ? 1 : 0;
        
        UpdateFlag(Managers.Localization.Language2Letter);
    }
    
    public void UpdateFlag(string language2Letter)
    {
        var flagPath = $"Sprites/Icons/IconFlag/Small/icon_flag_{language2Letter}";
        var flagImage = GetImage((int)Images.FlagImage);
        flagImage.sprite = Managers.Resource.Load<Sprite>(flagPath);
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
        
        if (PlayerPrefs.GetInt("notification", 1) == 0)
        {
            handleRect.anchoredPosition = new Vector2(57, 0);
            notificationFill.gameObject.SetActive(true);
            notificationText.text = "ON";
            PlayerPrefs.SetInt("notification", 1);
        }
        else
        {
            handleRect.anchoredPosition = new Vector2(-57, 0);
            notificationFill.gameObject.SetActive(false);
            notificationText.text = "OFF";
            PlayerPrefs.SetInt("notification", 0);
        }
    }
    
    private void OnLanguageClicked(PointerEventData data)
    {
        var popup = Managers.UI.ShowPopupUI<UI_LanguagePopup>();
        popup.SettingsPopup = this;
    }

    private void OnAddReferrerClicked(PointerEventData data)
    {
        
    }
    
    private void OnLinkSocialClicked(PointerEventData data)
    {
        
    }
    
    private void OnLogoutClicked(PointerEventData data)
    {
        
    }
    
    private void OnDeleteAccountClicked(PointerEventData data)
    {
        
    }
    
    public void ChangeLanguage(string language2Letter)
    {
        Managers.Localization.UpdateChangedTextAndFont(_textDict, language2Letter);
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
