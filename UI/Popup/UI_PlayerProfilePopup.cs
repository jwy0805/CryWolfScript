using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PlayerProfilePopup : UI_Popup
{
    private Slider _expSlider;
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public UserInfo PlayerUserInfo { get; set; }
    
    private enum Images
    {
        SliderBackground,
        Flag
    }

    private enum Buttons
    {
        ExitButton,
    }

    private enum Texts
    {
        PlayerProfileTitleText,
        PlayerProfileRankingTitleText,
        PlayerProfileVictoriesTitleText,
        PlayerProfileWinRateTitleText,
        
        UsernameText,
        RankPointText,
        LevelText,
        ExpText,
        
        HighestRankText,
        RankingText,
        VictoriesText,
        WinRateText,
    }

    protected override void Init()
    {
        base.Init();
        
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
        Managers.Localization.UpdateFont(_textDict["UsernameText"]);
        
        var background = GetImage((int)Images.SliderBackground);
        _expSlider = background.transform.parent.GetComponent<Slider>();
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
    }

    protected override void InitUI()
    {
        var exp = PlayerUserInfo.Exp;
        var expMax = PlayerUserInfo.ExpToLevelUp;
        _expSlider.value = exp / (float)expMax;
        
        GetText((int)Texts.UsernameText).text = PlayerUserInfo.UserName;
        GetText((int)Texts.LevelText).text = PlayerUserInfo.Level.ToString();
        GetText((int)Texts.RankPointText).text = PlayerUserInfo.RankPoint.ToString();
        GetText((int)Texts.ExpText).text = $"{exp.ToString()} / {expMax.ToString()}";

        GetText((int)Texts.HighestRankText).text = PlayerUserInfo.HighestRankPoint.ToString();
        GetText((int)Texts.RankingText).text = "100";
        GetText((int)Texts.VictoriesText).text = PlayerUserInfo.Victories.ToString();
        GetText((int)Texts.WinRateText).text = PlayerUserInfo.WinRate.ToString();
    }

    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
