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
        base.Init();

        await _singlePlayVm.Initialize();
        BindObjects();
        InitButtonEvents();
        InitUI();
    }
    
    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        
        Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.BackButton).gameObject.BindEvent(OnBackClicked);
        GetButton((int)Buttons.InfoButton).gameObject.BindEvent(OnInfoClicked);
        GetButton((int)Buttons.StartButton).gameObject.BindEvent(OnStartClicked);
    }
    
    protected override void InitUI()
    {
        SetUserInfo();
        
        var highestStage = _singlePlayVm.UserStageInfos.Max(usi => usi.StageId) % 10;
        var singlePlayStageText = _textDict["SinglePlayStageText"].GetComponent<TextMeshProUGUI>(); 
        singlePlayStageText.text = $"{_singlePlayVm.StageLevel} - {highestStage}";
    }

    private void SetUserInfo()
    {
        var faction = Util.Faction;
        var deck = _deckVm.GetDeck(faction);
        var deckImage = GetImage((int)Images.Deck).transform;
        var userNameText = GetText((int)Texts.UserNameText);
        var rankPointText = GetText((int)Texts.RankPointText);
        
        userNameText.text = _userService.UserInfo.UserName;
        rankPointText.text = _userService.UserInfo.RankPoint.ToString();
        
        foreach (var unit in deck.UnitsOnDeck)
        {
            Managers.Resource.GetCardResources<UnitId>(unit, deckImage.transform);
        }
    }

    public void StartSinglePlay(int sessionId)
    {
        _ = _singlePlayVm.StartSinglePlay(sessionId);
    }
    
    private void OnBackClicked(PointerEventData data)
    {
        Managers.Scene.LoadScene(Define.Scene.MainLobby);
    }
    
    private void OnInfoClicked(PointerEventData data)
    {
        var popup = Managers.UI.ShowPopupUI<UI_SinglePlayMapPopup>();
        popup.Faction = Util.Faction;
    }
    
    private void OnStartClicked(PointerEventData data)
    {
        // Managers.Scene.LoadScene(Define.Scene.Game);
    }
}
