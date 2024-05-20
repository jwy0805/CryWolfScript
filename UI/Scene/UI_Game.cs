using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public partial class UI_Game : UI_Scene
{
    private UI_Portrait _isActive;
    private List<int> _northSlots = new();
    private List<int> _southSlots = new();
    private List<int> _northInactives = new();
    private List<int> _southInactives = new();
    public List<UI_Popup> Popups = new();
    
    #region Properties

    public MyPlayerController Player { get; set; }
    public UI_Mediator Mediator { get; private set; }
    public bool CameraDirection { get; set; } = true;
    public Camp Camp { get; private set; }
    public GameObject ResourceText => _dictCommonTxt["ResourceText"];

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
    
    public void RegisterInSlot(S_RegisterInSlot packet)
    {
        int objectId = packet.ObjectId;
        int unitId = packet.UnitId;
        var type = packet.ObjectType;
        var way = packet.Way;
        GameObject slot;
        
        if (way == SpawnWay.North)
        {
            var window = Util.FindChild(gameObject, "NorthCapacityWindow", true, true);
            slot = Util.FindChild(window, $"NorthUnitButton{_northSlots.Count}", true, true);
            _northSlots.Add(objectId);
        }
        else
        {
            var window = Util.FindChild(gameObject, "SouthCapacityWindow", true, true);
            slot = Util.FindChild(window, $"SouthUnitButton{_southSlots.Count}", true, true);
            _southSlots.Add(objectId);
        }
        
        var slotColleague = slot.GetComponent<UI_SlotColleague>(); 
        slotColleague.ObjectId = objectId;
        slot.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>($"Sprites/Portrait/{((UnitId)unitId).ToString()}");
        Mediator.CurrentWindow = null;
    }

    public void RegisterMonsterInSlot(S_RegisterMonsterInSlot packet)
    {
        int objectId = packet.ObjectId;
        int statueId = packet.StatueId;
        var slot = Mediator.FindSlotById(statueId);

        if (slot != null)
        {
            slot.ObjectIdList.Add(objectId);
        }
    }

    private void InactiveSlot()
    {   // TODO 비활성화 기능 삭제 예정
        int objectId = Player.SelectedUnitId;
        GameObject go = Managers.Object.FindById(objectId);
        if (go == null) return;
        if (go.TryGetComponent(out TuskController _) || go.TryGetComponent(out PumpkinController _))
        {
            // go가 Tusk, Pumpkin 이면 다시 원래 유닛 Active
            var packet = new S_RegisterInSlot
            {
                
            };
            RegisterInSlot(packet);
        }
        else
        {
            DeleteSlot(inactive: true);
        }
    }
    
    private void DeleteSlot(bool inactive = false)
    {
        // 서버에 삭제 요청, slot 이미지 초기화 및 정보 초기화
        int objectId = Player.SelectedUnitId;
        var slot = Mediator.FindSlotById(objectId);
        if (slot == null) return;
        slot.gameObject.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>($"Sprites/SlotBackground");

        int idDeleted = 0;
        foreach (var id in slot.ObjectIdList)
        {
            var go = Managers.Object.FindById(id);
            if (go == null) continue;

            var cc = go.GetComponent<CreatureController>();
            if (cc.ObjectType is not (GameObjectType.Tower or GameObjectType.MonsterStatue)) continue;
            idDeleted = cc.Id;
            
            if (cc.Way == SpawnWay.North)
            {
                int index = _northSlots.IndexOf(id);
                if (index != -1) _northSlots[index] = 0;
            }
            else
            {
                int index = _southSlots.IndexOf(id);
                if (index != -1) _southSlots[index] = 0;
            }
            
            break;
        }
        
        Managers.Network.Send(new C_DeleteUnit { ObjectId = idDeleted, Inactive = inactive });
        Mediator.CurrentWindow = null;
    }
    
    public void UpgradeSlot(S_UpgradeSlot packet)
    {
        int oldObjectId = packet.OldObjectId; // Existing TowerId or StatueId
        int newObjectId = packet.NewObjectId; // TowerId or StatueId To Be Updated
        int unitId = packet.UnitId;
        var type = packet.ObjectType;
        var way = packet.Way;
        var slot = Mediator.FindSlotById(oldObjectId);

        if (slot == null) return;
        slot.GetComponent<Image>().sprite = 
            Managers.Resource.Load<Sprite>($"Sprites/Portrait/{((UnitId)unitId).ToString()}");
        slot.ObjectIdList.Remove(oldObjectId);
        slot.ObjectIdList.Add(newObjectId);
        
        if (way == SpawnWay.North)
        {
            int index = _northSlots.IndexOf(oldObjectId);
            if (index != -1) _northSlots[index] = newObjectId;
        }
        else
        {
            int index = _southSlots.IndexOf(oldObjectId);
            if (index != -1) _southSlots[index] = newObjectId;
        }
    }
    
    protected override void SetButtonEvents()
    {
        foreach (var pair in _dictSkillBtn)
        {
            pair.Value.GetComponent<Button>().onClick.AddListener(OnSkillClicked);
        }

        foreach (var pair in _dictUnitBtn)
        {
            pair.Value.GetComponent<Button>().onClick.AddListener(OnSlotClicked);
        }
        
        _dictCommonBtn["MenuButton"].GetComponent<Button>().onClick.AddListener(OnUIObjectClicked);
        _dictCommonBtn["CapacityButton"].GetComponent<Button>().onClick.AddListener(OnUIObjectClicked);
        _dictCommonBtn["SubResourceButton"].GetComponent<Button>().onClick.AddListener(OnUIObjectClicked);
        _dictCommonBtn["MenuEmotionButton"].BindEvent(OnEmotionClicked);
        _dictCommonBtn["MenuChatButton"].BindEvent(OnChatClicked);
        _dictCommonBtn["MenuOptionButton"].BindEvent(OnOptionClicked);
        _dictCommonBtn["MenuExitButton"].BindEvent(OnExitClicked);
        _dictCommonBtn["CameraButton"].BindEvent(OnCameraClicked);
        _dictCommonBtn["UpgradeButton"].BindEvent(OnUpgradeClicked);
        
        _dictControlBtn["UnitUpgradeButton"].BindEvent(OnUpgradeUnitClicked);
        _dictControlBtn["UnitMoveButton"].BindEvent(OnMoveUnitClicked);
        _dictControlBtn["UnitInactivateButton"].BindEvent(OnInactivateClicked);
        _dictControlBtn["UnitDeleteButton"].BindEvent(OnDeleteUnitClicked);
    }
    
    #region ButtonFunction

    private void OnUIObjectClicked()
    {
        Mediator.CurrentSelectedButton = EventSystem.current.currentSelectedGameObject;
    }
    
    private void OnSlotClicked()
    {
        Mediator.CurrentSelectedSlot = EventSystem.current.currentSelectedGameObject;
    }
    
    private void OnPortraitClicked()
    {
        Mediator.CurrentSelectedPortrait = EventSystem.current.currentSelectedGameObject;
    }
    
    private void OnSkillClicked()
    {
        // 스킬 활성화, 튤립버튼 fill 적용
        Mediator.CurrentSelectedSkill = EventSystem.current.currentSelectedGameObject;
    }

    private void OnUpgradeClicked(PointerEventData data)
    {
        if (Mediator.CurrentSelectedPortrait == null) return;
        var level = GetLevelFromUIObject(Mediator.CurrentSelectedPortrait);
        if (level >= 3) return;
        var unitId = Mediator.CurrentSelectedPortrait.GetComponent<UI_Portrait>().UnitId;
        
        Managers.Network.Send(new C_PortraitUpgrade { UnitId = unitId });
    }

    private void OnUpgradeUnitClicked(PointerEventData data)
    {
        var level = Mediator.CurrentSelectedUnit.GetComponent<CreatureController>().Stat.Level;
        if (level >= 3) return;
        var unitId = Mediator.CurrentSelectedUnit.GetComponent<BaseController>().Id;
        
        Managers.Network.Send(new C_UnitUpgrade { ObjectId = unitId });
        Mediator.CurrentWindow = null;
    }
    
    private void OnMoveUnitClicked(PointerEventData data)
    {
        
    }
    
    private void OnInactivateClicked(PointerEventData data)
    {
        InactiveSlot();
    }

    private void OnDeleteUnitClicked(PointerEventData data)
    {
        DeleteSlot();
    }
    
    private void OnEmotionClicked(PointerEventData data)
    {
        
    }
    
    private void OnChatClicked(PointerEventData data)
    {
        
    }
    
    private void OnOptionClicked(PointerEventData data)
    {
        
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        
    }
    
    private void OnCameraClicked(PointerEventData data)
    {
        CameraDirection = !CameraDirection;
        GameObject virtualCamera = GameObject.Find("FollowCam");
        var followCam = virtualCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTransposer>();
        followCam.m_FollowOffset = CameraDirection ? new Vector3(0, 15, -10) : new Vector3(0, 15, 10);
    }
    
    #endregion
}
