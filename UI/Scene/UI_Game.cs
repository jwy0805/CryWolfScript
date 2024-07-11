using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UI_Game : UI_Scene
{
    
    #region Properties

    public MyPlayerController Player { get; set; }
    public UI_Mediator Mediator { get; private set; }
    public bool CameraDirection { get; set; } = true;
    public Camp Camp { get; set; }
    public GameObject ResourceText => DictCommonTxt["ResourceText"];
    public List<UI_Popup> Popups { get; } = new();

    #endregion

    protected override void Init()
    {
        base.Init();
        Mediator = GetComponent<UI_Mediator>();
        Camp = Util.Camp;
        
        BindObjects();
        SetButtonEvents();
        SetUI();
        SetTexts();
    }

    public virtual void RegisterInSlot(S_RegisterInSlot packet) { }
    
    public virtual void RegisterMonsterInSlot(S_RegisterMonsterInSlot packet) { }

    protected virtual void DeleteSlot(bool inactive = false) { }
    
    public virtual void UpgradeSlot(S_UpgradeSlot packet) { }
    
    protected override void SetButtonEvents()
    {
        foreach (var pair in DictSkillBtn)
        {
            pair.Value.GetComponent<Button>().onClick.AddListener(OnSkillClicked);
        }

        foreach (var pair in DictUnitBtn)
        {
            pair.Value.GetComponent<Button>().onClick.AddListener(OnSlotClicked);
        }
        
        DictCommonBtn["MenuButton"].GetComponent<Button>().onClick.AddListener(OnUIObjectClicked);
        DictCommonBtn["CapacityButton"].GetComponent<Button>().onClick.AddListener(OnUIObjectClicked);
        DictCommonBtn["SubResourceButton"].GetComponent<Button>().onClick.AddListener(OnUIObjectClicked);
        DictCommonBtn["MenuEmotionButton"].BindEvent(OnEmotionClicked);
        DictCommonBtn["MenuChatButton"].BindEvent(OnChatClicked);
        DictCommonBtn["MenuOptionButton"].BindEvent(OnOptionClicked);
        DictCommonBtn["MenuExitButton"].BindEvent(OnExitClicked);
        DictCommonBtn["CameraButton"].BindEvent(OnCameraClicked);
        DictCommonBtn["UpgradeButton"].BindEvent(OnUpgradeClicked);
        
        DictControlBtn["UnitUpgradeButton"].BindEvent(OnUpgradeUnitClicked);
        DictControlBtn["UnitMoveButton"].BindEvent(OnMoveUnitClicked);
        DictControlBtn["UnitDeleteButton"].BindEvent(OnDeleteUnitClicked);
    }
    
    #region ButtonFunctions

    protected virtual void OnUIObjectClicked()
    {
        Mediator.CurrentSelectedButton = EventSystem.current.currentSelectedGameObject;
    }
    
    protected virtual void OnSlotClicked()
    {
        Mediator.CurrentSelectedSlot = EventSystem.current.currentSelectedGameObject;
    }
    
    protected virtual void OnPortraitClicked()
    {
        Mediator.CurrentSelectedPortrait = EventSystem.current.currentSelectedGameObject;
    }
    
    protected virtual void OnSkillClicked()
    {
        // 스킬 활성화, 튤립버튼 fill 적용
        Mediator.CurrentSelectedSkill = EventSystem.current.currentSelectedGameObject;
    }
    
    protected virtual void OnUpgradeClicked(PointerEventData data)
    {
        if (Mediator.CurrentSelectedPortrait == null) return;
        var level = GetLevelFromUIObject(Mediator.CurrentSelectedPortrait);
        if (level >= 3) return;
        var unitId = Mediator.CurrentSelectedPortrait.GetComponent<UI_Portrait>().UnitId;
        
        Managers.Network.Send(new C_PortraitUpgrade { UnitId = unitId });
    }

    protected virtual void OnUpgradeUnitClicked(PointerEventData data)
    {
        var level = Mediator.CurrentSelectedUnit.GetComponent<CreatureController>().Stat.Level;
        if (level >= 3) return;
        var unitId = Mediator.CurrentSelectedUnit.GetComponent<BaseController>().Id;
        
        Managers.Network.Send(new C_UnitUpgrade { ObjectId = unitId });
        Mediator.CurrentWindow = null;
    }
    
    protected virtual void OnMoveUnitClicked(PointerEventData data)
    {
        
    }

    protected virtual void OnDeleteUnitClicked(PointerEventData data)
    {
        DeleteSlot();
    }
    
    protected virtual void OnEmotionClicked(PointerEventData data)
    {
        
    }
    
    protected virtual void OnChatClicked(PointerEventData data)
    {
        
    }
    
    protected virtual void OnOptionClicked(PointerEventData data)
    {
        
    }
    
    protected virtual void OnExitClicked(PointerEventData data)
    {
        
    }
    
    protected virtual void OnCameraClicked(PointerEventData data)
    {
        CameraDirection = !CameraDirection;
        GameObject virtualCamera = GameObject.Find("FollowCam");
        var followCam = virtualCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTransposer>();
        followCam.m_FollowOffset = CameraDirection ? new Vector3(0, 15, -10) : new Vector3(0, 15, 10);
    }

    #endregion
}
