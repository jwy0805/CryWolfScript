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

public class UI_GameSingleWay : UI_Game
{
    private UI_Portrait _isActive;
    private List<int> _northSlots = new();

    protected override void BindObjects()
    {
        BindData<Button>(typeof(CommonButtonsS), DictCommonBtn);
        BindData<Image>(typeof(CommonImagesS), DictCommonImg);
        BindData<TextMeshProUGUI>(typeof(CommonTextsS), DictCommonTxt);
        BindData<Button>(typeof(UnitButtonsS), DictUnitBtn);
        BindData<Button>(typeof(UnitControlButtonsS), DictControlBtn);
        
        SetLog();
        BringSkillPanels();
        BringBaseSkillPanels();
    }
    
    public override void RegisterInSlot(S_RegisterInSlot packet)
    {
        int objectId = packet.ObjectId;
        int unitId = packet.UnitId;
        var type = packet.ObjectType;
        var window = Util.FindChild(gameObject, "NorthCapacityWindow", true, true);
        var slot = Util.FindChild(window, $"NorthUnitButton{_northSlots.Count}", true, true);
        var slotColleague = slot.GetComponent<UI_SlotColleague>(); 

        _northSlots.Add(objectId);
        slotColleague.ObjectId = objectId;
        slot.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>($"Sprites/Portrait/{((UnitId)unitId).ToString()}");
        Mediator.CurrentWindow = null;
    }

    public override void RegisterMonsterInSlot(S_RegisterMonsterInSlot packet)
    {
        int objectId = packet.ObjectId;
        int statueId = packet.StatueId;
        var slot = Mediator.FindSlotById(statueId);

        if (slot != null)
        {
            slot.ObjectIdList.Add(objectId);
        }
    }
    
    protected override void DeleteSlot(bool inactive = false)
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
            
            int index = _northSlots.IndexOf(id);
            if (index != -1) _northSlots[index] = 0;
            
            break;
        }
        
        Managers.Network.Send(new C_DeleteUnit { ObjectId = idDeleted, Inactive = inactive });
        Mediator.CurrentWindow = null;
    }
    
    public override void UpgradeSlot(S_UpgradeSlot packet)
    {
        int oldObjectId = packet.OldObjectId; // Existing TowerId or StatueId
        int newObjectId = packet.NewObjectId; // TowerId or StatueId To Be Updated
        int unitId = packet.UnitId;
        var type = packet.ObjectType;
        var slot = Mediator.FindSlotById(oldObjectId);

        if (slot == null) return;
        slot.GetComponent<Image>().sprite = 
            Managers.Resource.Load<Sprite>($"Sprites/Portrait/{((UnitId)unitId).ToString()}");
        slot.ObjectIdList.Remove(oldObjectId);
        slot.ObjectIdList.Add(newObjectId);
        
        int index = _northSlots.IndexOf(oldObjectId);
        if (index != -1) _northSlots[index] = newObjectId;
    }
}
