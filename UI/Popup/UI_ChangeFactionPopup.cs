using System;
using System.Collections;
using System.Threading.Tasks;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_ChangeFactionPopup : UI_Popup
{
    private IUIFactory _uiFactory;
    private TutorialViewModel _tutorialVm;

    private Camera _tutorialCamera;
    private RawImage _rawImage;
    private RenderTexture _renderTexture;
    private GameObject _tutorialNpc;
    private GameObject _continueButton;
    private GameObject _hand;
    private GameObject _speakerPanel;
    private RectTransform _handRect;
    private TextMeshProUGUI _speechBubbleText;
    private bool _typing;

    private enum Images
    {
        ContinueButtonLine,
        LeftPanel,
        Hand,
    }

    private enum Buttons
    {
        ContinueButton,
        ExitButton,
    }

    private enum Texts
    {
        ContinueButtonText
    }

    [Inject]
    public void Construct(IUIFactory uiFactory, TutorialViewModel tutorialViewModel)
    {
        _uiFactory = uiFactory;
        _tutorialVm = tutorialViewModel;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            BindObjects();
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
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _hand = GetImage((int)Images.Hand).gameObject;
        _handRect = _hand.GetComponent<RectTransform>();
        _speakerPanel = GetImage((int)Images.LeftPanel).gameObject;
        _speechBubbleText = Util.FindChild(_speakerPanel, "SpeechBubbleText", true).GetComponent<TextMeshProUGUI>();
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ContinueButton).gameObject.BindEvent(OnContinueClicked);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
    }

    protected override async Task InitUIAsync()
    {
        _tutorialNpc = GameObject.FindGameObjectWithTag("Npc");
        if (_tutorialNpc == null)
        {
            _tutorialNpc = await Managers.Resource.Instantiate("Npc/NpcWerewolf");
        }
        
        var npcInfo = _tutorialNpc.GetComponent<TutorialNpcInfo>();
        var npcPos = npcInfo.Position;
        var cameraPos = npcInfo.CameraPosition;
        
        _tutorialVm.InitTutorialChangeFaction(npcPos, cameraPos);
        
        StartCoroutine(PointToFactionButton());
        
        const string key = "tutorial_change_faction_popup_text";
        var textContent = await Managers.Localization.BindLocalizedText(_speechBubbleText, key);
        _speechBubbleText.text = textContent;
    }

    private void InitCamera()
    {
        var leftPanel = GetImage((int)Images.LeftPanel);
        
        _rawImage = leftPanel.GetComponentInChildren<RawImage>();
        _renderTexture = _uiFactory.CreateRenderTexture("texture");
        _tutorialCamera = GameObject.Find("TutorialCamera1").GetComponent<Camera>();

        if (_tutorialCamera == null)
        {
            Debug.LogError("Tutorial Camera not found in the scene.");
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
    
    #endregion

    private IEnumerator PointToFactionButton()
    {
        var anchor = new Vector2(0.23f, 0.88f);
        var offset = new Vector2(-60, 60);
        
        _handRect.anchorMin = anchor;
        _handRect.anchorMax = anchor;
        
        yield return StartCoroutine(HandPokeRoutine(offset, 0.5f, 0.1f, 0.2f));
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
            _tutorialVm.ChangeFactionTutorialEndHandler();
            Managers.UI.CloseAllPopupUI();
        }
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
    
    public void OnTypeStarted()
    {
        _typing = true;
    }

    public void OnTextShowed()
    {
        _typing = false;
    }
}
