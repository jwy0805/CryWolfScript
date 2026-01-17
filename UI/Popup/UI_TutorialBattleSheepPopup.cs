using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Febucci.UI;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_TutorialBattleSheepPopup : UI_Popup
{
    private IUserService _userService;
    private TutorialViewModel _tutorialVm;

    private GameObject _tutorialNpc;
    private GameObject _flowerFace;
    private GameObject _dim;
    private GameObject _uiBlocker;
    private GameObject _continueButton;
    private GameObject _speakerPanel;
    private GameObject _infoBubble;
    private GameObject _hand;
    private RectTransform _handRect;
    private TextMeshProUGUI _speechBubbleText;
    private TextMeshProUGUI _infoBubbleText;
    private Coroutine _handPokeRoutine;
    private Coroutine _dragCoroutine;
    private Vector2 _portraitAnchor;
    private Vector2 _skillButtonAnchor;
    private Camera _tutorialCamera;
    private RawImage _rawImage;
    private RenderTexture _renderTexture;
    private bool _typing;
        
    // upkeep 팝업처럼 튜토리얼 순서에 상관없이 특정 상황에 뜨는 팝업 = true
    public bool IsInterrupted { get; set; } 
    public string InterruptTag { get; set; }
    
    public void OnTypeStarted() => _typing = true;
    public void OnTextShowed() => _typing = false;
    
    private enum Images
    {
        Dim,
        ContinueButtonLine,
        Blocker,
        Hand,
        BackgroundLeft
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
    public void Construct(IUserService userService, TutorialViewModel tutorialViewModel)
    {
        _userService = userService;
        _tutorialVm = tutorialViewModel;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();

            BindObjects();
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
    
    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons)); 
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _dim = GetImage((int)Images.Dim).gameObject;
        _uiBlocker = GetImage((int)Images.Blocker).gameObject;
        _continueButton = GetButton((int)Buttons.ContinueButton).gameObject;
        _hand = Util.FindChild(gameObject, "Hand", true, true);
        _handRect = _hand.GetComponent<RectTransform>();
        _speakerPanel = Util.FindChild(gameObject, "LeftPanel");
        _infoBubble = Util.FindChild(gameObject, "InfoBubble");
        _speechBubbleText = Util.FindChild(_speakerPanel, "SpeechBubbleText", true).GetComponent<TextMeshProUGUI>();
        _infoBubbleText = Util.FindChild(_infoBubble, "InfoBubbleText", true).GetComponent<TextMeshProUGUI>();
        _skillButtonAnchor = new Vector2(0.95f, 0.12f);
    }
    
    private void BindActions()
    {        
        _tutorialVm.OnRunTutorialTag += RunTutorialTag;
        _tutorialVm.OnUiBlocker += OnUiBlocker;
        _tutorialVm.OffUiBlocker += OffUiBlocker;
        _tutorialVm.OnUiBlocker05Sec += OnUiBlocker05Sec;
        _tutorialVm.OnUiBlocker1Sec += OnUiBlocker1Sec;
        _tutorialVm.OnUiBlocker2Sec += OnUiBlocker2Sec;
        _tutorialVm.OnUiBlocker3Sec += OnUiBlocker3Sec;
        _tutorialVm.OnHandImage += OnHandImage;
        _tutorialVm.OffHandImage += OffHandImage;
        _tutorialVm.OnContinueButton += OnContinueButton;
        _tutorialVm.OffContinueButton += OffContinueButton;
        _tutorialVm.OnShowSpeaker += ShowSpeaker;
        _tutorialVm.OnShowSpeakerAfter3Sec += ShowSpeakerAfter3SecHandler;
        _tutorialVm.PointToTimePanel += PointToTimePanelHandler;
        _tutorialVm.PointToResourcePanel += PointToResourcePanelHandler;
        _tutorialVm.PointToCapacityPanel += PointToCapacityPanelHandler;
        _tutorialVm.PointToLog += PointToLogHandler;
        _tutorialVm.PointToUpgradeButton += PointToUpgradeButtonHandler;
        _tutorialVm.DragTankerUnit += DragTankerUnitHandler;
        _tutorialVm.DragRangerUnit += DragRangerUnitHandler;
        _tutorialVm.DragScene += DragSceneHandler;
        _tutorialVm.ShowSimpleTooltip += ShowSimpleTooltip;
        _tutorialVm.PointToSkillButtonAndPortrait += PointToSkillButtonAndPortraitHandler;
        _tutorialVm.AdjustUiBlockerSize += AdjustUiBlockerSize;
        _tutorialVm.ClearScene += ClearScene;
        _tutorialVm.ResumeGame += ResumeGame;
        _tutorialVm.OnChangeFaceHappy += ChangeFaceHappy;
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ContinueButton).gameObject.BindEvent(OnContinueClicked);
    }

    protected override async Task InitUIAsync()
    {
        _tutorialNpc = GameObject.FindGameObjectWithTag("Npc");
        if (_tutorialNpc == null)
        {
            _tutorialNpc = await Managers.Resource.Instantiate("Npc/NpcFlower");
            _flowerFace = Util.FindChild(_tutorialNpc, "+ Head", true);
        }

        var npcInfo = _tutorialNpc.GetComponent<TutorialNpcInfo>();
        
        var npcPos = npcInfo.Position;
        var cameraPos = npcInfo.CameraPosition;
        _tutorialVm.InitTutorialBattleSheep(npcPos, cameraPos);
        ClearScene();
        
        // 첫 튜토리얼 태그 실행 or popup이 꺼졌다가 이어서 실행
        var tutorialTag = string.IsNullOrEmpty(_tutorialVm.CurrentTag)
            ? "BattleSheep.InfoRound"
            : _tutorialVm.NextTag;        
        await RunTutorialTag(IsInterrupted ? InterruptTag : tutorialTag);
        StartCoroutine(SmoothAlphaRoutine());
    }

    private void InitCamera()
    {
        _tutorialCamera = GameObject.FindGameObjectsWithTag("Camera")
            .FirstOrDefault(obj => obj.name == "TutorialCamera")?.GetComponent<Camera>();

        if (_tutorialCamera == null)
        {
            Debug.LogWarning("TutorialCamera not found!");
            return;
        }
        
        _rawImage = GetImage((int)Images.BackgroundLeft).GetComponentInChildren<RawImage>();
        _renderTexture = Managers.Resource.CreateRenderTexture("TutorialRenderTexture");

        if (_rawImage == null)
        {
            Debug.LogWarning("RawImage not found in BackgroundLeft!");
            return;
        }
        
        _rawImage.texture = _renderTexture;
        _tutorialCamera.targetTexture = _renderTexture;
    }
    
    #region UI Effects
    
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

    // The offset for moving the finger from its original position (in pixels on the UI).
    // For example, setting (30, -30) will move it diagonally towards the bottom-right.
    private IEnumerator HandPokeRoutine(Vector2 offset, float moveDuration, float holdTime, float waitTime, int repeat = 3)
    {
        Vector2 originalPos = _handRect.anchoredPosition;
        Vector2 targetPos = originalPos + offset;
        
        for (int i = 0; i < repeat; i++)
        {
            // 목표 위치로 부드럽게 이동
            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                _handRect.anchoredPosition = Vector2.Lerp(originalPos, targetPos, t);
                yield return null;
            }

            // 목표 위치에서 잠시 대기
            yield return new WaitForSeconds(holdTime);

            // 원래 위치로 부드럽게 복귀
            elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                _handRect.anchoredPosition = Vector2.Lerp(targetPos, originalPos, t);
                yield return null;
            }

            // 다음 동작 전에 대기
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator SmoothMoveRectAnchor(RectTransform rect, Vector2 target, float duration)
    {
        Vector2 originalMin = rect.anchorMin;
        Vector2 originalMax = rect.anchorMax;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.anchorMin = Vector2.Lerp(originalMin, target, t);
            rect.anchorMax = Vector2.Lerp(originalMax, target, t);
            yield return null;
        }
        
        rect.anchorMin = target;
        rect.anchorMax = target;
    }

    private IEnumerator DragRectAnchor(RectTransform rect, Vector2 target, float restoreDelay, float moveDuration)
    {
        Vector2 originMin = rect.anchorMin;
        Vector2 originMax = rect.anchorMax;

        for (int i = 0; i < 3; i++)
        {
            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                rect.anchorMin = Vector2.Lerp(originMin, target, t);
                rect.anchorMax = Vector2.Lerp(originMax, target, t);
                yield return null;
            }
        
            rect.anchorMin = target;
            rect.anchorMax = target;
        
            yield return new WaitForSeconds(restoreDelay);
        
            rect.anchorMin = originMin;
            rect.anchorMax = originMax;
        }
    }
    
    #endregion

    private async Task RunTutorialTag(string tutorialTag)
    {
        if (!Managers.Data.TutorialDict.TryGetValue(TutorialType.BattleSheep, out var tutorialData)) return;
        if (!tutorialData.StepsDict.TryGetValue(tutorialTag, out var step)) return;
        if (IsInterrupted == false)
        {
            _tutorialVm.CurrentTag = tutorialTag;
            _tutorialVm.NextTag = step.Next;   
        }
        
        foreach (var actionKey in step.Actions)
        {
            _tutorialVm.ActionDict[actionKey]?.Invoke();
        }

        switch (step.Speaker)
        {
            case "Flower":
            {
                var textContent = await Managers.Localization.BindLocalizedText(_speechBubbleText, step.DialogKey);
                _speechBubbleText.text = textContent;
                break;
            }
            case "Echo":
            {
                var textContent = await Managers.Localization.BindLocalizedText(_infoBubbleText, step.DialogKey);
                _infoBubbleText.text = textContent;
                break;
            }
        }
    }
    
    private void OnUiBlocker()
    {
        _uiBlocker.SetActive(true);
    }

    private void OffUiBlocker()
    {
        _uiBlocker.SetActive(false);
    }

    private void OnUiBlocker05Sec()
    {
        StartCoroutine(nameof(OnUiBlockerTimerRoutine), 0.5f);
    }
    
    private void OnUiBlocker1Sec()
    {
        StartCoroutine(nameof(OnUiBlockerTimerRoutine), 1f);
    }

    private void OnUiBlocker2Sec()
    {
        StartCoroutine(nameof(OnUiBlockerTimerRoutine), 2f);
    }
    
    private void OnUiBlocker3Sec()
    {
        StartCoroutine(nameof(OnUiBlockerTimerRoutine), 3f);
    }
    
    private IEnumerator OnUiBlockerTimerRoutine(float duration = 2f)
    {
        _uiBlocker.SetActive(true);
        yield return new WaitForSeconds(duration);
        _uiBlocker.SetActive(false);
    }
    
    private void OnHandImage()
    {
        _hand.SetActive(true);
    }
    
    private void OffHandImage()
    {
        _hand.SetActive(false);
    }

    private void OffContinueButton()
    {
        _continueButton.SetActive(false);
    }
    
    private void OnContinueButton()
    {
        _continueButton.SetActive(true);
    }
    
    private void ShowSpeakerAfter3SecHandler()
    {
        StartCoroutine(nameof(ShowSpeakerAfter3Sec));
    }
    
    private IEnumerator ShowSpeakerAfter3Sec()
    {
        yield return new WaitForSeconds(3f);
        _tutorialVm.SendHoldPacket(true);
        ShowSpeakerPanel();
    }

    private void ShowSpeaker()
    {
        _tutorialVm.SendHoldPacket(true);
        ShowSpeakerPanel();
    }
    
    private void ShowSpeakerPanel()
    {
        _dim.SetActive(true);
        _speakerPanel.SetActive(true);
        _continueButton.SetActive(true);
    }

    private void PointToTimePanelHandler()
    {
        StartCoroutine(nameof(PointToTimePanel));
    }
    
    private IEnumerator PointToTimePanel()
    {
        yield return new WaitForSeconds(1f);
        _hand.SetActive(true);
        var anchor = new Vector2(0.63f, 0.84f);
        yield return StartCoroutine(SmoothMoveRectAnchor(_handRect, anchor, 0.5f));
        
        var offset = new Vector2(-60, 60);
        _handPokeRoutine = StartCoroutine(HandPokeRoutine(offset, 0.5f, 0.1f, 0.2f));
    }
    
        private void PointToResourcePanelHandler()
    {
        StartCoroutine(nameof(PointToResourcePanel));
    }
    
    private IEnumerator PointToResourcePanel()
    {
        var anchor = new Vector2(0.84f, 0.84f);
        yield return StartCoroutine(HandMoveAndPokeRoutine(anchor));
    }

    private void PointToCapacityPanelHandler()
    {
        StartCoroutine(nameof(PointToCapacityPanel));
    }
    
    private IEnumerator PointToCapacityPanel()
    {
        var anchor = new Vector2(1.04f, 0.84f);
        yield return StartCoroutine(HandMoveAndPokeRoutine(anchor));
    }

    private void PointToLogHandler()
    {
        StartCoroutine(nameof(PointToLog));
    }
    
    private IEnumerator PointToLog()
    {
        var anchor = new Vector2(0.65f, -0.02f);  
        yield return StartCoroutine(HandMoveAndPokeRoutine(anchor, 1.5f));
    }

    private void PointToUpgradeButtonHandler()
    {
        StartCoroutine(nameof(PointToUpgradeButton));
    }

    private IEnumerator PointToUpgradeButton()
    {
        var anchor = new Vector2(0.28f, 0.12f);
        yield return StartCoroutine(HandMoveAndPokeRoutine(anchor));
    }
    
    private IEnumerator HandMoveAndPokeRoutine(Vector2 anchor, float duration = 1f)
    {
        if (_handPokeRoutine != null)
        {
            StopCoroutine(_handPokeRoutine);
            _handPokeRoutine = null;
        }
        
        yield return StartCoroutine(SmoothMoveRectAnchor(_handRect, anchor, duration));
        
        var offset = new Vector2(-60, 60);
        _handPokeRoutine = StartCoroutine(HandPokeRoutine(offset, 0.5f, 0.1f, 0.2f));
    }
    
    private void DragTankerUnitHandler()
    {
        _uiBlocker.SetActive(true);
        _uiBlocker.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.1f);
        StartCoroutine(nameof(DragTankerUnit));
    }

    private IEnumerator DragTankerUnit()
    {
        var portraitIndex = _tutorialVm.GetTankerAnchorIndex();
        var targetVector = new Vector2(0.6f, 0.45f);
        
        _portraitAnchor = new Vector2(GetAnchor(portraitIndex), 0.05f);
        _handRect.anchorMin = _portraitAnchor;
        _handRect.anchorMax = _portraitAnchor;
        
        yield return StartCoroutine(DragRectAnchor(_handRect, targetVector, 0.5f, 1f));
        OffHandImage();
        
        _uiBlocker.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        _uiBlocker.SetActive(false);
    }
    
    private void DragRangerUnitHandler()
    {
        StartCoroutine(nameof(DragRangerUnit));
    }

    private IEnumerator DragRangerUnit()
    {
        var portraitIndex = _tutorialVm.GetRangerAnchorIndex();
        var targetVector = new Vector2(0.6f, 0.4f);
        
        _portraitAnchor = new Vector2(GetAnchor(portraitIndex), 0.05f);
        _handRect.anchorMin = _portraitAnchor;
        _handRect.anchorMax = _portraitAnchor;
        
        yield return StartCoroutine(DragRectAnchor(_handRect, targetVector, 0.5f, 1f));
        OffHandImage();
        
        _uiBlocker.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        _uiBlocker.SetActive(false);
    }
    
    private void DragSceneHandler()
    {
        StartCoroutine(nameof(DragScene));
    }

    private IEnumerator DragScene()
    {
        var highRect = new Vector2(0.6f, 0.65f);
        var lowRect = new Vector2(0.6f, 0.35f);
        
        yield return StartCoroutine(SmoothMoveRectAnchor(_handRect, highRect, 0.5f));
        yield return StartCoroutine(SmoothMoveRectAnchor(_handRect, lowRect, 0.5f));
        yield return StartCoroutine(SmoothMoveRectAnchor(_handRect, highRect, 0.5f));
        yield return StartCoroutine(SmoothMoveRectAnchor(_handRect, lowRect, 0.5f));
    }
    
    private void PointToSkillButtonAndPortraitHandler()
    {
        StartCoroutine(nameof(PointToSkillButtonAndPortrait));
    }

    private IEnumerator PointToSkillButtonAndPortrait()
    {
        _handRect.anchorMin = new Vector2(_skillButtonAnchor.x - 0.02f, _skillButtonAnchor.y);
        _handRect.anchorMax = new Vector2(_skillButtonAnchor.x - 0.02f, _skillButtonAnchor.y);
        
        if (_handPokeRoutine != null)
        {
            StopCoroutine(_handPokeRoutine);
        }
        
        var offset = new Vector2(-60, 60);
        var portraitIndex = _tutorialVm.GetTankerAnchorIndex();
        
        _portraitAnchor = new Vector2(GetAnchor(portraitIndex), 0.0f);
        
        yield return StartCoroutine(HandPokeRoutine(offset, 0.5f, 0.1f, 0.2f, 2));
        yield return StartCoroutine(SmoothMoveRectAnchor(_handRect, _portraitAnchor, 0.5f));
        yield return StartCoroutine(HandPokeRoutine(offset, 0.5f, 0.1f, 0.2f, 2));
    }
    
    private float GetAnchor(int index)
    {
        return index switch
        {
            0 => 0.17f,
            1 => 0.34f,
            2 => 0.51f,
            3 => 0.68f,
            4 => 0.85f,
            5 => 1.02f,
            _ => 2f
        };
    }
    
    private void ShowSimpleTooltip()
    {
        _infoBubble.SetActive(true);
        StartCoroutine(TempBlocking());
    }
    
    private void AdjustUiBlockerSize()
    {
        var dimRect = _dim.GetComponent<RectTransform>();
        dimRect.anchorMin = new Vector2(0, 0.3f);
        dimRect.offsetMin = new Vector2(dimRect.offsetMin.x, 0f);
        dimRect.offsetMax = new Vector2(dimRect.offsetMax.x, 0f);
        
        var panel = Util.FindChild(gameObject, "LeftPanel", true, true);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.3f);
        panelRect.anchorMax = new Vector2(panelRect.offsetMin.x, 1f);
        panelRect.offsetMin = new Vector2(panelRect.offsetMax.x, 0f);
    }
    
    private void ChangeFaceHappy()
    {
        _tutorialNpc = GameObject.FindGameObjectWithTag("Npc");
        _flowerFace = Util.FindChild(_tutorialNpc, "+ Head", true);
        Util.DestroyAllChildren(_flowerFace.transform);
        _ = Managers.Resource.Instantiate("Npc/Face Happy", _flowerFace.transform);
    }
    
    private void ClearScene()
    {
        _hand.SetActive(false);
        _speakerPanel.SetActive(false);
        _infoBubble.SetActive(false);
        _dim.SetActive(false);
        _uiBlocker.SetActive(false);
        _continueButton.SetActive(false);
    }

    private void ResumeGame()
    {
        _tutorialVm.SendHoldPacket(false);
    }
    
    private async Task OnContinueClicked(PointerEventData data)
    {
        if (_typing)
        {
            _speechBubbleText.GetComponent<TypewriterByCharacter>().SkipTypewriter();
            _typing = false;
        }
        else
        {
            if (_tutorialVm.NextTag == "BattleSheep.End")
            {
                await _tutorialVm.BattleTutorialEndHandler(Faction.Sheep);
                return;
            }
            
            if (IsInterrupted)
            {
                if (!Managers.Data.TutorialDict.TryGetValue(TutorialType.BattleSheep, out var tutorialData)) return;
                if (!tutorialData.StepsDict.TryGetValue(InterruptTag, out var step)) return;
                if (step.Next.Contains("Close"))
                {
                    ResumeGame();
                    ClosePopup();
                    return;
                }
            }
            
            if (_tutorialVm.NextTag.Contains("Close") ||
                _tutorialVm.NextTag == "BattleSheep.InfoResource")
            {
                ClosePopup();
                return;
            }
        
            await RunTutorialTag(_tutorialVm.NextTag);
        }
    }

    private IEnumerator TempBlocking()
    {
        _uiBlocker.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        _uiBlocker.SetActive(false);
    }
    
    private void ClosePopup()
    {
        Managers.UI.ClosePopupUI();
    } 
    
    private void OnDestroy()
    {
        _tutorialVm.OnRunTutorialTag -= RunTutorialTag;
        _tutorialVm.OnUiBlocker -= OnUiBlocker;
        _tutorialVm.OffUiBlocker -= OffUiBlocker;
        _tutorialVm.OnUiBlocker05Sec -= OnUiBlocker05Sec;
        _tutorialVm.OnUiBlocker1Sec -= OnUiBlocker1Sec;
        _tutorialVm.OnUiBlocker2Sec -= OnUiBlocker2Sec;
        _tutorialVm.OnUiBlocker3Sec -= OnUiBlocker3Sec;
        _tutorialVm.OnHandImage -= OnHandImage;
        _tutorialVm.OffHandImage -= OffHandImage;
        _tutorialVm.OnContinueButton -= OnContinueButton;
        _tutorialVm.OffContinueButton -= OffContinueButton;
        _tutorialVm.OnShowSpeaker -= ShowSpeaker;
        _tutorialVm.OnShowSpeakerAfter3Sec -= ShowSpeakerAfter3SecHandler;
        _tutorialVm.PointToTimePanel -= PointToTimePanelHandler;
        _tutorialVm.PointToResourcePanel -= PointToResourcePanelHandler;
        _tutorialVm.PointToCapacityPanel -= PointToCapacityPanelHandler;
        _tutorialVm.PointToLog -= PointToLogHandler;
        _tutorialVm.PointToUpgradeButton -= PointToUpgradeButtonHandler;
        _tutorialVm.DragTankerUnit -= DragTankerUnitHandler;
        _tutorialVm.DragRangerUnit -= DragRangerUnitHandler;
        _tutorialVm.DragScene -= DragSceneHandler;
        _tutorialVm.ShowSimpleTooltip -= ShowSimpleTooltip;
        _tutorialVm.PointToSkillButtonAndPortrait -= PointToSkillButtonAndPortraitHandler;
        _tutorialVm.AdjustUiBlockerSize -= AdjustUiBlockerSize;
        _tutorialVm.ClearScene -= ClearScene;
        _tutorialVm.ResumeGame -= ResumeGame;
        _tutorialVm.OnChangeFaceHappy -= ChangeFaceHappy;
        
        _tutorialVm = null;
        _dim = null;
    }
}
