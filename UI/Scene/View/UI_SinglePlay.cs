using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_SinglePlay : UI_Scene
{
    private IUserService _userService;
    private SinglePlayViewModel _singlePlayVm;
    private DeckViewModel _deckVm;

    private readonly Dictionary<string, GameObject> _textDict = new();

    private Coroutine _infoButtonHintRoutine;

    private enum Buttons
    {
        BackButton,
        InfoButton,
        StartButton
    }
    
    private enum Images
    {
        BackgroundColor,
        BackgroundImage,
        BackgroundPanelGlow,
        BackgroundGlow,
        BackgroundBottomImage,
        BackgroundStageImage,
        
        Deck,
    }

    private enum Texts
    {
        SinglePlayInfoText,
        SinglePlayStartButtonText,
        SinglePlayTitleText,
        SinglePlayStageText,
        
        UserNameText,
        RankPointText,
    }
    
    [Inject]
    public void Construct(IUserService userService, SinglePlayViewModel singlePlayVm, DeckViewModel deckVm)
    {
        _userService = userService;
        _singlePlayVm = singlePlayVm;
        _deckVm = deckVm;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();

            await _singlePlayVm.Initialize();
            BindObjects();
            InitButtonEvents();
            await InitUIAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    
    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        
        _ = Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.BackButton).onClick.AddListener(OnBackClicked);
        GetButton((int)Buttons.InfoButton).gameObject.BindEvent(OnInfoClicked);
        GetButton((int)Buttons.StartButton).gameObject.BindEvent(OnStartClicked);
    }
    
    protected override async Task InitUIAsync()
    {
        await SetUserInfo();
        
        var singlePlayStageText = _textDict["SinglePlayStageText"].GetComponent<TextMeshProUGUI>(); 
        singlePlayStageText.text = $"{_singlePlayVm.StageLevel} - {_singlePlayVm.StageId % 1000}";
        
        StartInfoButtonHint();
    }

    private async Task SetUserInfo()
    {
        var faction = Util.Faction;
        var deck = _deckVm.GetDeck(faction);
        var deckImage = GetImage((int)Images.Deck).transform;
        var userNameText = GetText((int)Texts.UserNameText);
        var rankPointText = GetText((int)Texts.RankPointText);

        userNameText.text = _userService.User.UserInfo.UserName;
        rankPointText.text = _userService.User.UserInfo.RankPoint.ToString();
        await Managers.Localization.UpdateFont(userNameText);
        
        foreach (var unit in deck.UnitsOnDeck)
        {
            await Managers.Resource.GetCardResources<UnitId>(unit, deckImage.transform);
        }
    }

    private void StartInfoButtonHint()
    {
        if (_infoButtonHintRoutine != null) StopCoroutine(_infoButtonHintRoutine);
        _infoButtonHintRoutine = StartCoroutine(InfoButtonHintRoutine());
    }

    private IEnumerator InfoButtonHintRoutine()
    {
        var infoButton = GetButton((int)Buttons.InfoButton);
        var rect = infoButton.GetComponent<RectTransform>();
        var originalPos = rect.anchoredPosition;
        var originalRot = rect.localEulerAngles;
        
        const int repeatCount = 3;              // 반복
        const float firstDelay = 1.0f;          // 씬 초기화 후 첫 딜레이
        const float betweenDelay = 1.2f;        // 반복 사이 딜레이
        const float dropDist = 25f;             // 아래로 떨어지는 거리
        const float dropTime = 0.15f;           // 떨어지는 시간
        const float bounceTime = 0.1f;          // 다시 올라오는 시간
        const float shakeTime = 0.35f;          // 좌우 흔들리는 총 시간
        const float maxShakeAngle = 15f;        // 최대 흔들림 각도
        const int shakeWaves = 5;               // 몇 번 왔다갔다

        yield return new WaitForSeconds(firstDelay);

        for (int i = 0; i < repeatCount; i++)
        {
            // 툭 떨어짐
            var startPos = originalPos;
            var endPos = originalPos + Vector2.down * dropDist;
            float t = 0;

            while (t < dropTime)
            {
                t += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(t / dropTime);
                // 가속 ease-in
                float ease = normalized * normalized;
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);
                yield return null;
            }
            
            // 튕겨 올라감
            t = 0;
            while (t < bounceTime)
            {
                t += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(t / bounceTime);
                // 감속 ease-out
                float ease = 1 - (1 - normalized) * (1 - normalized);
                rect.anchoredPosition = Vector2.Lerp(endPos, originalPos, ease);
                yield return null;
            }
            
            rect.anchoredPosition = originalPos;
            
            if (i < repeatCount - 1)
            {
                yield return new WaitForSeconds(betweenDelay);
            }
        }
    }
    
    public void StartSinglePlay(int sessionId)
    {
        _ = _singlePlayVm.StartSinglePlay(sessionId);
    }
    
    private void OnBackClicked()
    {
        Managers.Scene.LoadScene(Define.Scene.MainLobby);
    }
    
    private async Task OnInfoClicked(PointerEventData data)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_SinglePlayMapPopup>();
        popup.Faction = Util.Faction;
    }
    
    private async Task OnStartClicked(PointerEventData data)
    {
        await Managers.Network.ConnectGameSession();
    }
}
