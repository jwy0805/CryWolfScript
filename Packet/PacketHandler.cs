using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Febucci.UI;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using State = Google.Protobuf.Protocol.State;

public class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = (S_EnterGame)packet;
        if (enterGamePacket != null)
        {
            Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
        }
    }
    
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGamePacket = (S_LeaveGame)packet;
        GameObjectType type = ObjectManager.GetObjectTypeById(leaveGamePacket.ObjectId);
        switch (type)
        {
            case GameObjectType.Effect:
                GameObject go = Managers.Object.FindById(leaveGamePacket.ObjectId);
                Managers.Resource.Destroy(go);
                Managers.Object.Remove(leaveGamePacket.ObjectId);
                break;
            case GameObjectType.Player:
                Managers.Object.Remove(leaveGamePacket.ObjectId);
                Managers.Object.RemoveMyPlayer();
                break;
        }
    }
    
    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = (S_Spawn)packet;
        foreach (var obj in spawnPacket.Objects)
        {
            Managers.Object.Add(obj);
        }
    }

    public static void S_SpawnParentHandler(PacketSession session, IMessage packet)
    {
        S_SpawnParent spawnPacket = (S_SpawnParent)packet;
        Managers.Object.Add(spawnPacket.Object, spawnPacket.ParentId);
    }
    
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = (S_Despawn)packet;
        foreach (var id in despawnPacket.ObjectIds)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(id);
            var go = Managers.Object.FindById(id);
            Managers.Object.Remove(id);
            Managers.Resource.Destroy(go);
            
            // switch (type)
            // {
            //     case GameObjectType.Tower:
            //     case GameObjectType.Monster:
            //     case GameObjectType.Sheep:
            //         go = Managers.Object.FindById(id);
            //         Managers.Object.Remove(id);
            //         go.GetComponent<CreatureController>().();
            //         break;
            //     default:
            //         
            //         break;
            // }
        }
    }
    
    public static void S_PlayerMoveHandler(PacketSession session, IMessage packet)
    {
        S_PlayerMove playerMovePacket = (S_PlayerMove)packet;
        GameObject go = Managers.Object.FindById(playerMovePacket.ObjectId);
        if (go == null) return;
        go.TryGetComponent(out MyPlayerController pc);
        if (pc == null) return;

        pc.DestVec = playerMovePacket.DestPos;
        pc.State = playerMovePacket.State;
    }
    
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = (S_Move)packet;
        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null) return;
        go.TryGetComponent(out BaseController bc);
        if (bc != null) bc.PosInfo = movePacket.PosInfo;
    }

    public static void S_StateHandler(PacketSession session, IMessage packet)
    {
        S_State statePacket = (S_State)packet;
        GameObject go = Managers.Object.FindById(statePacket.ObjectId);
        if (go == null) return;
        go.TryGetComponent(out CreatureController cc);
        if (cc != null) cc.NextState = statePacket.State;
    }
    
    public static void S_SetPathHandler(PacketSession session, IMessage packet)
    {
        S_SetPath pathPacket = (S_SetPath)packet;
        GameObject go = Managers.Object.FindById(pathPacket.ObjectId);
        if (go == null) return;

        GameObjectType type = ObjectManager.GetObjectTypeById(pathPacket.ObjectId);
        if (type != GameObjectType.Projectile)
        {
            if (go.TryGetComponent(out CreatureController cc) == false) return;
            if (cc == null) return;
            cc.OnPathReceived(pathPacket);
        }
        else
        {
            
        }
        // if (type != GameObjectType.Projectile)
        // {
        //     go.TryGetComponent(out CreatureController cc);
        //     if (cc == null) return;
        //     if (destPacket.Dest == null) return;
        //
        //     Queue<Vector3> destQueue = new Queue<Vector3>();
        //     Queue<double> dirQueue = new Queue<double>();
        //     if (destPacket.Dest.Count == 0)
        //     {
        //         cc.TotalMoveSpeed = destPacket.MoveSpeed;
        //         cc.DestQueue = destQueue;
        //         cc.DirQueue = dirQueue;
        //         return;
        //     }
        //
        //     foreach (var dest in destPacket.Dest) destQueue.Enqueue(new Vector3(dest.X, dest.Y, dest.Z));
        //     foreach (var dir in destPacket.Dir) dirQueue.Enqueue(dir);
        //
        //     cc.TotalMoveSpeed = destPacket.MoveSpeed;
        //     cc.DestQueue = destQueue;
        //     cc.DirQueue = dirQueue;
        // }
        // else
        // {
        //     go.TryGetComponent(out ProjectileController pc);
        //     if (pc == null) return;
        //     if (destPacket.Dest == null) return;
        //     if (destPacket.Dest.Count == 0) return;
        //
        //     Vector3 destPos = new Vector3(destPacket.Dest[0].X, destPacket.Dest[0].Y, destPacket.Dest[0].Z);
        //     pc.destPos = destPos;
        // }
    }

    public static void S_SetKnockBackHandler(PacketSession session, IMessage packet)
    {
        S_SetKnockBack knockBackPacket = (S_SetKnockBack)packet;
        GameObject go = Managers.Object.FindById(knockBackPacket.ObjectId);
        if (go == null) return;

        if (go.TryGetComponent(out BaseController bc))
        {
            Vector3 v = new Vector3(knockBackPacket.Dest.X, knockBackPacket.Dest.Y, knockBackPacket.Dest.Z);
            bc.DestPos = v;
            bc.SetKnockBackDest = true;
        }
    }
    
    public static void S_SetDestSkillHandler(PacketSession session, IMessage packet)
    {
        S_SetDestSkill destSkillPacket = (S_SetDestSkill)packet;
        GameObject go = Managers.Object.FindById(destSkillPacket.ObjectId);
        if (go == null) return;
        go.TryGetComponent(out ProjectileController pc);
        if (pc == null) return;
        if (destSkillPacket.Dest == null) return;

        Vector3 dest = new Vector3(destSkillPacket.Dest.X, destSkillPacket.Dest.Y, destSkillPacket.Dest.Z);
        pc.destPos = dest;
    }

    public static void S_SetDestResourceHandler(PacketSession session, IMessage packet)
    {
        S_SetDestResource resourceDestPacket = (S_SetDestResource)packet;
        GameObject go = Managers.Object.FindById(resourceDestPacket.ObjectId);
        if (go == null) return;
        go.TryGetComponent(out ResourceController rc);
        if (rc == null) return;
        if (resourceDestPacket.Dest == null) return;

        Vector3 dest = new Vector3(resourceDestPacket.Dest.X, resourceDestPacket.Dest.Y, resourceDestPacket.Dest.Z);
        rc.DestPos = dest + Vector3.up * 0.5f;
        rc.moveFlag = true;
        rc.yield = resourceDestPacket.Yield;
    }

    public static void S_SetAnimSpeedHandler(PacketSession session, IMessage packet)
    {
        S_SetAnimSpeed animPacket = (S_SetAnimSpeed)packet;
        GameObject go = GameObject.FindWithTag("SkillSubject");
        if (go.TryGetComponent(out SkillSubject subject))
        {
            List<ISkillObserver> observers = subject.Observers;
            if (observers.Count == 0) return;
            foreach (var observer in observers)
            {
                BaseController creature = observer as BaseController;
                if (creature.Id == animPacket.ObjectId) creature.OnAnimSpeedUpdated(animPacket.Param);
            }
        }
    }
    
    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        
    }

    public static void S_SkillUpgradeHandler(PacketSession session, IMessage packet)
    {
        S_SkillUpgrade upgradePacket = (S_SkillUpgrade)packet;
        GameObject uigo = GameObject.FindWithTag("UI");
        if (!uigo.TryGetComponent(out UI_Game ui)) return;
        
        var skillButton = Util.FindChild(ui.gameObject,
            string.Concat(upgradePacket.Skill.ToString(), "Button"), true);
        Util.SetAlpha(skillButton.GetComponent<Image>(), 1.0f);
    }
    
    public static void S_SkillUpdateHandler(PacketSession session, IMessage packet)
    {
        S_SkillUpdate updatePacket = (S_SkillUpdate)packet;
        GameObject go = GameObject.FindWithTag("SkillSubject");
        if (go.TryGetComponent(out SkillSubject subject))
        {
            subject.SkillUpdated(updatePacket.ObjectEnumId, updatePacket.ObjectType, updatePacket.SkillType, updatePacket.Step);
        }    
    }

    public static void S_PortraitUpgradeHandler(PacketSession session, IMessage packet)
    {
        var upgradePacket = (S_PortraitUpgrade)packet;
        var unitName = upgradePacket.UnitId.ToString();
        var ui = GameObject.FindWithTag("UI").GetComponent<UI_Game>();
        var mediator = ui.Mediator;
        var portrait = mediator.CurrentSelectedPortrait;
        var uiPortrait = portrait.GetComponent<UI_Portrait>();
        
        uiPortrait.InitSkillPanel(portrait);
        uiPortrait.UnitId = upgradePacket.UnitId;
        uiPortrait.InitSkillPanel(portrait, true);
        ui.SetUpgradeButton(portrait);
        portrait.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>($"Sprites/Portrait/{unitName}");
    }

    public static void S_UnitUpgradeHandler(PacketSession session, IMessage packet)
    {
        
    }

    public static void S_GetDamageHandler(PacketSession session, IMessage packet)
    {   
        S_GetDamage damagePacket = (S_GetDamage)packet;
        GameObject go = Managers.Object.FindById(damagePacket.ObjectId);
        if (go == null) return;
        // Floating text instantiate
        GameObject floatingText = Managers.Resource.Instantiate("WorldObjects/DmgText");
        floatingText.transform.position = go.transform.position + Vector3.up;
        floatingText.GetComponentInChildren<TextAnimatorPlayer>().ShowText($"{damagePacket.Damage}");
        var text = floatingText.GetComponentInChildren<TextMeshPro>();
        switch (damagePacket.DamageType)
        {
            case Damage.Normal:
                text.color = new Color32(255, 114, 4, 255);
                text.outlineColor = new Color32(255, 30, 0, 255);
                break;
            case Damage.Magical:
                text.color = new Color32(0, 255, 245, 255);
                text.outlineColor = new Color32(0, 35, 255, 255);
                break;
            case Damage.Poison:
                text.color = new Color32(177, 0, 255, 255);
                text.outlineColor = new Color32(57, 0, 255, 255);
                break;
            case Damage.True:
            case Damage.None:
            case Damage.Fire:
            default:
                text.color = new Color32(255, 255, 255, 255);
                text.outlineColor = Color.black;
                break;
        }
        
        go.TryGetComponent(out CreatureController cc);
        if (cc != null) cc.Hp -= damagePacket.Damage;
    }
    
    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp hpPacket = (S_ChangeHp)packet;
        GameObject go = Managers.Object.FindById(hpPacket.ObjectId);
        if (go == null) return;
        // Change hp bar
        go.TryGetComponent(out CreatureController cc);
        if (cc != null) cc.Hp = hpPacket.Hp;
    }

    public static void S_ChangeMaxHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeMaxHp maxHpPacket = (S_ChangeMaxHp)packet;
        GameObject go = Managers.Object.FindById(maxHpPacket.ObjectId);
        if (go == null) return;
        go.TryGetComponent(out CreatureController cc);
        if (cc != null) cc.MaxHp = maxHpPacket.MaxHp;
    }
    
    public static void S_ChangeMpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeMp mpPacket = (S_ChangeMp)packet;
        GameObject go = Managers.Object.FindById(mpPacket.ObjectId);
        if (go == null) return;
        go.TryGetComponent(out CreatureController cc);
        if (cc != null) cc.Mp = mpPacket.Mp;
    }

    public static void S_ChangeSpeedHandler(PacketSession session, IMessage packet)
    {
        var speedPacket = (S_ChangeSpeed)packet;
        var go = Managers.Object.FindById(speedPacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out CreatureController cc) == false) return;
        cc.TotalMoveSpeed = speedPacket.MoveSpeed;
    }
    
    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = (S_Die)packet;

        GameObject go = Managers.Object.FindById(diePacket.ObjectId);
        if (go == null) return;
        go.GetComponent<CreatureController>().State = State.Die;

        go.TryGetComponent(out CreatureController cc);
        if (cc == null) return;
        cc.Hp = 0;
        if (diePacket.Revive == false) cc.OnDead();
    }
    
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        
    }

    public static void S_UnitSpawnPosHandler(PacketSession session, IMessage packet)
    {
        S_UnitSpawnPos spawnPacket = (S_UnitSpawnPos)packet;
        Managers.Game.PickedButton.GetComponent<Image>().color = spawnPacket.CanSpawn == false ? Color.red : Color.white;
        
        var dragPortrait = Managers.Game.PickedButton.GetComponent<UI_DragPortrait>();
        if (dragPortrait.endDrag == false) return;
        
        Managers.Game.PickedButton.GetComponent<Image>().color = Color.white;
        if (spawnPacket.CanSpawn == false) return;
        
        Vector3 pos = Util.NearestCell(dragPortrait.position);
        bool register = spawnPacket.ObjectType == GameObjectType.Tower;
        string unitName = Managers.Game.PickedButton.GetComponent<Image>().sprite.name;
        C_Spawn cSpawnPacket = new()
        {
            Type = spawnPacket.ObjectType,
            Num = (int)Enum.Parse(typeof(UnitId), unitName),
            PosInfo = new PositionInfo { State = State.Idle, PosX = pos.x, PosY = pos.y, PosZ = pos.z },
            Way = pos.z > 0 ? SpawnWay.North : SpawnWay.South,
            Register = register
        };
        
        Managers.Network.Send(cSpawnPacket);
    }
    
    public static void S_TimeHandler(PacketSession session, IMessage packet)
    {
        S_Time timePacket = (S_Time)packet;
        UI_Game ui = GameObject.FindWithTag("UI").GetComponent<UI_Game>();
        var hourHand = Util.FindChild<RotateHourHand>(ui.gameObject, "HourHandImage", true);
        var roundText = Util.FindChild<TextMeshProUGUI>(ui.gameObject, "RoundText", true);
        var timeText = Util.FindChild<TextMeshProUGUI>(ui.gameObject, "TimeText", true);
        
        if (roundText.text != string.Concat("R", timePacket.Round.ToString())) hourHand.InitializingRotation();
        hourHand.RotationStart = true;
        roundText.text = string.Concat("R", timePacket.Round.ToString());
        timeText.text = string.Concat(timePacket.Time.ToString());
    }

    public static void S_SetTextUIHandler(PacketSession session, IMessage packet)
    {
        var uiPacket = (S_SetTextUI)packet;
        var ui = GameObject.FindWithTag("UI").GetComponent<UI_Game>();
        var textName = uiPacket.TextUI.ToString();
        var textUI = Util.FindChild(ui.gameObject, textName, true).GetComponent<TextMeshProUGUI>();
        if (textUI.text.Contains("/"))
        {
            const string regularExpression = @"^(\d{1,2})/(\d{1,2})$";
            var match = Regex.Match(textUI.text, regularExpression);
            if (!match.Success) return;
            
            string group1 = match.Groups[1].Value;
            string group2 = match.Groups[2].Value;
            string str = uiPacket.Max ? group2 : group1;
            
            if (group1 == group2)
            {
                textUI.text = uiPacket.Max 
                    ? textUI.text.Replace("/" + group2, "/" + uiPacket.Value) 
                    : textUI.text.Replace(group1 + "/", uiPacket.Value + "/");
            }
            else
            {
                textUI.text = string.Concat(textUI.text.Replace(str, uiPacket.Value.ToString()));
            }
        }
        else
        {
            textUI.text = uiPacket.Value.ToString();
        }
    }

    public static void S_RegisterInSlotHandler(PacketSession session, IMessage packet)
    {
        var registerPacket = (S_RegisterInSlot)packet;
        var ui = GameObject.FindWithTag("UI").GetComponent<UI_Game>();
        ui.RegisterInSlot(registerPacket);
    }

    public static void S_RegisterMonsterInSlotHandler(PacketSession session, IMessage packet)
    {
        var registerPacket = (S_RegisterMonsterInSlot)packet;
        var ui = GameObject.FindWithTag("UI").GetComponent<UI_Game>();
        ui.RegisterMonsterInSlot(registerPacket);
    }    

    public static void S_UpgradeSlotHandler(PacketSession session, IMessage packet)
    {
        var upgradePacket = (S_UpgradeSlot)packet;
        var ui = GameObject.FindWithTag("UI").GetComponent<UI_Game>();
        ui.UpgradeSlot(upgradePacket);
    }

    public static void S_SetUpgradePopupHandler(PacketSession session, IMessage packet)
    {
        var popupPacket = (S_SetUpgradePopup)packet;
        var popupList = Managers.UI.PopupList;
        foreach (var popup in popupList.Where(p => p is UI_UpgradePopup))
        {
            popup.GetComponent<UI_UpgradePopup>().SetPopup(popupPacket);
        }
    }

    public static void S_SetUpgradeButtonHandler(PacketSession session, IMessage packet)
    {
        var upgradePacket = (S_SetUpgradeButton)packet;
        var ui = GameObject.FindWithTag("UI").GetComponent<UI_Game>();
        ui.SetUpgradeButtonText(upgradePacket.Cost);
    }
    
    public static void S_SendWarningInGameHandler(PacketSession session, IMessage packet)
    {
        var popupPacket = (S_SendWarningInGame)packet;
        var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
        popup.GetComponent<UI_WarningPopup>().SetPopup(popupPacket.Warning);
    }

    public static void S_ShowResultSceneHandler(PacketSession session, IMessage packet)
    {
        var resultPacket = (S_ShowResultScene)packet;
        Managers.Game.GameResult = resultPacket.Win;
        SceneManager.LoadScene("Scenes/Result");
        Managers.Clear();
    }

    public static void S_ShowResultPopupHandler(PacketSession session, IMessage packet)
    {
        var resultPacket = (S_ShowResultPopup)packet;
        Managers.Game.GameResult = resultPacket.Win;
        var popup = Managers.UI.ShowPopupUI<UI_ResultPopup>();
        popup.GetComponent<UI_ResultPopup>().SetPopup();

    }
}