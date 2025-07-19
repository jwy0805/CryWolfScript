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
using Zenject;
using State = Google.Protobuf.Protocol.State;

public class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        var enterGamePacket = (S_EnterGame)packet;
        if (enterGamePacket != null)
        {
            Managers.Object.Add(enterGamePacket.Player, true);
        }
    }
    
    public static void S_ConnectSessionHandler(PacketSession session, IMessage packet)
    {
        var connectPacket = (S_ConnectSession)packet;
        Managers.Network.SessionId = connectPacket.SessionId;
        Debug.Log(connectPacket.SessionId);
    }
    
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        var leaveGamePacket = (S_LeaveGame)packet;
        var type = ObjectManager.GetObjectTypeById(leaveGamePacket.ObjectId);
        switch (type)
        {
            case GameObjectType.Effect:
                var go = Managers.Object.FindById(leaveGamePacket.ObjectId);
                Managers.Resource.Destroy(go);
                Managers.Object.Remove(leaveGamePacket.ObjectId);
                break;
            case GameObjectType.Player:
                Managers.Object.Remove(leaveGamePacket.ObjectId);
                Managers.Object.RemoveMyPlayer();
                break;
        }
    }
    
    public static void S_StepTutorialHandler(PacketSession session, IMessage packet)
    {
        var stepPacket = (S_StepTutorial)packet;
        var sceneContext = UnityEngine.Object.FindAnyObjectByType<SceneContext>();
        if (sceneContext == null) return;
        
        var tutorialVm = sceneContext.Container.Resolve<TutorialViewModel>();
        if (tutorialVm == null) return;
        
        if (stepPacket.Step != 0)
        {
            tutorialVm.Step = stepPacket.Step;
        }
            
        if (stepPacket.Process == false)
        {
            // Show new window
            _ = tutorialVm.ShowTutorialPopup();
            // ui.SetTutorialUI();
        }
        else
        {
            // Just step tutorial
            tutorialVm.StepTutorial();
        }
    }
    
    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        var spawnPacket = (S_Spawn)packet;
        foreach (var obj in spawnPacket.Objects)
        {
            Managers.Object.Add(obj);
        }
    }

    public static void S_BindStatueInfoHandler(PacketSession session, IMessage packet)
    {
        var bindPacket = (S_BindStatueInfo)packet;
        var gameObject = Managers.Object.FindById(bindPacket.StatueId);
        var cc = gameObject?.GetComponent<CreatureController>();
        if (cc == null) return;
        cc.UnitId = bindPacket.UnitId;
    }
    
    public static void S_SpawnProjectileHandler(PacketSession session, IMessage packet)
    {
        var spawnPacket = (S_SpawnProjectile)packet;
        Managers.Object.AddProjectile(
            spawnPacket.Object, spawnPacket.ParentId, spawnPacket.DestPos, spawnPacket.MoveSpeed);
    }
    
    public static void S_SpawnEffectHandler(PacketSession session, IMessage packet)
    {
        var spawnPacket = (S_SpawnEffect)packet;
        Managers.Object.AddEffect(
            spawnPacket.Object, spawnPacket.ParentId, spawnPacket.TrailingParent, spawnPacket.Duration);
    }
    
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        var despawnPacket = (S_Despawn)packet;
        foreach (var id in despawnPacket.ObjectIds)
        {
            var go = Managers.Object.FindById(id);
            Managers.Object.Remove(id);
            Managers.Resource.Destroy(go);
        }
    }
    
    public static void S_PlayerMoveHandler(PacketSession session, IMessage packet)
    {
        var playerMovePacket = (S_PlayerMove)packet;
        var go = Managers.Object.FindById(playerMovePacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out MyPlayerController pc) == false) return;

        pc.DestPos = new Vector3(playerMovePacket.DestPos.X, playerMovePacket.DestPos.Y, playerMovePacket.DestPos.Z);
        pc.State = playerMovePacket.State;
    }
    
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        var movePacket = (S_Move)packet;
        var go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out BaseController bc) == false) return;
        
        bc.PosInfo = movePacket.PosInfo;
    }

    public static void S_InstantMoveHandler(PacketSession session, IMessage packet)
    {
        var movePacket = (S_InstantMove)packet;
        var go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out BaseController bc) == false) return;

        bc.transform.position = new Vector3(movePacket.Dest.X, movePacket.Dest.Y, movePacket.Dest.Z);
    }
    
    public static void S_StateHandler(PacketSession session, IMessage packet)
    {
        var statePacket = (S_State)packet;
        var go = Managers.Object.FindById(statePacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out CreatureController cc) == false) return;
        
        cc.State = statePacket.State;
    }

    public static void S_SyncHandler(PacketSession session, IMessage packet)
    {
        var syncPacket = (S_Sync)packet;
        var go = Managers.Object.FindById(syncPacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out CreatureController cc) == false) return;

        cc.SyncPos = new Vector3(syncPacket.PosInfo.PosX, syncPacket.PosInfo.PosY, syncPacket.PosInfo.PosZ);
        cc.PosInfo.Dir = syncPacket.PosInfo.Dir;
    }    
    
    public static void S_SetPathHandler(PacketSession session, IMessage packet)
    {
        var pathPacket = (S_SetPath)packet;
        var go = Managers.Object.FindById(pathPacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out CreatureController cc) == false) return;
        
        cc.OnPathReceived(pathPacket);
    }
    
    public static void S_SetKnockBackHandler(PacketSession session, IMessage packet)
    {
        var knockBackPacket = (S_SetKnockBack)packet;
        var go = Managers.Object.FindById(knockBackPacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out BaseController bc) == false) return;
        
        Vector3 v = new Vector3(knockBackPacket.Dest.X, knockBackPacket.Dest.Y, knockBackPacket.Dest.Z);
        bc.DestPos = v;
        bc.SetKnockBackDest = true;
    }
    
    public static void S_SetDestSkillHandler(PacketSession session, IMessage packet)
    {
        var destSkillPacket = (S_SetDestSkill)packet;
        var go = Managers.Object.FindById(destSkillPacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out ProjectileController pc) == false) return;
        if (destSkillPacket.Dest == null) return;

        Vector3 dest = new Vector3(destSkillPacket.Dest.X, destSkillPacket.Dest.Y, destSkillPacket.Dest.Z);
        pc.DestPos = dest;
    }

    public static void S_SetDestResourceHandler(PacketSession session, IMessage packet)
    {
        var resourceDestPacket = (S_SetDestResource)packet;
        var go = Managers.Object.FindById(resourceDestPacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out ResourceController rc) == false) return;
        if (resourceDestPacket.Dest == null) return;

        Vector3 dest = new Vector3(resourceDestPacket.Dest.X, resourceDestPacket.Dest.Y, resourceDestPacket.Dest.Z);
        rc.DestPos = dest + Vector3.up * 0.5f;
    }

    public static void S_SetAnimSpeedHandler(PacketSession session, IMessage packet)
    {
        var animSpeedPacket = (S_SetAnimSpeed)packet;
        var go = Managers.Object.FindById(animSpeedPacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out BaseController bc) == false) return;
        
        bc.OnAnimSpeedUpdated(animSpeedPacket.SpeedParam);
    }
    
    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        
    }

    public static void S_BaseUpgradeHandler(PacketSession session, IMessage packet)
    {
        var upgradePacket = (S_BaseUpgrade)packet;

        if (upgradePacket.Faction == Faction.Wolf)
        {
            var portal = GameObject.FindWithTag("Portal").GetComponent<PortalController>();
            portal.UpgradePortal(upgradePacket.Level);
        }
        else
        {
            Managers.Object.UpgradeStorage(upgradePacket.Level, upgradePacket.BaseZ);
        }
    }
    
    public static void S_SkillUpgradeHandler(PacketSession session, IMessage packet)
    {
        var upgradePacket = (S_SkillUpgrade)packet;
        var uiObject = GameObject.FindWithTag("UI");
        if (uiObject.TryGetComponent(out UI_GameSingleWay ui) == false) return;
        ui.UpgradeSkill(upgradePacket.Skill);
    }
    
    public static void S_SkillUpdateHandler(PacketSession session, IMessage packet)
    {
        var updatePacket = (S_SkillUpdate)packet;
        var go = GameObject.FindWithTag("SkillSubject");
        if (go.TryGetComponent(out SkillSubject subject) == false) return;
        
        subject.SkillUpdated(
            updatePacket.ObjectEnumId, updatePacket.ObjectType, updatePacket.SkillType, updatePacket.Step);
    }

    public static async void S_PortraitUpgradeHandler(PacketSession session, IMessage packet)
    {
        try
        {
            var upgradePacket = (S_PortraitUpgrade)packet;
            var ui = GameObject.FindWithTag("UI").GetComponent<UI_GameSingleWay>();
            await ui.UpgradePortrait(upgradePacket.UnitId.ToString());
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public static async void S_GetDamageHandler(PacketSession session, IMessage packet)
    {
        try
        {
            var damagePacket = (S_GetDamage)packet;
            var go = Managers.Object.FindById(damagePacket.ObjectId);
            if (go == null) return;
            // Floating text instantiate
            var floatingTextObject = await Managers.Resource.Instantiate("WorldObjects/DmgText");
            var text = floatingTextObject.GetComponentInChildren<TextMeshPro>();
            var typeWriter = floatingTextObject.GetComponentInChildren<TypewriterByCharacter>();
            floatingTextObject.transform.position = go.transform.position + Vector3.up;
            typeWriter.ShowText($"{damagePacket.Damage}");
            switch (damagePacket.DamageType)
            {
                case Damage.Normal:
                    text.color = new Color32(255, 114, 4, 255);
                    // text.outlineColor = new Color32(255, 30, 0, 255);
                    break;
                case Damage.Magical:
                    text.color = new Color32(0, 255, 245, 255);
                    // text.outlineColor = new Color32(0, 35, 255, 255);
                    break;
                case Damage.Poison:
                    text.color = new Color32(177, 0, 255, 255);
                    // text.outlineColor = new Color32(57, 0, 255, 255);
                    break;
                case Damage.True:
                    text.color = new Color32(255, 255, 186, 255);
                    break;
                case Damage.None:
                case Damage.Fire:
                default:
                    text.color = new Color32(255, 255, 255, 255);
                    text.outlineColor = Color.black;
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        var hpPacket = (S_ChangeHp)packet;
        var go = Managers.Object.FindById(hpPacket.ObjectId);
        if (go == null) return;
        // Change hp bar
        if (go.TryGetComponent(out CreatureController cc) == false) return;

        cc.MaxHp = hpPacket.MaxHp;
        cc.Hp = hpPacket.Hp;
    }

    public static void S_ChangeShieldHandler(PacketSession session, IMessage packet)
    {
        var shieldPacket = (S_ChangeShield)packet;
        var go = Managers.Object.FindById(shieldPacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out CreatureController cc) == false) return;
        
        cc.ShieldAdd = shieldPacket.ShieldAdd;
        cc.ShieldRemain = shieldPacket.ShieldRemain;
    }
    
    public static void S_ChangeMpHandler(PacketSession session, IMessage packet)
    {
        var mpPacket = (S_ChangeMp)packet;
        var go = Managers.Object.FindById(mpPacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out CreatureController cc) == false) return;

        cc.MaxMp = mpPacket.MaxMp;
        cc.Mp = mpPacket.Mp;
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
        var diePacket = (S_Die)packet;
        var go = Managers.Object.FindById(diePacket.ObjectId);
        if (go == null) return;
        if (go.TryGetComponent(out CreatureController cc) == false) return;

        cc.Hp = 0;
        cc.State = State.Die;
        if (diePacket.Revive == false && go.GetComponent<BaseController>().ObjectType != GameObjectType.Tower)
        {
            cc.OnDead();
        }
    }
    
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        
    }

    public static void S_UnitSpawnPosHandler(PacketSession session, IMessage packet)
    {
        Managers.Event.TriggerEvent("CanSpawn", packet);
    }
    
    public static void S_GetRangesHandler(PacketSession session, IMessage packet)
    {
        Managers.Event.TriggerEvent("ShowRings", packet);
    }
    
    public static void S_GetSpawnableBoundsHandler(PacketSession session, IMessage packet)
    {
        Managers.Event.TriggerEvent("ShowBounds", packet);
    }
    
    public static void S_TimeHandler(PacketSession session, IMessage packet)
    {
        var timePacket = (S_Time)packet;
        var ui = GameObject.FindWithTag("UI").GetComponent<UI_Game>();
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
        var mapId = Managers.Map.MapId; 
        UI_Game ui = mapId == 1 
            ? GameObject.FindWithTag("UI").GetComponent<UI_GameSingleWay>() 
            : GameObject.FindWithTag("UI").GetComponent<UI_GameDoubleWay>();
        var textName = uiPacket.TextUI.ToString();
        var textUI = Util.FindChild(ui.gameObject, textName, true).GetComponent<TextMeshProUGUI>();
        if (textUI.text.Contains("/"))
        {
            // About capacity text
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
            break;
        }

        foreach (var popup in popupList.Where(p => p is UI_UpgradePopupNoCost))
        {
            popup.GetComponent<UI_UpgradePopupNoCost>().SetPopup(popupPacket);
            break;
        }
    }

    public static void S_SetUpgradeButtonCostHandler(PacketSession session, IMessage packet)
    {
        var upgradePacket = (S_SetUpgradeButtonCost)packet;
        var ui = GameObject.FindWithTag("UI").GetComponent<UI_GameSingleWay>();
        ui.UpdateUpgradeCost(upgradePacket.Cost);
    }
    
    public static void S_SetUnitUpgradeCostHandler(PacketSession session, IMessage packet)
    {
        Managers.Event.TriggerEvent("UpdateUnitUpgradeCost", packet);
    }
    
    public static void S_SetUnitDeleteCostHandler(PacketSession session, IMessage packet)
    {
        Managers.Event.TriggerEvent("UpdateUnitDeleteCost", packet);
    }
    
    public static void S_SetUnitRepairCostHandler(PacketSession session, IMessage packet)
    {
        Managers.Event.TriggerEvent("UpdateUnitRepairCost", packet);
    }
    
    public static void S_SetBaseSkillCostHandler(PacketSession session, IMessage packet)
    {
        Managers.Event.TriggerEvent("SetBaseSkillCost", packet);
    }
    
    public static async void S_SendWarningInGameHandler(PacketSession session, IMessage packet)
    {
        try
        {
            var popupPacket = (S_SendWarningInGame)packet;
            var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
            await Managers.Localization.UpdateWarningPopupText(popup, popupPacket.MessageKey);
        
            var sceneContext = UnityEngine.Object.FindAnyObjectByType<SceneContext>();
            var gameViewModel = sceneContext.Container.TryResolve<GameViewModel>();
            gameViewModel?.WarningHandler(popupPacket.MessageKey);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public static async void S_ShowRankResultPopupHandler(PacketSession session, IMessage packet)
    {
        try
        {
            var resultPacket = (S_ShowRankResultPopup)packet;
            Managers.UI.CloseAllPopupUI();
            Managers.Game.GameResult = resultPacket.Win;
        
            if (resultPacket.Win)
            {
                var popup = await Managers.UI.ShowPopupUI<UI_ResultVictoryPopup>();
                popup.RankPointValue = resultPacket.RankPointValue;
                popup.RankPoint = resultPacket.RankPoint;
                popup.Reward = resultPacket.Rewards.ToList();
            }
            else
            {
                var popup = await Managers.UI.ShowPopupUI<UI_ResultDefeatPopup>();
                popup.RankPointValue = resultPacket.RankPointValue;
                popup.RankPoint = resultPacket.RankPoint;
                popup.Reward = resultPacket.Rewards.ToList();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public static async void S_ShowSingleResultPopupHandler(PacketSession session, IMessage packet)
    {
        try
        {
            var resultPacket = (S_ShowSingleResultPopup)packet;
            Managers.UI.CloseAllPopupUI();
            Managers.Game.GameResult = resultPacket.Win;
        
            if (resultPacket.Win)
            {
                var popup = await Managers.UI.ShowPopupUI<UI_ResultSingleVictoryPopup>();
                popup.Star = resultPacket.Star;
                popup.Reward = resultPacket.SingleRewards.Select(sr => new Reward
                {
                    ItemId = sr.ItemId, ProductType = sr.ProductType, Count = sr.Count
                }).ToList();
            }
            else
            {
                await Managers.UI.ShowPopupUI<UI_ResultSingleDefeatPopup>();
            }
        }
        catch (Exception e)
        {
            
            Debug.LogWarning(e);
        }
    }
    
    public static void S_SendTutorialRewardHandler(PacketSession session, IMessage packet)
    {
        var rewardPacket = (S_SendTutorialReward)packet;
        var sceneContext = UnityEngine.Object.FindAnyObjectByType<SceneContext>();
        if (sceneContext == null) return;
        
        var tutorialVm = sceneContext.Container.Resolve<TutorialViewModel>();
        if (tutorialVm == null) return;

        tutorialVm.Step = Util.Faction == Faction.Wolf ? 18 : 22;
        tutorialVm.SetTutorialReward(rewardPacket.RewardUnitId);
        
        _ = tutorialVm.ShowTutorialPopup();
    }
    
    public static async void S_MatchMakingSuccessHandler(PacketSession session, IMessage packet)
    {
        try
        {
            var matchMakingPacket = (S_MatchMakingSuccess)packet;
            var ui = GameObject.FindWithTag("UI").GetComponent<UI_MatchMaking>();
            await ui.SetEnemyUserInfo(matchMakingPacket);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
}