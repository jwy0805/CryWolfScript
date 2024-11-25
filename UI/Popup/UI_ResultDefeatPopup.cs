using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ResultDefeatPopup : UI_Popup
{
    public int RankPointValue { get; set; }
    public int RankPoint { get; set; }
    public List<Reward> Reward { get; set; }

    private enum Images
    {
        DefeatLabel,
    }
    
    private enum Buttons
    {
        ContinueButton,
    }

    private enum Texts
    {
        RankValueText,
        RankText,
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
        StopGame();
    }

    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ContinueButton).gameObject.BindEvent(OnContinueClicked);
    }
    
    protected override void InitUI()
    {
        GetText((int)Texts.RankValueText).text = RankPointValue.ToString();
        GetText((int)Texts.RankText).text = RankPoint.ToString();
    }
    
    private void StopGame()
    {
        Managers.Network.Disconnect();
    }
    
    private void OnContinueClicked(PointerEventData data)
    {
        var popup = Managers.UI.ShowPopupUI<UI_RewardPopup>();
        popup.Rewards = Reward;
    }
}
