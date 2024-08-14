using System;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class UI_BattlePopupSheep : UI_Popup
{
    private IUserService _userService;
    private ITokenService _tokenService;
    
    enum Buttons
    {
        ExitButton,
        RankGameButton,
        ExhibitionGameButton,
        TestButton,
    }

    enum Images
    {
        SheepIcon,
    }
    
    [Inject]
    public void Construct(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        SetButtonEvents();
        SetUI();
    }

    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }

    private async void OnRankGameClicked(PointerEventData data)
    {
        var deckPacket = new GetSelectedDeckRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Camp = (int)User.Instance.DeckSheep.Camp,
            DeckNumber = User.Instance.DeckSheep.DeckNumber,
        };
        
        var task = Managers.Web.SendPostRequestAsync<GetSelectedDeckResponse>("Collection/GetSelectedDeck", deckPacket);
        var response = await task;

        if (response.GetSelectedDeckOk == false)
        {
            Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        }
        else
        {
            _userService.SaveDeck(response.Deck);
            
            Managers.Map.MapId = 1;
            Managers.Scene.LoadScene(Define.Scene.MatchMaking);
            Managers.Clear();
        }
    }

    private async void OnExhibitionGameClicked(PointerEventData data)
    {
        var deckPacket = new GetSelectedDeckRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Camp = (int)User.Instance.DeckSheep.Camp,
            DeckNumber = User.Instance.DeckSheep.DeckNumber,
        };
        
        var task = Managers.Web.SendPostRequestAsync<GetSelectedDeckResponse>("Collection/GetSelectedDeck", deckPacket);
        var response = await task;

        if (response.GetSelectedDeckOk == false)
        {
            Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        }
        else
        {
            _userService.SaveDeck(response.Deck);
            
            Managers.Map.MapId = 2;
            Managers.Scene.LoadScene(Define.Scene.MatchMaking);
            Managers.Clear();
        }
    }
    
    private void OnTestClicked(PointerEventData data)
    {
        Managers.Map.MapId = 1;
        Managers.Scene.LoadScene(Define.Scene.Game);    
        Managers.Network.ConnectGameSession(true);
    }

    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
    }

    protected override void SetButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
        GetButton((int)Buttons.RankGameButton).gameObject.BindEvent(OnRankGameClicked);
        GetButton((int)Buttons.ExhibitionGameButton).gameObject.BindEvent(OnExhibitionGameClicked);
        GetButton((int)Buttons.TestButton).gameObject.BindEvent(OnTestClicked);
    }

    protected override void SetUI()
    {
        SetObjectSize(GetImage((int)Images.SheepIcon).gameObject, 0.25f);
    }
}
