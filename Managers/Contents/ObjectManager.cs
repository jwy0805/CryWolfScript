using System;
using System.Collections.Generic;
using Cinemachine;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    private readonly Dictionary<int, GameObject> _objects = new();

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }
    
    public void Add(ObjectInfo info, bool myPlayer = false)
    {
        GameObjectType objectType = GetObjectTypeById(info.ObjectId);
        GameObject go;
        
        switch (objectType)
        {
            case GameObjectType.Player:

                go = Managers.Game.Spawn("PlayerCharacter");
                if (myPlayer)
                {
                    var isSheep = Util.Faction == Faction.Sheep;
                    var battleSetting = User.Instance.BattleSetting;
                    var enterPacket = new C_EnterGame
                    {
                        IsSheep = isSheep,
                        CharacterId = battleSetting.CharacterInfo.Id,
                        AssetId = isSheep ? battleSetting.SheepInfo.Id : battleSetting.EnchantInfo.Id
                    };
                
                    Managers.Network.Send(enterPacket);
                    
                    var controller = go.AddComponent<MyPlayerController>();
                    Object.FindObjectOfType<SceneContext>().Container.Inject(controller);
                    
                    go.transform.position = new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ);
                    go.name = info.Name;
                    _objects.Add(info.ObjectId, go);
                    
                    MyPlayer = go.GetComponent<MyPlayerController>();
                    MyPlayer.Id = info.ObjectId;
                    MyPlayer.PosInfo = info.PosInfo;
                    MyPlayer.Faction = isSheep ? Faction.Sheep : Faction.Wolf;
                    
                    Vector3 pos = go.transform.position;
                    MyPlayer.PosInfo.PosX = pos.x;
                    MyPlayer.PosInfo.PosY = pos.y;
                    MyPlayer.PosInfo.PosZ = pos.z;

                    var obj = GameObject.FindWithTag("UI");
                    var ui = obj.GetComponent<UI_GameSingleWay>();
                    ui.Player = MyPlayer;
                    Managers.Network.Send(new C_SetTextUI { Init = true });
                    
                    var followCam = GameObject.Find("FollowCam").GetComponent<CinemachineVirtualCamera>();
                    var cameraFocus = Managers.Resource.Instantiate("CameraFocus");
                    cameraFocus.transform.position = new Vector3(pos.x, pos.y, pos.z < 0 ? pos.z + 10 : pos.z - 10);
                    
                    var tf = cameraFocus.transform;
                    followCam.Follow = tf;
                    followCam.LookAt = tf;
                    
                    if (Util.Faction == Faction.Wolf)
                    {
                        var cameraRotation = followCam.transform.rotation.eulerAngles;
                        cameraRotation.y += 180f;
                        followCam.transform.rotation = Quaternion.Euler(cameraRotation);
                        MyPlayer.transform.rotation = Quaternion.Euler(0, 180, 0);
                        cameraFocus.transform.rotation = Quaternion.Euler(0, 180, 0);
                        
                        var cinemachineTransposer = followCam.GetCinemachineComponent<CinemachineTransposer>();
                        cinemachineTransposer.m_FollowOffset = new Vector3(0, 15, 10);
                    }
                }
                else
                {
                    var isSheep = Util.Faction != Faction.Sheep;
                    var enterPacket = new C_EnterGameNpc
                    {
                        IsSheep = isSheep,
                        CharacterId = 2001,
                        AssetId = isSheep ? 901 : 1001
                    };
                
                    Managers.Network.Send(enterPacket);
                    
                    go.AddComponent<PlayerController>();
                    go.name = info.Name;
                    _objects.Add(info.ObjectId, go);
                    
                    var pc = go.GetComponent<PlayerController>();
                    pc.Id = info.ObjectId;
                    pc.PosInfo = info.PosInfo;
                    pc.Faction = isSheep ? Faction.Sheep : Faction.Wolf;
                    pc.SetPosition();
                }
                
                break;
            
            case GameObjectType.Tower:
                go = Managers.Game.Spawn($"Towers/{info.Name}");
                _objects.Add(info.ObjectId, go);
                var towerPos = info.PosInfo;
                go.transform.position = new Vector3(towerPos.PosX, towerPos.PosY, towerPos.PosZ);
                if (go.TryGetComponent(out TowerController tc) == false) return;
                tc.Id = info.ObjectId;
                tc.PosInfo = towerPos;
                tc.Stat = info.StatInfo; 
                break;
            
            case GameObjectType.Monster:
                go = Managers.Game.Spawn($"Monsters/{info.Name}");
                _objects.Add(info.ObjectId, go);
                var monPos = info.PosInfo;
                go.transform.position = new Vector3(monPos.PosX, monPos.PosY, monPos.PosZ);
                if (go.TryGetComponent(out MonsterController mc) == false) return; 
                mc.Id = info.ObjectId;
                mc.PosInfo = monPos;
                mc.Stat = info.StatInfo;
                break;
            
            case GameObjectType.MonsterStatue:
                go = Managers.Game.Spawn($"Statues/{info.Name}");
                _objects.Add(info.ObjectId, go);
                PositionInfo statuePos = info.PosInfo;
                go.transform.position = new Vector3(statuePos.PosX, statuePos.PosY, statuePos.PosZ);
                go.transform.rotation = Quaternion.Euler(0, info.PosInfo.Dir, 0);
                StatueController msc = go.GetComponent<StatueController>();
                msc.Id = info.ObjectId;
                msc.PosInfo = info.PosInfo;
                msc.Stat = info.StatInfo;
                break;
            
            case GameObjectType.Fence:
                go = Managers.Game.Spawn($"Fences/{info.Name}");
                _objects.Add(info.ObjectId, go);
                PositionInfo fencePos = info.PosInfo;
                go.transform.position = new Vector3(fencePos.PosX, fencePos.PosY, fencePos.PosZ);
                go.transform.rotation = Quaternion.Euler(0, info.PosInfo.Dir, 0);
                FenceController fc = go.GetComponent<FenceController>();
                fc.Id = info.ObjectId;
                fc.PosInfo = info.PosInfo;
                fc.Stat = info.StatInfo;
                break;
            
            case GameObjectType.Sheep:
                go = Managers.Game.Spawn($"Sheep/{info.Name}");
                _objects.Add(info.ObjectId, go);
                PositionInfo sheepPos = info.PosInfo;
                go.transform.position = new Vector3(sheepPos.PosX, sheepPos.PosY, sheepPos.PosZ);
                go.transform.rotation = Quaternion.Euler(0, info.PosInfo.Dir, 0);
                SheepController sc = go.GetComponent<SheepController>();
                sc.Id = info.ObjectId;
                sc.PosInfo = info.PosInfo;
                sc.Stat = info.StatInfo;
                break;
            
            case GameObjectType.Resource:
                go = Managers.Game.Spawn($"Items/{info.Name}");
                _objects.Add(info.ObjectId, go);
                ResourceController rc = go.GetComponent<ResourceController>();
                rc.Id = info.ObjectId;
                go.transform.position = new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ);
                break;
            
            case GameObjectType.Portal:
                go = Managers.Game.Spawn($"Portals/{info.Name}");
                _objects.Add(info.ObjectId, go);
                PositionInfo portalPos = info.PosInfo;
                go.transform.position = new Vector3(portalPos.PosX, portalPos.PosY, portalPos.PosZ);
                go.transform.rotation = Quaternion.Euler(0, info.PosInfo.Dir, 0);
                if (go.TryGetComponent(out PortalController poc))
                {
                    poc.Id = info.ObjectId;
                    poc.PosInfo = info.PosInfo;
                    poc.Stat = info.StatInfo;
                }
                break;
        }
    }
    
    public void AddProjectile(ObjectInfo info, int parentId, DestVector destVector, float speed)
    {   
        var parent = FindById(parentId);
        if (parent == null) return;
        var projectilePos = new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ);
        var go = Managers.Game.Spawn($"Effects/{info.Name}", projectilePos);
        _objects.Add(info.ObjectId, go);
        if (go.TryGetComponent(out ProjectileController prc) == false) return;
        prc.Id = info.ObjectId;
        // prc.transform.position = new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ);
        prc.DestPos = new Vector3(destVector.X, destVector.Y, destVector.Z);
        prc.Speed = speed;
    }

    public void AddEffect(ObjectInfo info, int masterId, bool trailingParent, int duration)
    {
        var master = FindById(masterId);
        var go = Managers.Game.Spawn($"Effects/{info.Name}");
        _objects.Add(info.ObjectId, go);
        if (go.TryGetComponent(out EffectController ec) == false) return; 
        ec.Id = info.ObjectId;
        ec.Master = master;
        ec.TrailingMaster = trailingParent;
        ec.Duration = duration / (float)1000;
        go.transform.position = new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ);
    }

    public void Remove(int id)
    {
        _objects.Remove(id);
    }

    public void RemoveMyPlayer()
    {
        if (MyPlayer == null) return;
        Remove(MyPlayer.Id);
        MyPlayer = null;
    }

    [CanBeNull]
    public GameObject FindById(int id)
    {
        _objects.TryGetValue(id, out var go);
        return go;
    }
    
    public GameObject Find(Vector3 cellPos)
    {
        foreach (var obj in _objects.Values)
        {
            BaseController bc = obj.GetComponent<BaseController>();
            if (bc == null) continue;
            if (bc.CellPos == cellPos) return obj;
        }

        return null;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (var obj in _objects.Values)
        {
            if (condition.Invoke(obj)) return obj;
        }

        return null;
    }

    public void Clear()
    {
        _objects.Clear();
    }
}
