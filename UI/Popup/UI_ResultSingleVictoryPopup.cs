using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ResultSingleVictoryPopup : UI_Popup
{
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public List<Reward> Reward { get; set; }
    public int Star { get; set; }
    
    private enum Images
    {
        Star
    }
    
    private enum Buttons
    {
        ContinueButton,
    }

    private enum Texts
    {
        VictoryText,
        ContinueText
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
        
        Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ContinueButton).gameObject.BindEvent(OnContinueClicked);
    }

    protected override void InitUI()
    {
        var starPanel = GetImage((int)Images.Star).transform;
        
        for (int i = 0; i < 3; i++)
        {
            starPanel.GetChild(i).gameObject.SetActive(i < Star);
        }
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
