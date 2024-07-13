using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class UI_BattlePopupWolf : UI_Popup
{
    private enum Buttons
    {
        ExitButton,
        RankGameButton,
        ExhibitionGameButton,
    }

    private enum Images
    {
        WolfIcon,
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
            AccessToken = Managers.User.AccessToken,
            Camp = (int)Managers.User.DeckSheep.Camp,
            DeckNumber = Managers.User.DeckSheep.DeckNumber,
        };
        var task = Managers.Web.SendPostRequestAsync<GetSelectedDeckResponse>("Collection/GetSelectedDeck", deckPacket);
        var response = await task;

        if (response.GetSelectedDeckOk == false)
        {
            Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        }
        else
        {
            Managers.User.SaveDeck(response.Deck);
            Managers.Map.MapId = 2;
            Managers.Scene.LoadScene(Define.Scene.MatchMaking);
            Managers.Clear();
        }
    }

    private async void OnExhibitionGameClicked(PointerEventData data)
    {
        var deckPacket = new GetSelectedDeckRequired
        {
            AccessToken = Managers.User.AccessToken,
            Camp = (int)Managers.User.DeckSheep.Camp,
            DeckNumber = Managers.User.DeckSheep.DeckNumber,
        };
        var task = Managers.Web.SendPostRequestAsync<GetSelectedDeckResponse>("Collection/GetSelectedDeck", deckPacket);
        var response = await task;

        if (response.GetSelectedDeckOk == false)
        {
            Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        }
        else
        {
            Managers.User.SaveDeck(response.Deck);
            Managers.Map.MapId = 1;
            Managers.Scene.LoadScene(Define.Scene.MatchMaking);
            Managers.Clear();
        }
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
    }

    protected override void SetUI()
    {
        SetObjectSize(GetImage((int)Images.WolfIcon).gameObject, 0.25f);
    }
}
