using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Zenject;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class UI_BattlePopupWolf : UI_Popup
{
    private IUserService _userService;
    private IWebService _webService;
    private ITokenService _tokenService;
    
    private enum Buttons
    {
        ExitButton,
        RankGameButton,
        ExhibitionGameButton,
        TestButton
    }

    private enum Images
    {
        WolfIcon,
    }
    
    [Inject]
    public void Construct(IUserService userService, IWebService webService, ITokenService tokenService)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
    }

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
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
            Faction = (int)User.Instance.DeckSheep.Faction,
            DeckNumber = User.Instance.DeckSheep.DeckNumber,
        };
        
        var task = _webService.SendWebRequestAsync<GetSelectedDeckResponse>(
            "Collection/GetSelectedDeck", "POST", deckPacket);
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
            Faction = (int)User.Instance.DeckSheep.Faction,
            DeckNumber = User.Instance.DeckSheep.DeckNumber,
        };
        
        var task = _webService.SendWebRequestAsync<GetSelectedDeckResponse>(
            "Collection/GetSelectedDeck", "POST", deckPacket);
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

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
        GetButton((int)Buttons.RankGameButton).gameObject.BindEvent(OnRankGameClicked);
        GetButton((int)Buttons.ExhibitionGameButton).gameObject.BindEvent(OnExhibitionGameClicked);
        GetButton((int)Buttons.TestButton).gameObject.BindEvent(OnTestClicked);
    }

    protected override void InitUI()
    {
        SetObjectSize(GetImage((int)Images.WolfIcon).gameObject, 0.25f);
    }
}
