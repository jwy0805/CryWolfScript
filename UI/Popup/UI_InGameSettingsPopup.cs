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

public class UI_InGameSettingsPopup : UI_Popup
{
    private IWebService _webService;
    private ITokenService _tokenService;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private Slider _musicSlider;
    private Slider _sfxSlider;
    
    private enum Images
    {
        MusicHandle,
        SfxHandle,
        BuildHandle,
        Dimed,
        BuildFill,
    }
    
    private enum Buttons
    {
        ExitButton,
    }

    private enum Texts
    {
        SettingsTitleText,
        SettingsMusicText,
        SettingsSfxText,
        SettingsBuildRecommendationText,
        BuildText,
    }

    [Inject]
    public void Construct(IWebService webService, ITokenService tokenService)
    {
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
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
        
        GetImage((int)Images.Dimed).gameObject.BindEvent(OnExitClicked);
        GetImage((int)Images.BuildHandle).gameObject.BindEvent(OnBuildHandleClicked);
        
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
        var buildFill = GetImage((int)Images.BuildFill);
        buildFill.fillAmount = PlayerPrefs.GetInt("buildRecommendation", 1) == 1 ? 1 : 0;
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

    private void OnBuildHandleClicked(PointerEventData data)
    {
        var handle = GetImage((int)Images.BuildHandle);
        var fill = GetImage((int)Images.BuildFill);
        var text = GetText((int)Texts.BuildText);
        var handleRect = handle.GetComponent<RectTransform>();
        
        if (PlayerPrefs.GetInt("buildRecommendation", 1) == 0)
        {
            handleRect.anchoredPosition = new Vector2(57, 0);
            fill.gameObject.SetActive(true);
            text.text = "ON";
            PlayerPrefs.SetInt("buildRecommendation", 1);
        }
        else
        {
            handleRect.anchoredPosition = new Vector2(-57, 0);
            fill.gameObject.SetActive(false);
            text.text = "OFF";
            PlayerPrefs.SetInt("buildRecommendation", 0);
        }
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
