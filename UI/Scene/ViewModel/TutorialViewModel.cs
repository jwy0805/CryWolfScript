using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

/* Last Modified : 25. 04. 22
 * Version : 1.02
 */

public class TutorialViewModel : IDisposable
{
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    
    public readonly Dictionary<string, Action> MainEventDict = new();
    public readonly Dictionary<string, Action> BattleWolfEventDict = new();
    public readonly Dictionary<string, Action> BattleSheepEventDict = new();
    public readonly Dictionary<string, Action> CollectionEventDict = new();
    public readonly Dictionary<string, Action> CraftingEventDict = new();
    
    public bool ProcessTutorial { get; set; }
    public Faction TutorialFaction { get; set; }
    public int Step { get; set; }
    public UnitId RewardUnitId { get; set; }
    
    #region Events 
    
    public event Action<Vector3, Vector3> OnInitTutorialCamera1;
    public event Action<Vector3, Vector3> OnInitTutorialCamera2;
    public event Action OnStepTutorial;
    public event Action OnShowSpeakerAfter3Sec;
    public event Action OnShowSpeaker;
    public event Action OnShowNewSpeaker;
    public event Action OnChangeSpeaker;
    public event Action OnShowFactionSelectPopup;
    public event Action OnChangeFaceCry;
    public event Action OnChangeFaceHappy;
    public event Action OnChangeFaceNormal;
    public event Action OnUiBlocker;
    public event Action OffUiBlocker;
    public event Action OnHandImage;
    public event Action OffHandImage;
    public event Action OffContinueButton;
    public event Action OnContinueButton;
    public event Action PointToTimePanel;
    public event Action PointToResourcePanel;
    public event Action PointToCapacityPanel;
    public event Action PointToLog;
    public event Action PointToUpgradeButton;
    public event Action DragTankerUnit;
    public event Action DragRangerUnit;
    public event Action DragScene;
    public event Action ShowSimpleTooltip;
    public event Action ClearScene;
    public event Func<int> OnGetTankerIndex;
    public event Func<int> OnGetRangerIndex;
    public event Action PointToSkillButtonAndPortrait;
    public event Action AdjustUiBlockerSize;
    public event Action ResumeGame;
    
    #endregion
    
    [Inject]
    public TutorialViewModel(IUserService userService, IWebService webService, ITokenService tokenService)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
    }
    
    public void InitTutorialMain(Vector3 npc1Position, Vector3 camera1Position, Vector3 npc2Position, Vector3 camera2Position)
    {
        OnInitTutorialCamera1?.Invoke(npc1Position, camera1Position);
        OnInitTutorialCamera2?.Invoke(npc2Position, camera2Position);
        
        MainEventDict.TryAdd("ShowSpeaker", OnShowSpeakerAfter3Sec);
        MainEventDict.TryAdd("ShowNewSpeaker", OnShowNewSpeaker);
        MainEventDict.TryAdd("ChangeSpeaker", OnChangeSpeaker);
        MainEventDict.TryAdd("ShowFactionSelectPopup", OnShowFactionSelectPopup);
        MainEventDict.TryAdd("ChangeFaceCry", OnChangeFaceCry);
        MainEventDict.TryAdd("ChangeFaceHappy", OnChangeFaceHappy);
        MainEventDict.TryAdd("ChangeFaceNormal", OnChangeFaceNormal);
    }

    public void InitTutorialBattleWolf(Vector3 npcPosition, Vector3 cameraPosition)
    {
        OnInitTutorialCamera1?.Invoke(npcPosition, cameraPosition);
        
        BattleWolfEventDict.TryAdd("OnUiBlocker", OnUiBlocker);
        BattleWolfEventDict.TryAdd("OffUiBlocker", OffUiBlocker);
        BattleWolfEventDict.TryAdd("OnHandImage", OnHandImage);
        BattleWolfEventDict.TryAdd("OffHandImage", OffHandImage);
        BattleWolfEventDict.TryAdd("OffContinueButton", OffContinueButton);
        BattleWolfEventDict.TryAdd("OnContinueButton", OnContinueButton);
        BattleWolfEventDict.TryAdd("ShowSpeakerAfter3Sec", OnShowSpeakerAfter3Sec);
        BattleWolfEventDict.TryAdd("ShowSpeaker", OnShowSpeaker);
        BattleWolfEventDict.TryAdd("PointToTimePanel", PointToTimePanel);
        BattleWolfEventDict.TryAdd("PointToResourcePanel", PointToResourcePanel);
        BattleWolfEventDict.TryAdd("PointToCapacityPanel", PointToCapacityPanel);
        BattleWolfEventDict.TryAdd("PointToLog", PointToLog);
        BattleWolfEventDict.TryAdd("PointToUpgradeButton", PointToUpgradeButton);
        BattleWolfEventDict.TryAdd("DragTankerUnit", DragTankerUnit);
        BattleWolfEventDict.TryAdd("DragRangerUnit", DragRangerUnit);
        BattleWolfEventDict.TryAdd("DragScene", DragScene);
        BattleWolfEventDict.TryAdd("ShowSimpleTooltip", ShowSimpleTooltip);
        BattleWolfEventDict.TryAdd("ClearScene", ClearScene);
        BattleWolfEventDict.TryAdd("PointToSkillButtonAndPortrait", PointToSkillButtonAndPortrait);
        BattleWolfEventDict.TryAdd("AdjustUiBlockerSize", AdjustUiBlockerSize);
        BattleWolfEventDict.TryAdd("ResumeGame", ResumeGame);
    }

    public void InitTutorialBattleSheep(Vector3 npcPosition, Vector3 cameraPosition)
    {
        OnInitTutorialCamera1?.Invoke(npcPosition, cameraPosition);
        
        BattleSheepEventDict.TryAdd("OnUiBlocker", OnUiBlocker);
        BattleSheepEventDict.TryAdd("OffUiBlocker", OffUiBlocker);
        BattleSheepEventDict.TryAdd("OnHandImage", OnHandImage);
        BattleSheepEventDict.TryAdd("OffHandImage", OffHandImage);
        BattleSheepEventDict.TryAdd("OffContinueButton", OffContinueButton);
        BattleSheepEventDict.TryAdd("OnContinueButton", OnContinueButton);
        BattleSheepEventDict.TryAdd("ShowSpeakerAfter3Sec", OnShowSpeakerAfter3Sec);
        BattleSheepEventDict.TryAdd("ShowSpeaker", OnShowSpeaker);
        BattleSheepEventDict.TryAdd("PointToTimePanel", PointToTimePanel);
        BattleSheepEventDict.TryAdd("PointToResourcePanel", PointToResourcePanel);
        BattleSheepEventDict.TryAdd("PointToCapacityPanel", PointToCapacityPanel);
        BattleSheepEventDict.TryAdd("PointToLog", PointToLog);
        BattleSheepEventDict.TryAdd("PointToUpgradeButton", PointToUpgradeButton);
        BattleSheepEventDict.TryAdd("DragTankerUnit", DragTankerUnit);
        BattleSheepEventDict.TryAdd("DragRangerUnit", DragRangerUnit);
        BattleSheepEventDict.TryAdd("DragScene", DragScene);
        BattleSheepEventDict.TryAdd("ShowSimpleTooltip", ShowSimpleTooltip);
        BattleSheepEventDict.TryAdd("ClearScene", ClearScene);
        BattleSheepEventDict.TryAdd("PointToSkillButtonAndPortrait", PointToSkillButtonAndPortrait);
        BattleSheepEventDict.TryAdd("AdjustUiBlockerSize", AdjustUiBlockerSize);
        BattleSheepEventDict.TryAdd("ResumeGame", ResumeGame);
        BattleSheepEventDict.TryAdd("ChangeFaceHappy", OnChangeFaceHappy);
    }

    public void InitTutorialChangeFaction(Vector3 npcPosition, Vector3 cameraPosition)
    {
        OnInitTutorialCamera1?.Invoke(npcPosition, cameraPosition);
    }
    
    public void InitTutorialCollection()
    {
        
    }

    public void InitTutorialCrafting()
    {
        
    }
    
    public void ClearDictionary()
    {
        MainEventDict.Clear();
        BattleWolfEventDict.Clear();
        BattleSheepEventDict.Clear();
        CollectionEventDict.Clear();
        CraftingEventDict.Clear();
    }
    
    public async Task StartTutorial(Faction faction, int sessionId)
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

    public void StepTutorial()
    {
        OnStepTutorial?.Invoke();
    }
    
    public void SendHoldPacket(bool hold)
    {
        var holdPacket = new C_HoldGame
        {
            Hold = hold
        };
        
        Managers.Network.Send(holdPacket);
    }

    public void PortraitDragStartHandler()
    {
        if ((Step != 6 && Step != 10 && Step != 12 && Util.Faction == Faction.Wolf) ||
            (Step != 6 && Step != 8 && Util.Faction == Faction.Sheep)) return;
        
        SendHoldPacket(false);
    }

    public void PortraitDragEndHandler()
    {
        if (Util.Faction == Faction.Wolf)
        {
            if (Step != 6 && Step != 12) return;
        }

        if (Util.Faction == Faction.Sheep)
        {
            if (Step != 6 && Step != 8) return;
        }
        
        _ = ShowTutorialPopup();
    }

    public int GetTankerAnchorIndex()
    {
        return OnGetTankerIndex?.Invoke() ?? int.MinValue;
    }
    
    public int GetRangerAnchorIndex()
    {
        return OnGetRangerIndex?.Invoke() ?? int.MinValue;
    }
    
    public void StepTutorialByClickingUI(bool hold = true)
    {
        SendHoldPacket(hold);
        
        if (Util.Faction == Faction.Wolf)
        {
            Managers.UI.ClosePopupUI<UI_TutorialBattleWolfPopup>();
        }
        else
        {
            Managers.UI.ClosePopupUI<UI_TutorialBattleSheepPopup>();
        }
        
        _ = ShowTutorialPopup();
    }
    
    public async Task ShowTutorialPopup()
    {
        await Task.Delay(100);
        
        // Init method in popup implements step tutorial
        if (Util.Faction == Faction.Wolf)
        {
            await Managers.UI.ShowPopupUI<UI_TutorialBattleWolfPopup>();
        }
        else
        {
            await Managers.UI.ShowPopupUI<UI_TutorialBattleSheepPopup>();
        }
        
        ClearDictionary();
    }

    private async void ShowTutorialContinueNotifyPopupSheep()
    {
        try
        {
            const string titleKey = "notify_select_tutorial_continue_title";
            const string messageKey = "notify_select_tutorial_wolf_continue_message";
        
            await Managers.UI.ShowNotifySelectPopup(titleKey, messageKey,
                StartTutorialWolf, 
                RejectFollowWolfTutorial);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    private async void ShowTutorialContinueNotifyPopupWolf()
    {
        try
        {
            const string titleKey = "notify_select_tutorial_continue_title";
            const string messageKey = "notify_select_tutorial_sheep_continue_message";
        
            await Managers.UI.ShowNotifySelectPopup(titleKey, messageKey,
                StartTutorialSheep, 
                RejectFollowSheepTutorial);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private void StartTutorialSheep()
    {
        Util.Faction = Faction.Sheep;
        TutorialFaction = Faction.Sheep;
        StartTutorial();
    }
    
    private void StartTutorialWolf()
    {
        Util.Faction = Faction.Wolf;
        TutorialFaction = Faction.Wolf;
        StartTutorial();
    }

    private void StartTutorial()
    {
        ProcessTutorial = true;
        _ = Managers.Network.ConnectGameSession();
    }

    private async void RejectFollowSheepTutorial()
    {
        try
        {
            const string titleKey = "notify_select_tutorial_omit_title";
            const string messageKey = "notify_select_tutorial_omit_message";
        
            await Managers.UI.ShowNotifySelectPopup(titleKey, messageKey,
                OnRejectTutorial,
                ShowTutorialContinueNotifyPopupWolf);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private async void RejectFollowWolfTutorial()
    {
        try
        {
            const string titleKey = "notify_select_tutorial_omit_title";
            const string messageKey = "notify_select_tutorial_omit_message";
        
            await Managers.UI.ShowNotifySelectPopup(titleKey, messageKey,
                OnRejectTutorial,
                ShowTutorialContinueNotifyPopupSheep);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private void OnRejectTutorial()
    {
        var packet = new UpdateTutorialRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            TutorialTypes = new[] { TutorialType.BattleWolf, TutorialType.BattleSheep, TutorialType.ChangeFaction },
            Done = true,
        };
        
        _ = _webService.SendWebRequestAsync<UpdateTutorialResponse>(
            "UserAccount/UpdateTutorial", UnityWebRequest.kHttpVerbPUT, packet);

        Managers.UI.CloseAllPopupUI();
    }

    public async Task BattleTutorialEndHandler(Faction faction)
    {
        if (faction == Faction.Wolf)
        {
            _userService.TutorialInfo.WolfTutorialDone = true;
            UpdateTutorialInfo(TutorialType.BattleWolf);
        }
        else
        {
            _userService.TutorialInfo.SheepTutorialDone = true;
            UpdateTutorialInfo(TutorialType.BattleSheep);
        }
        
        var popup = await Managers.UI.ShowPopupUI<UI_RewardPopup>();
        popup.FromTutorial = true;
        popup.Rewards = new List<Reward>
        {
            new()
            {
                ProductType = Google.Protobuf.Protocol.ProductType.Unit,
                ItemId = (int)RewardUnitId,
                Count = 1
            }
        };
        
        Managers.Network.Disconnect();
    }

    public void ChangeFactionTutorialEndHandler()
    {
        UpdateTutorialInfo(TutorialType.ChangeFaction);
    }
    
    public void CompleteTutorialWolf()
    {
        ShowTutorialContinueNotifyPopupWolf();   
    }
    
    public void CompleteTutorialSheep()
    {
        ShowTutorialContinueNotifyPopupSheep();
    }
    
    private void UpdateTutorialInfo(TutorialType tutorialType)
    {
        var packet = new UpdateTutorialRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            TutorialTypes = new[] { tutorialType },
            Done = true
        };
        
        _ = _webService.SendWebRequestAsync<UpdateTutorialResponse>(
            "UserAccount/UpdateTutorial", UnityWebRequest.kHttpVerbPUT, packet);

        Debug.Log($"User {User.Instance.UserInfo.UserName} completed tutorial: {tutorialType}");
    }

    public void SetTutorialReward(UnitId rewardUnitId)
    {
        RewardUnitId = rewardUnitId;
    }
    
    public void Dispose()
    {
        ClearDictionary();
    }
}
