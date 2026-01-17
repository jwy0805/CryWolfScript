using System;
using System.Collections.Generic;
using System.Linq;
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
    private UnitId _rewardUnitId;

    public readonly Dictionary<string, Action> ActionDict = new();
    
    public bool ProcessTutorial { get; set; }
    public Faction TutorialFaction { get; set; }
    public string CurrentTag { get; set; } = string.Empty;
    public string NextTag { get; set; } = string.Empty;
    
    #region Events 
    
    public event Action<Vector3, Vector3> OnInitTutorialCamera1;
    public event Action<Vector3, Vector3> OnInitTutorialCamera2;
    public event Func<string, Task> OnRunTutorialTag;
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
    public event Action OnUiBlocker05Sec;
    public event Action OnUiBlocker1Sec;
    public event Action OnUiBlocker2Sec;
    public event Action OnUiBlocker3Sec;
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
        InitActionDict();
    }

    public void InitTutorialBattleWolf(Vector3 npcPosition, Vector3 cameraPosition)
    {
        OnInitTutorialCamera1?.Invoke(npcPosition, cameraPosition);
        InitActionDict();
    }

    public void InitTutorialBattleSheep(Vector3 npcPosition, Vector3 cameraPosition)
    {
        OnInitTutorialCamera1?.Invoke(npcPosition, cameraPosition);
        InitActionDict();
    }
    
    public void InitTutorialChangeFaction(Vector3 npcPosition, Vector3 cameraPosition)
    {
        OnInitTutorialCamera1?.Invoke(npcPosition, cameraPosition);
        InitActionDict();
    }
    
    public void InitTutorialCollection()
    {
        
    }

    public void InitTutorialCrafting()
    {
        
    }

    private void InitActionDict()
    {
    ActionDict.TryAdd("ShowSpeaker", () => OnShowSpeaker?.Invoke());
    ActionDict.TryAdd("ShowSpeakerAfter3Sec", () => OnShowSpeakerAfter3Sec?.Invoke());
    ActionDict.TryAdd("ShowNewSpeaker", () => OnShowNewSpeaker?.Invoke());
    ActionDict.TryAdd("ChangeSpeaker", () => OnChangeSpeaker?.Invoke());
    ActionDict.TryAdd("ShowFactionSelectPopup", () => OnShowFactionSelectPopup?.Invoke());
    ActionDict.TryAdd("ChangeFaceCry", () => OnChangeFaceCry?.Invoke());
    ActionDict.TryAdd("ChangeFaceHappy", () => OnChangeFaceHappy?.Invoke());
    ActionDict.TryAdd("ChangeFaceNormal", () => OnChangeFaceNormal?.Invoke());
    ActionDict.TryAdd("OnUiBlocker", () => OnUiBlocker?.Invoke());
    ActionDict.TryAdd("OffUiBlocker", () => OffUiBlocker?.Invoke());
    ActionDict.TryAdd("OnUiBlocker05Sec", () => OnUiBlocker05Sec?.Invoke());
    ActionDict.TryAdd("OnUiBlocker1Sec", () => OnUiBlocker1Sec?.Invoke());
    ActionDict.TryAdd("OnUiBlocker2Sec", () => OnUiBlocker2Sec?.Invoke());
    ActionDict.TryAdd("OnUiBlocker3Sec", () => OnUiBlocker3Sec?.Invoke());
    ActionDict.TryAdd("OnHandImage", () => OnHandImage?.Invoke());
    ActionDict.TryAdd("OffHandImage", () => OffHandImage?.Invoke());
    ActionDict.TryAdd("OffContinueButton", () => OffContinueButton?.Invoke());
    ActionDict.TryAdd("OnContinueButton", () => OnContinueButton?.Invoke());
    ActionDict.TryAdd("PointToTimePanel", () => PointToTimePanel?.Invoke());
    ActionDict.TryAdd("PointToResourcePanel", () => PointToResourcePanel?.Invoke());
    ActionDict.TryAdd("PointToCapacityPanel", () => PointToCapacityPanel?.Invoke());
    ActionDict.TryAdd("PointToLog", () => PointToLog?.Invoke());
    ActionDict.TryAdd("PointToUpgradeButton", () => PointToUpgradeButton?.Invoke());
    ActionDict.TryAdd("DragTankerUnit", () => DragTankerUnit?.Invoke());
    ActionDict.TryAdd("DragRangerUnit", () => DragRangerUnit?.Invoke());
    ActionDict.TryAdd("DragScene", () => DragScene?.Invoke());
    ActionDict.TryAdd("ShowSimpleTooltip", () => ShowSimpleTooltip?.Invoke());
    ActionDict.TryAdd("ClearScene", () => ClearScene?.Invoke());
    ActionDict.TryAdd("PointToSkillButtonAndPortrait", () => PointToSkillButtonAndPortrait?.Invoke());
    ActionDict.TryAdd("AdjustUiBlockerSize", () => AdjustUiBlockerSize?.Invoke());
    ActionDict.TryAdd("ResumeGame", () => ResumeGame?.Invoke());
    }
    
    public void ClearDictionary()
    {
        ActionDict.Clear();
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
            Managers.Game.TutorialType = faction == Faction.Wolf ? TutorialType.BattleWolf : TutorialType.BattleSheep;
            Managers.Scene.LoadScene(Define.Scene.Game);
        }
    }

    public void InitTag()
    {
        CurrentTag = string.Empty;
        NextTag = string.Empty;
    }
    
    public void RunTutorialTag(string tutorialTag)
    {
        OnRunTutorialTag?.Invoke(tutorialTag);
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
        if (!CurrentTag.Contains("Drag")) return;
        SendHoldPacket(false);
    }

    public void PortraitDragEndHandler()
    {
        if (CurrentTag.Contains("DragOneMore")) return;
        if (!CurrentTag.Contains("Drag")) return;
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
    
    public async Task ShowTutorialPopup(bool isInterrupted = false, string tutorialTag = "")
    {
        await Task.Delay(100);
        
        // Init method in popup implements step tutorial
        if (Util.Faction == Faction.Wolf)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_TutorialBattleWolfPopup>();
            popup.IsInterrupted = isInterrupted;
            if (isInterrupted)
            {
                popup.InterruptTag = tutorialTag;
            }
            else
            {
                if (tutorialTag != string.Empty)
                {
                    NextTag = tutorialTag;
                }
            }
        }
        else
        {
            var popup = await Managers.UI.ShowPopupUI<UI_TutorialBattleSheepPopup>();
            popup.IsInterrupted = isInterrupted;
            if (isInterrupted)
            {
                popup.InterruptTag = tutorialTag;
            }
            else
            {
                if (tutorialTag != string.Empty)
                {
                    NextTag = tutorialTag;
                }
            }
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
            TutorialStep = 0
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
                ItemId = (int)_rewardUnitId,
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
            Done = true,
            TutorialStep = 0
        };
        
        _ = _webService.SendWebRequestAsync<UpdateTutorialResponse>(
            "UserAccount/UpdateTutorial", UnityWebRequest.kHttpVerbPUT, packet);
    }

    public void OnInterruptTutorial(TutorialType tutorialType)
    {
        var packet = new UpdateTutorialRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            TutorialTypes = new[] { tutorialType },
            Done = false,
            TutorialStep = -1
        };
        
        _webService.SendWebRequest("UserAccount/UpdateTutorial", UnityWebRequest.kHttpVerbPUT, packet);
    }

    public void SetTutorialReward(UnitId rewardUnitId)
    {
        _rewardUnitId = rewardUnitId;
    }
    
    public void Dispose()
    {
        ClearDictionary();
    }
}
