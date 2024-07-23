using System;
using System.Collections.Generic;
using Cinemachine;
using Google.Protobuf.Protocol;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    private Dictionary<int, GameObject> _objects = new();

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
                bool isSheep = Util.Camp == Camp.Sheep;
                Managers.Network.Send(new C_EnterGame { IsSheep = isSheep });
                if (myPlayer)
                {
                    go = Managers.Game.Spawn(GameObjectType.Player, "PlayerCharacter");
                    go.AddComponent<MyPlayerController>();
                    go.transform.position = new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ);
                    go.name = info.Name;
                    _objects.Add(info.ObjectId, go);
                    
                    MyPlayer = go.GetComponent<MyPlayerController>();
                    MyPlayer.Id = info.ObjectId;
                    MyPlayer.PosInfo = info.PosInfo;
                    MyPlayer.Camp = isSheep ? Camp.Sheep : Camp.Wolf;
                    
                    Vector3 pos = go.transform.position;
                    MyPlayer.PosInfo.PosX = pos.x;
                    MyPlayer.PosInfo.PosY = pos.y;
                    MyPlayer.PosInfo.PosZ = pos.z;
                    
                    UI_Game ui = GameObject.FindWithTag("UI").GetComponent<UI_Game>();
                    ui.Player = MyPlayer;
                    Managers.Network.Send(new C_SetTextUI { Init = true });
                    
                    GameObject virtualCamera = GameObject.Find("FollowCam");
                    CinemachineVirtualCamera followCam = virtualCamera.GetComponent<CinemachineVirtualCamera>();
                    
                    GameObject cameraFocus = Managers.Resource.Instantiate("CameraFocus");
                    cameraFocus.transform.position = new Vector3(pos.x, pos.y, pos.z < 0 ? pos.z + 10 : pos.z - 10);
                    
                    var tf = cameraFocus.transform;
                    followCam.Follow = tf;
                    followCam.LookAt = tf;
                    
                    if (Util.Camp == Camp.Wolf)
                    {
                        MyPlayer.transform.rotation = Quaternion.Euler(0, 180, 0);
                        var cinemachineTransposer = followCam.GetCinemachineComponent<CinemachineTransposer>();
                        cinemachineTransposer.m_FollowOffset = new Vector3(0, 15, 10);
                    }
                }
                else
                {
                    go = Managers.Game.Spawn(GameObjectType.Player, "PlayerCharacter");
                    go.AddComponent<PlayerController>();
                    go.name = info.Name;
                    _objects.Add(info.ObjectId, go);
                    
                    PlayerController pc = go.GetComponent<PlayerController>();
                    pc.Id = info.ObjectId;
                    pc.PosInfo = info.PosInfo;
                }
                break;
            
            case GameObjectType.Tower:
                go = Managers.Game.Spawn(GameObjectType.Tower, $"Towers/{info.Name}");
                _objects.Add(info.ObjectId, go);
                var towerPos = info.PosInfo;
                go.transform.position = new Vector3(towerPos.PosX, towerPos.PosY, towerPos.PosZ);
                if (go.TryGetComponent(out TowerController tc) == false) return;
                tc.Id = info.ObjectId;
                tc.PosInfo = towerPos;
                tc.Stat = info.StatInfo; 
                
                // if (go.TryGetComponent(out UI_HealthCircle healthCircle)) healthCircle.TowerSpawned(stat.SizeX);
                // if (go.TryGetComponent(out UI_CanSpawn canSpawn))
                // {
                //     canSpawn.background.color = Color.clear;
                //     canSpawn.enabled = false;
                // }
                // if (go.TryGetComponent(out Drag drag)) drag.enabled = false;
                break;
            
            case GameObjectType.Monster:
                go = Managers.Game.Spawn(GameObjectType.Monster, $"Monsters/{info.Name}");
                _objects.Add(info.ObjectId, go);
                var monPos = info.PosInfo;
                go.transform.position = new Vector3(monPos.PosX, monPos.PosY, monPos.PosZ);
                if (go.TryGetComponent(out MonsterController mc) == false) return; 
                mc.Id = info.ObjectId;
                mc.PosInfo = monPos;
                mc.Stat = info.StatInfo;
                break;
            
            case GameObjectType.MonsterStatue:
                go = Managers.Game.Spawn(GameObjectType.MonsterStatue, $"Statues/{info.Name}");
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
                go = Managers.Game.Spawn(GameObjectType.Fence, $"{info.Name}");
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
                go = Managers.Game.Spawn(GameObjectType.Sheep, $"{info.Name}");
                _objects.Add(info.ObjectId, go);
                PositionInfo sheepPos = info.PosInfo;
                go.transform.position = new Vector3(sheepPos.PosX, sheepPos.PosY, sheepPos.PosZ);
                go.transform.rotation = Quaternion.Euler(0, info.PosInfo.Dir, 0);
                SheepController sc = go.GetComponent<SheepController>();
                sc.Id = info.ObjectId;
                sc.PosInfo = info.PosInfo;
                sc.Stat = info.StatInfo;
                break;
            
            case GameObjectType.Effect:
                go = Managers.Game.Spawn(GameObjectType.Effect, $"Effects/{info.Name}");
                _objects.Add(info.ObjectId, go);
                go.transform.position = new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ);
                break;
            
            case GameObjectType.Projectile:
                go = Managers.Game.Spawn(GameObjectType.Projectile, $"Effects/{info.Name}");
                _objects.Add(info.ObjectId, go);
                if (go.TryGetComponent(out ProjectileController prc))
                {
                    prc.Id = info.ObjectId;
                    prc.transform.position = new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ);
                }
                break;
            
            case GameObjectType.Resource:
                go = Managers.Game.Spawn(GameObjectType.Resource, $"Items/{info.Name}");
                _objects.Add(info.ObjectId, go);
                ResourceController rc = go.GetComponent<ResourceController>();
                rc.Id = info.ObjectId;
                go.transform.position = new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ);
                break;
            
            case GameObjectType.Portal:
                go = Managers.Game.Spawn(GameObjectType.Portal, $"{info.Name}");
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
        var go = Managers.Game.Spawn(GameObjectType.Projectile, $"Effects/{info.Name}", 
            new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ));
        _objects.Add(info.ObjectId, go);
        if (go.TryGetComponent(out ProjectileController prc) == false) return;
        prc.Id = info.ObjectId;
        // prc.transform.position = new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ);
        prc.DestPos = new Vector3(destVector.X, destVector.Y, destVector.Z);
        prc.Speed = speed;
    }

    public void AddEffect(ObjectInfo info, int parentId, bool trailingParent, int duration)
    {
        var parent = FindById(parentId);
        GameObject go = Managers.Game.Spawn(GameObjectType.Effect, $"Effects/{info.Name}");
        _objects.Add(info.ObjectId, go);
        if (go.TryGetComponent(out EffectController ec) == false) return; 
        ec.Id = info.ObjectId;
        ec.Parent = parent;
        ec.TrailingParent = trailingParent;
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
