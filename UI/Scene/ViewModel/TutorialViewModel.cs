using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

public class TutorialViewModel
{
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    
    public readonly Dictionary<string, Action> MainEventDict = new();
    public readonly Dictionary<string, Action> BattleWolfEventDict = new();
    public readonly Dictionary<string, Action> BattleSheepEventDict = new();
    
    public event Action<Vector3, Vector3> OnInitTutorialCamera1;
    public event Action<Vector3, Vector3> OnInitTutorialCamera2;
    public event Action OnShowSpeaker;
    public event Action OnShowNewSpeaker;
    public event Action OnChangeSpeaker;
    public event Action OnShowFactionSelectPopup;
    public event Action OnChangeFaceCry;
    public event Action OnChangeFaceHappy;
    public event Action OnChangeFaceNormal;
    
    [Inject]
    public TutorialViewModel(IWebService webService, ITokenService tokenService)
    {
        _webService = webService;
        _tokenService = tokenService;
    }
    
    public void InitTutorialMain(Vector3 npc1Position, Vector3 camera1Position, Vector3 npc2Position, Vector3 camera2Position)
    {
        OnInitTutorialCamera1?.Invoke(npc1Position, camera1Position);
        OnInitTutorialCamera2?.Invoke(npc2Position, camera2Position);
        
        MainEventDict.Add("ShowSpeaker", OnShowSpeaker);
        MainEventDict.Add("ShowNewSpeaker", OnShowNewSpeaker);
        MainEventDict.Add("ChangeSpeaker", OnChangeSpeaker);
        MainEventDict.Add("ShowFactionSelectPopup", OnShowFactionSelectPopup);
        MainEventDict.Add("ChangeFaceCry", OnChangeFaceCry);
        MainEventDict.Add("ChangeFaceHappy", OnChangeFaceHappy);
        MainEventDict.Add("ChangeFaceNormal", OnChangeFaceNormal);
    }

    public void InitTutorialBattleWolf(Vector3 npcPosition, Vector3 cameraPosition)
    {
        OnInitTutorialCamera1?.Invoke(npcPosition, cameraPosition);
        
        MainEventDict.Add("ShowSpeaker", OnShowSpeaker);
    }

    public void InitTutorialBattleSheep(Vector3 npcPosition, Vector3 cameraPosition)
    {
        OnInitTutorialCamera1?.Invoke(npcPosition, cameraPosition);
        
        MainEventDict.Add("ShowSpeaker", OnShowSpeaker);
        MainEventDict.Add("ChangeFaceCry", OnChangeFaceCry);
        MainEventDict.Add("ChangeFaceHappy", OnChangeFaceHappy);
        MainEventDict.Add("ChangeFaceNormal", OnChangeFaceNormal);
        
    }
    
    public async void StartTutorial(Faction faction, int sessionId)
    {
        Managers.Game.IsTutorial = true;
        
        var changePacket = new ChangeActPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            SessionId = sessionId,
            Faction = faction,
            MapId = Managers.Map.MapId
        };
        
        var apiTask = _webService.SendWebRequestAsync<ChangeActPacketResponse>(
            "Match/ChangeActByTutorial", UnityWebRequest.kHttpVerbPUT, changePacket);

        await apiTask;
        
        if (apiTask.Result.ChangeOk)
        {
            Managers.Scene.LoadScene(Define.Scene.Game);
        }
    }

    public void SendHoldPacket(bool hold)
    {
        var holdPacket = new C_HoldGame
        {
            Hold = hold
        };
        
        Managers.Network.Send(holdPacket);
    }
}
