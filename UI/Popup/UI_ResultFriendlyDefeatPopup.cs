using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_ResultFriendlyDefeatPopup : UI_Popup
{
    private ISignalRClient _signalRClient;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private enum Images
    {
        
    }
    
    private enum Buttons
    {
        ContinueButton,
    }

    private enum Texts
    {
        DefeatText,
        ContinueText
    }

    [Inject]
    public void Construct(ISignalRClient signalRClient)
    {
        _signalRClient = signalRClient;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            BindObjects();
            InitButtonEvents();
            await InitUIAsync();
            await StopGame();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _ = Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ContinueButton).gameObject.BindEvent(OnContinueClicked);
    }

    private async Task StopGame()
    {
        Managers.Network.Disconnect();
        var tuple = await _signalRClient.ReEntryFriendlyMatch(User.Instance.UserInfo.UserName);
        Managers.Network.IsFriendlyMatchHost = tuple.Item1;
        Managers.Game.ReEntryResponse = tuple.Item2;
        Managers.Game.ReEntry = true;
    }
    
    private void OnContinueClicked(PointerEventData data)
    {
        Managers.Scene.LoadScene(Define.Scene.FriendlyMatch);
        Managers.UI.ClosePopupUI();
    }
}
