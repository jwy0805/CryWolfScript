using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_BattlePopupSheep : UI_Popup
{
    enum Buttons
    {
        ExitButton,
        RankGameButton,
        ExhibitionGameButton,
    }

    enum Images
    {
        SheepIcon,
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

    private void OnRankGameClicked(PointerEventData data)
    {
        Managers.Map.MapId = 1;
        Managers.Scene.LoadScene(Define.Scene.Game);
        Managers.Clear();
    }

    private void OnExhibitionGameClicked(PointerEventData data)
    {
        Managers.Map.MapId = 2;
        Managers.Scene.LoadScene(Define.Scene.Game);
        Managers.Clear();
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
        SetObjectSize(GetImage((int)Images.SheepIcon).gameObject, 0.25f);
    }
}
