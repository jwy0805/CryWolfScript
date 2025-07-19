using System;
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
    public void Construct(SinglePlayViewModel singlePlayVm, DeckViewModel deckVm)
    {
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
    }

    private async Task SetUserInfo()
    {
        var faction = Util.Faction;
        var deck = _deckVm.GetDeck(faction);
        var deckImage = GetImage((int)Images.Deck).transform;
        var userNameText = GetText((int)Texts.UserNameText);
        var rankPointText = GetText((int)Texts.RankPointText);

        userNameText.text = User.Instance.UserInfo.UserName;
        rankPointText.text = User.Instance.UserInfo.RankPoint.ToString();
        await Managers.Localization.UpdateFont(userNameText);
        
        foreach (var unit in deck.UnitsOnDeck)
        {
            await Managers.Resource.GetCardResources<UnitId>(unit, deckImage.transform);
        }
    }
    
    public void StartSinglePlay(int sessionId)
    {
        _ = _singlePlayVm.StartSinglePlay(sessionId);
        Debug.Log("start single play");
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
        await _singlePlayVm.ConnectGameSession();
    }
}
