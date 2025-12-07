using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_ResultFriendlyVictoryPopup : UI_Popup
{
    private ISignalRClient _signalRClient;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    private GameObject _rotateImage;
    
    private enum Images
    {
        RotateImage,
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
    
    private void Update()
    {
        _rotateImage.transform.Rotate(0f, 0f, Time.deltaTime * 10f);
    }
    
    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _rotateImage = GetImage((int)Images.RotateImage).gameObject;
        
        _ = Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ContinueButton).gameObject.BindEvent(OnContinueClicked);
    }

    private async Task StopGame()
    {
        Managers.Network.Disconnect();
        var tuple = await _signalRClient.ReEntryFriendlyMatch(User.Instance.UserInfo.UserTag);
        // Managers.Network.IsFriendlyMatchHost = tuple.Item1;
        Managers.Game.ReEntryResponse = tuple.Item2;
        Managers.Game.ReEntry = true;
    }
    
    private void OnContinueClicked(PointerEventData data)
    {
        Managers.Scene.LoadScene(Define.Scene.FriendlyMatch);
    }
}
