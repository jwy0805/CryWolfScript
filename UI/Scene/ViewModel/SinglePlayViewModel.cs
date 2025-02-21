using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

public class SinglePlayViewModel : IDisposable
{
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    
    public int SelectedStageId { get; set; }
    public int StageLevel { get; set; }
    public List<UserStageInfo> UserStageInfos { get; set; }
    public List<StageEnemyInfo> StageEnemyInfos { get; set; }
    public List<StageRewardInfo> StageRewardInfos { get; set; }
    
    [Inject]
    public SinglePlayViewModel(IWebService webService, ITokenService tokenService)
    {
        _webService = webService;
        _tokenService = tokenService;
    }
    
    public async Task Initialize()
    {
        var stagePacket = new LoadStageInfoPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken()
        };
        var stageTask = _webService.SendWebRequestAsync<LoadStageInfoPacketResponse>(
            "SinglePlay/LoadStageInfo", UnityWebRequest.kHttpVerbPOST, stagePacket);

        await stageTask;

        StageLevel = Util.Faction == Faction.Sheep 
            ? stageTask.Result.UserStageInfos.Where(usi => usi.StageId < 5000).Max(usi => usi.StageLevel) 
            : stageTask.Result.UserStageInfos.Where(usi => usi.StageId >= 5000).Max(usi => usi.StageLevel);

        UserStageInfos = stageTask.Result.UserStageInfos;
        StageEnemyInfos = stageTask.Result.StageEnemyInfos;
        StageRewardInfos = stageTask.Result.StageRewardInfos;
    }

    public async Task ConnectGameSession()
    {
        var sessionTask = Managers.Network.ConnectGameSession();
        
        await sessionTask;
    }

    public async Task StartSinglePlay(int sessionId)
    {
        var changePacket = new ChangeActPacketSingleRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            SessionId = sessionId,
            StageId = SelectedStageId,
            Faction = Util.Faction
        };
        var apiTask = _webService.SendWebRequestAsync<ChangeActPacketSingleResponse>(
            "SinglePlay/StartGame", UnityWebRequest.kHttpVerbPUT, changePacket);

        await apiTask;
        
        if (apiTask.Result.ChangeOk)
        {
            Managers.Scene.LoadScene(Define.Scene.Game);
        }
    }
    
    public void Dispose()
    {
        
    }
}
