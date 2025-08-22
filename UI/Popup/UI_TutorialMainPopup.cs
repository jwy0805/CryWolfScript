using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetKits.ParticleImage;
using Febucci.UI;
using Febucci.UI.Core;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using Zenject;

/* Last Modified : 25. 04. 22
 * Version : 1.02
 */

public class UI_TutorialMainPopup : UI_Popup
{
    private IWebService _webService;
    private ITokenService _tokenService;
    private TutorialViewModel _tutorialVm;
    
    private TutorialNpcInfo _tutorialNpcInfo1;
    private TutorialNpcInfo _tutorialNpcInfo2;
    private Camera _tutorialCamera1;
    private Camera _tutorialCamera2;
    private GameObject _flowerFace;
    private GameObject _speaker1Panel;
    private GameObject _speaker1Bubble;
    private GameObject _speaker2Panel;
    private GameObject _speaker2Bubble;
    private GameObject _continueButton;
    private GameObject _selectPanel;
    private GameObject _buttonSelectEffect;
    private GameObject _masking;
    private TextMeshProUGUI _tutorialMainSelectText;
    private TextMeshProUGUI _speechBubbleText;
    private TextMeshProUGUI _factionText;
    private TextMeshProUGUI _factionInfoText;
    private VideoPlayer _videoPlayer;
    private readonly Dictionary<string, VideoClip> _videoClips = new();
    private RawImage _wolfRawImage;
    private RawImage _sheepRawImage;
    private RawImage _videoRawImage;
    private RenderTexture _wolfRenderTexture;
    private RenderTexture _sheepRenderTexture;
    private RenderTexture _videoRenderTexture;
    private bool _typing;
    
    private enum Images
    {
        ContinueButtonLine,
        BackgroundLeft,
        BackgroundRight,
        SelectPanel,
    }

    private enum Buttons
    {
        ContinueButton,
        ExitButton,
        
        WolfButton,
        SheepButton,
        PlayButton
    }

    private enum Texts
    {
        ContinueButtonText,
    }
    
    [Inject]
    public void Construct(IWebService webService, ITokenService tokenService, TutorialViewModel tutorialViewModel)
    {
        _webService = webService;
        _tokenService = tokenService;
        _tutorialVm = tutorialViewModel;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            await BindObjectsAsync();
            BindActions();
            InitButtonEvents();
            await InitUIAsync();
            InitCamera();
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

        _speaker1Panel = Util.FindChild(gameObject, "LeftPanel");
        _speaker1Bubble = Util.FindChild(_speaker1Panel, "SpeechBubbleLeft");
        _speaker2Panel = Util.FindChild(gameObject, "RightPanel");
        _speaker2Bubble = Util.FindChild(_speaker2Panel, "SpeechBubbleRight");
        _continueButton = Util.FindChild(gameObject, "ContinueButton");
        _selectPanel = Util.FindChild(gameObject, "SelectPanel");
        _tutorialMainSelectText = Util.FindChild(_selectPanel, "TutorialMainSelectText", true).GetComponent<TextMeshProUGUI>();
        _factionText = Util.FindChild(_selectPanel, "FactionText", true).GetComponent<TextMeshProUGUI>();
        _factionInfoText = Util.FindChild(_selectPanel, "FactionInfoText", true).GetComponent<TextMeshProUGUI>();
        _buttonSelectEffect = await Managers.Resource.Instantiate("UIEffects/ButtonSelectEffect", _selectPanel.transform);
        _videoPlayer = Util.FindChild(_selectPanel, "VideoPlayer", true).GetComponent<VideoPlayer>();
        _masking = Util.FindChild(_selectPanel, "TutorialMainVideoMasking", true);
        
        var wolfVideo = await Managers.Resource.LoadAsync<VideoClip>("VideoClips/TutorialWolf", "mov");
        var sheepVideo = await Managers.Resource.LoadAsync<VideoClip>("VideoClips/TutorialSheep", "mov");
        _videoClips.Add("Wolf", wolfVideo);
        _videoClips.Add("Sheep", sheepVideo);
    }

    private void InitCamera()
    {
        var backgroundLeft = GetImage((int)Images.BackgroundLeft);
        var backgroundRight = GetImage((int)Images.BackgroundRight);
        var selectPanel = GetImage((int)Images.SelectPanel);
        
        _wolfRawImage = backgroundLeft.GetComponentInChildren<RawImage>();
        _sheepRawImage = backgroundRight.GetComponentInChildren<RawImage>();
        _videoRawImage = selectPanel.GetComponentInChildren<RawImage>();
        _wolfRenderTexture = Managers.Resource.CreateRenderTexture("wolfTexture");
        _sheepRenderTexture = Managers.Resource.CreateRenderTexture("sheepTexture");
        _videoRenderTexture = Managers.Resource.CreateRenderTexture("videoTexture");
        _tutorialCamera1 = GameObject.Find("TutorialCamera1").GetComponent<Camera>();
        _tutorialCamera2 = GameObject.Find("TutorialCamera2").GetComponent<Camera>();
        
        if (_tutorialCamera1 == null || _tutorialCamera2 == null)
        {
            Debug.LogError("Tutorial cameras not found in the scene.");
            return;
        }
        
        _wolfRawImage.texture = _wolfRenderTexture;
        _sheepRawImage.texture = _sheepRenderTexture;
        _videoRawImage.texture = _videoRenderTexture;

        _tutorialCamera1.targetTexture = _wolfRenderTexture;
        _tutorialCamera2.targetTexture = _sheepRenderTexture;
        _videoPlayer.targetTexture = _videoRenderTexture;
    }
    
    private void BindActions()
    {
        _tutorialVm.OnShowSpeakerAfter3Sec += ShowSpeaker;
        _tutorialVm.OnShowNewSpeaker += ShowNewSpeaker;
        _tutorialVm.OnChangeSpeaker += ChangeSpeaker;
        _tutorialVm.OnShowFactionSelectPopup += ShowFactionSelectPopup;
        _tutorialVm.OnChangeFaceCry += ChangeFaceCry;
        _tutorialVm.OnChangeFaceHappy += ChangeFaceHappy;
        _tutorialVm.OnChangeFaceNormal += ChangeFaceNormal;
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ContinueButton).gameObject.BindEvent(OnContinueClicked);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
        GetButton((int)Buttons.WolfButton).gameObject.BindEvent(OnWolfClicked);
        GetButton((int)Buttons.SheepButton).gameObject.BindEvent(OnSheepClicked);
        GetButton((int)Buttons.PlayButton).gameObject.BindEvent(OnPlayClicked);
    }
    
    protected override async Task InitUIAsync()
    {
        var factionText = Util.FindChild(_selectPanel, "FactionText", true).GetComponent<TextMeshProUGUI>();
        factionText.text = await Managers.Localization.BindLocalizedText(factionText, "tutorial_main_faction_text");
        
        _speaker1Panel.SetActive(false);
        _speaker2Panel.SetActive(false);
        _continueButton.SetActive(false);
        _selectPanel.SetActive(false);
        _buttonSelectEffect.SetActive(false);
        
        var tutorialNpc1 = await Managers.Resource.Instantiate("Npc/NpcWerewolf");
        var tutorialNpc2 = await Managers.Resource.Instantiate("Npc/NpcFlower");
        _tutorialNpcInfo1 = tutorialNpc1.GetComponent<TutorialNpcInfo>();
        _tutorialNpcInfo2 = tutorialNpc2.GetComponent<TutorialNpcInfo>();
        _flowerFace = Util.FindChild(tutorialNpc2, "+ Head", true);
        
        var npc1Pos = _tutorialNpcInfo1.Position;
        var npc2Pos = _tutorialNpcInfo2.Position;
        var camera1Pos = _tutorialNpcInfo1.CameraPosition;
        var camera2Pos = _tutorialNpcInfo2.CameraPosition;
        _tutorialVm.InitTutorialMain(npc1Pos, camera1Pos, npc2Pos, camera2Pos);
        
        await StepTutorial();
        StartCoroutine(nameof(SmoothAlphaRoutine));
    }
    
    # region Flicker text
    
    private IEnumerator SmoothAlphaRoutine()
    {
        float highAlpha = 1f;           
        float lowAlpha = 120f / 255f;  
        float duration = 1f;         

        while (true)
        {
            // 1) highAlpha -> lowAlpha
            yield return StartCoroutine(LerpAlpha(highAlpha, lowAlpha, duration));

            // 2) lowAlpha -> highAlpha
            yield return StartCoroutine(LerpAlpha(lowAlpha, highAlpha, duration));
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator LerpAlpha(float from, float to, float duration)
    {
        if (_continueButton.activeSelf == false)
        {
            yield break;
        }
        
        float elapsed = 0f;
        var targetImage = GetImage((int)Images.ContinueButtonLine);
        var targetText = GetText((int)Texts.ContinueButtonText);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentAlpha = Mathf.Lerp(from, to, t);
            
            SetImageAlpha(targetImage, currentAlpha);
            SetTextAlpha(targetText, currentAlpha);

            yield return null;
        }
    }
    
    private void SetImageAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        var color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text == null) return;
        var color = text.color;
        color.a = alpha;
        text.color = color;
    }
    
    # endregion

    private async Task StepTutorial()
    {
        _tutorialVm.Step++;
        Managers.Data.TutorialDict.TryGetValue(TutorialType.Main, out var tutorialData);
        var step = tutorialData?.Steps.Find(s => s.Step == _tutorialVm.Step);
        if (step == null) return;
        foreach (var eventString in step.Events)
        {
            _tutorialVm.MainEventDict[eventString]?.Invoke();
        }
        
        var textContent = await Managers.Localization.BindLocalizedText(_speechBubbleText, step.DialogKey);
        _speechBubbleText.text = textContent;
    }

    private void ShowSpeaker()
    {
        _speaker1Panel.SetActive(true);
        _continueButton.SetActive(true);
        _speechBubbleText = Util.FindChild(_speaker1Panel, "SpeechBubbleText", true).GetComponent<TextMeshProUGUI>();
    }
    
    private void ShowNewSpeaker()
    {
        _speaker2Panel.SetActive(true);
        _speaker1Bubble.SetActive(false);
        _continueButton.SetActive(true);
        _speechBubbleText = Util.FindChild(_speaker2Panel, "SpeechBubbleText", true).GetComponent<TextMeshProUGUI>();
    }

    private void ChangeSpeaker()
    {
        var index1 = _speaker1Panel.transform.GetSiblingIndex();
        var index2 = _speaker2Panel.transform.GetSiblingIndex();
        if (index1 > index2)
        {
            _speaker1Panel.transform.SetSiblingIndex(index2);
            _speaker1Bubble.SetActive(false);
            _speaker2Panel.transform.SetSiblingIndex(index1);
            _speaker2Bubble.SetActive(true);
            _speechBubbleText = Util.FindChild(_speaker2Panel, "SpeechBubbleText", true).GetComponent<TextMeshProUGUI>();
        }
        else
        {
            _speaker1Panel.transform.SetSiblingIndex(index2);
            _speaker1Bubble.SetActive(true);
            _speaker2Panel.transform.SetSiblingIndex(index1);
            _speaker2Bubble.SetActive(false);
            _speechBubbleText = Util.FindChild(_speaker1Panel, "SpeechBubbleText", true).GetComponent<TextMeshProUGUI>();
        }
    }
    
    private async void ShowFactionSelectPopup()
    {
        try
        {
            _selectPanel.SetActive(true);
            _masking.SetActive(false);
        
            var playButtonText = Util.FindChild(_selectPanel, "PlayButtonText", true).GetComponent<TextMeshProUGUI>();
            var titleText = Util.FindChild(_selectPanel, "TutorialMainSelectText", true).GetComponent<TextMeshProUGUI>();
            playButtonText.text = await Managers.Localization.BindLocalizedText(playButtonText, "play_button_text");
            titleText.text = await Managers.Localization.BindLocalizedText(titleText, "tutorial_main_select_text");
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private async void ChangeFaceNormal()
    {
        try
        {
            Util.DestroyAllChildren(_flowerFace.transform);
            await Managers.Resource.Instantiate("Npc/Blinking Eyes 01", _flowerFace.transform);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private async void ChangeFaceHappy()
    {
        try
        {
            Util.DestroyAllChildren(_flowerFace.transform);
            await Managers.Resource.Instantiate("Npc/Face Happy", _flowerFace.transform);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private async void ChangeFaceCry()
    {
        try
        {
            Util.DestroyAllChildren(_flowerFace.transform);
            await Managers.Resource.Instantiate("Npc/Face Cry", _flowerFace.transform);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private IEnumerator AlertRoutine()
    {
        var text = _tutorialMainSelectText;
        
        text.color = Color.red;
        yield return new WaitForSeconds(0.75f);
        text.color = Color.white;
    }

    private void PlayFactionVideo(VideoClip clip)
    {
        _videoPlayer.Stop();
        _videoPlayer.source = VideoSource.VideoClip;
        _videoPlayer.clip = clip;
        _videoPlayer.Play();
    }
    
    #region ButtonEvents
    
    private async Task OnContinueClicked(PointerEventData data)
    {
        if (_typing)
        {
            _speechBubbleText.GetComponent<TypewriterByCharacter>().SkipTypewriter();
            _typing = false;
        }
        else
        {
            await StepTutorial();
        }
    }

    private async Task OnWolfClicked(PointerEventData data)
    {
        if (_masking.activeSelf == false)
        {
            _masking.SetActive(true);
        }

        Util.Faction = Faction.Wolf;
        _tutorialVm.TutorialFaction = Faction.Wolf;
        _factionText.text = await Managers.Localization.BindLocalizedText(_factionText, "wolf_text");
        _factionInfoText.text = await Managers.Localization.BindLocalizedText(_factionInfoText, "faction_info_text_wolf");
        _buttonSelectEffect.SetActive(true);
        _buttonSelectEffect.transform.position = GetButton((int)Buttons.WolfButton).transform.position;
        _buttonSelectEffect.GetComponent<ParticleImage>().Play();

        PlayFactionVideo(_videoClips["Wolf"]);
    }
    
    private async Task OnSheepClicked(PointerEventData data)
    {
        if (_masking.activeSelf == false)
        {
            _masking.SetActive(true);
        }
        
        Util.Faction = Faction.Sheep;
        _tutorialVm.TutorialFaction = Faction.Sheep;
        _factionText.text = await Managers.Localization.BindLocalizedText(_factionText, "sheep_text");
        _factionInfoText.text = await Managers.Localization.BindLocalizedText(_factionInfoText, "faction_info_text_sheep");
        _buttonSelectEffect.SetActive(true);
        _buttonSelectEffect.transform.position = GetButton((int)Buttons.SheepButton).transform.position;
        _buttonSelectEffect.GetComponent<ParticleImage>().Play();
        
        PlayFactionVideo(_videoClips["Sheep"]);
    }

    private void OnPlayClicked(PointerEventData data)
    {
        if (_tutorialVm.TutorialFaction == Faction.None)
        {
            StartCoroutine(nameof(AlertRoutine));
            return;
        }

        _tutorialVm.ProcessTutorial = true;
        _ = Managers.Network.ConnectGameSession();
    }
    
    private async Task OnExitClicked(PointerEventData data)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifySelectPopup>();
        
        await Managers.Localization.UpdateNotifySelectPopupText(popup,
            "notify_select_tutorial_exit_message",
            "notify_select_tutorial_exit_title");
        popup.SetYesCallbackF(async () =>
        {
            var packet = new UpdateTutorialRequired
            {
                AccessToken = _tokenService.GetAccessToken(),
                TutorialTypes = new[] { TutorialType.BattleSheep, TutorialType.BattleWolf, TutorialType.ChangeFaction },
                Done = true
            };
            
            await _webService.SendWebRequestAsync<UpdateTutorialRequired>(
                "UserAccount/UpdateTutorial", UnityWebRequest.kHttpVerbPUT, packet);
            
            Managers.UI.CloseAllPopupUI();
        });
    }
    
    #endregion

    public void OnTypeStarted()
    {
        _typing = true;
    }

    public void OnTextShowed()
    {
        _typing = false;
    }

    private void OnDestroy()
    {
        _tutorialVm.ProcessTutorial = false;
        
        _tutorialVm.OnShowSpeakerAfter3Sec -= ShowSpeaker;
        _tutorialVm.OnShowNewSpeaker -= ShowNewSpeaker;
        _tutorialVm.OnChangeSpeaker -= ChangeSpeaker;
        _tutorialVm.OnShowFactionSelectPopup -= ShowFactionSelectPopup;
        _tutorialVm.OnChangeFaceCry -= ChangeFaceCry;
        _tutorialVm.OnChangeFaceHappy -= ChangeFaceHappy;
        _tutorialVm.OnChangeFaceNormal -= ChangeFaceNormal;

        if (_wolfRenderTexture != null)
        {
            _wolfRenderTexture.Release();
            Destroy(_wolfRenderTexture);
        }

        if (_sheepRenderTexture != null)
        {
            _sheepRenderTexture.Release();
            Destroy(_sheepRenderTexture);
        }

        if (_videoRenderTexture != null)
        {
            _sheepRenderTexture.Release();
            Destroy(_videoRenderTexture);
        }
    }
}
