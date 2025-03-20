using System;
using System.Collections;
using System.Collections.Generic;
using AssetKits.ParticleImage;
using Febucci.UI;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using Zenject;

public class UI_TutorialMainPopup : UI_Popup
{
    private TutorialViewModel _tutorialVm;
    
    private TutorialNpcInfo _tutorialNpcInfo1;
    private TutorialNpcInfo _tutorialNpcInfo2;
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
    private Dictionary<string, VideoClip> _videoClips = new();

    private enum Images
    {
        ContinueButtonLine,
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
    public void Construct(TutorialViewModel tutorialViewModel)
    {
        _tutorialVm = tutorialViewModel;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        BindActions();
        InitButtonEvents();
        InitUI();
    }
    
    protected override void BindObjects()
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
        _buttonSelectEffect = Managers.Resource.Instantiate("UIEffects/ButtonSelectEffect", _selectPanel.transform);
        _videoPlayer = Util.FindChild(_selectPanel, "VideoPlayer", true).GetComponent<VideoPlayer>();
        _masking = Util.FindChild(_selectPanel, "TutorialMainVideoMasking", true);
        
        _videoClips.Add("Wolf", Resources.Load<VideoClip>("VideoClips/TutorialWolf"));
        _videoClips.Add("Sheep", Resources.Load<VideoClip>("VideoClips/TutorialSheep"));
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
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitButtonClicked);
        GetButton((int)Buttons.WolfButton).gameObject.BindEvent(OnWolfClicked);
        GetButton((int)Buttons.SheepButton).gameObject.BindEvent(OnSheepClicked);
        GetButton((int)Buttons.PlayButton).gameObject.BindEvent(OnPlayClicked);
    }
    
    protected override void InitUI()
    {
        var factionText = Util.FindChild(_selectPanel, "FactionText", true).GetComponent<TextMeshProUGUI>();
        factionText.text = Managers.Localization.GetLocalizedValue(factionText, "tutorial_main_faction_text");
        
        _speaker1Panel.SetActive(false);
        _speaker2Panel.SetActive(false);
        _continueButton.SetActive(false);
        _selectPanel.SetActive(false);
        _buttonSelectEffect.SetActive(false);
        
        var tutorialNpc1 = Managers.Resource.Instantiate("Npc/NpcWerewolf");
        var tutorialNpc2 = Managers.Resource.Instantiate("Npc/NpcFlower");
        _tutorialNpcInfo1 = tutorialNpc1.GetComponent<TutorialNpcInfo>();
        _tutorialNpcInfo2 = tutorialNpc2.GetComponent<TutorialNpcInfo>();
        _flowerFace = Util.FindChild(_tutorialNpcInfo2.gameObject, "+ Head", true);
        
        var npc1Pos = _tutorialNpcInfo1.Position;
        var npc2Pos = _tutorialNpcInfo2.Position;
        var camera1Pos = _tutorialNpcInfo1.CameraPosition;
        var camera2Pos = _tutorialNpcInfo2.CameraPosition;
        _tutorialVm.InitTutorialMain(npc1Pos, camera1Pos, npc2Pos, camera2Pos);
        StepTutorial();
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

    private void StepTutorial()
    {
        _tutorialVm.Step++;
        Managers.Data.TutorialDict.TryGetValue(TutorialType.Main, out var tutorialData);
        var step = tutorialData?.Steps.Find(s => s.Step == _tutorialVm.Step);
        if (step == null) return;
        foreach (var eventString in step.Events)
        {
            _tutorialVm.MainEventDict[eventString]?.Invoke();
        }
        
        var textContent = Managers.Localization.GetLocalizedValue(_speechBubbleText, step.DialogKey);
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
    
    private void ShowFactionSelectPopup()
    {
        _selectPanel.SetActive(true);
        _masking.SetActive(false);
        
        var playButtonText = Util.FindChild(_selectPanel, "PlayButtonText", true).GetComponent<TextMeshProUGUI>();
        var titleText = Util.FindChild(_selectPanel, "TutorialMainSelectText", true).GetComponent<TextMeshProUGUI>();
        playButtonText.text = Managers.Localization.GetLocalizedValue(playButtonText, "play_button_text");
        titleText.text = Managers.Localization.GetLocalizedValue(titleText, "tutorial_main_select_text");
    }

    private void ChangeFaceNormal()
    {
        Util.DestroyAllChildren(_flowerFace.transform);
        Managers.Resource.Instantiate("Npc/Blinking Eyes 01", _flowerFace.transform);
    }

    private void ChangeFaceHappy()
    {
        Util.DestroyAllChildren(_flowerFace.transform);
        Managers.Resource.Instantiate("Npc/Face Happy", _flowerFace.transform);
    }

    private void ChangeFaceCry()
    {
        Util.DestroyAllChildren(_flowerFace.transform);
        Managers.Resource.Instantiate("Npc/Face Cry", _flowerFace.transform);
    }

    private IEnumerator AlertRoutine()
    {
        var text = _tutorialMainSelectText;
        // var textAnimator = text.GetComponent<TextAnimator_TMP>();
        // var events = textAnimator.DefaultAppearancesTags;
        
        text.color = Color.red;
        yield return new WaitForSeconds(0.75f);
        text.color = Color.white;
    }
    
    private void OnContinueClicked(PointerEventData data)
    {
        StepTutorial();
    }

    private void OnWolfClicked(PointerEventData data)
    {
        if (_masking.activeSelf == false)
        {
            _masking.SetActive(true);
        }

        Util.Faction = Faction.Wolf;
        _tutorialVm.TutorialFaction = Faction.Wolf;
        _factionText.text = Managers.Localization.GetLocalizedValue(_factionText, "wolf_text");
        _factionInfoText.text = Managers.Localization.GetLocalizedValue(_factionInfoText, "faction_info_text_wolf");
        _buttonSelectEffect.SetActive(true);
        _buttonSelectEffect.transform.position = GetButton((int)Buttons.WolfButton).transform.position;
        _buttonSelectEffect.GetComponent<ParticleImage>().Play();
        _videoPlayer.source = VideoSource.VideoClip;
        _videoPlayer.clip = _videoClips["Wolf"];
        _videoPlayer.Play();
    }
    
    private void OnSheepClicked(PointerEventData data)
    {
        if (_masking.activeSelf == false)
        {
            _masking.SetActive(true);
        }
        
        Util.Faction = Faction.Sheep;
        _tutorialVm.TutorialFaction = Faction.Sheep;
        _factionText.text = Managers.Localization.GetLocalizedValue(_factionText, "sheep_text");
        _factionInfoText.text = Managers.Localization.GetLocalizedValue(_factionInfoText, "faction_info_text_sheep");
        _buttonSelectEffect.SetActive(true);
        _buttonSelectEffect.transform.position = GetButton((int)Buttons.SheepButton).transform.position;
        _buttonSelectEffect.GetComponent<ParticleImage>().Play();
        _videoPlayer.source = VideoSource.VideoClip;
        _videoPlayer.clip = _videoClips["Sheep"];
        _videoPlayer.Play();
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
    
    private void OnExitButtonClicked(PointerEventData data)
    {
        var popup = Managers.UI.ShowPopupUI<UI_NotifySelectPopup>();
        
        Managers.Localization.UpdateNotifySelectPopupText(popup,
            "notify_select_tutorial_exit_title", 
            "notify_select_tutorial_exit_message");
        popup.SetYesCallback(() =>
        {
            Managers.UI.CloseAllPopupUI();
        });
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
    }
}
