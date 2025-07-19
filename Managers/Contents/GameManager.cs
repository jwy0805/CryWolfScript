using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager
{
    public Action<int> OnSpawnEvent;
    public Queue<RewardInfo> RewardQueue;
    public bool GameResult { get; set; }
    public bool IsTutorial { get; set; }
    
    private GameObject _player;
    private GameObject _sheep;
    private readonly Dictionary<UnitId, GameObject> _units = new();
    
    public void Spawn(UnitId unitId, Vector3 spawnPos)
    {
        var eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        if (results.Any(result => result.gameObject.layer == LayerMask.NameToLayer("UI"))) return;
        
        // Distinction between Tower and MonsterStatue
        var type = unitId - 500 < 0 ? GameObjectType.Tower : GameObjectType.MonsterStatue;
        C_Spawn spawnPacket = new()
        {
            Type = type,
            Num = (int)unitId,
            PosInfo = new PositionInfo { State = State.Idle, PosX = spawnPos.x, PosY = spawnPos.y, PosZ = spawnPos.z },
            Way = Managers.Map.MapId == 1 ? SpawnWay.North : spawnPos.z > 0 ? SpawnWay.North : SpawnWay.South,
            Register = true
        };
        
        Managers.Network.Send(spawnPacket);
    }
    
    public async Task<GameObject> Spawn(string path, Transform parent = null)
    {
        var go = await Managers.Resource.Instantiate(path, parent);
        return go;
    }
    
    public async Task<GameObject> Spawn(string path, Vector3 position)
    {
        var go = await Managers.Resource.Instantiate(path, position);
        return go;
    }
    
    public void Despawn(GameObject go, float time = 0.0f)
    {
        GameObjectType type = GetWorldObjectType(go);
        
        switch (type)
        {
            case GameObjectType.Monster:
            case GameObjectType.Tower:
                var monsterId = GetUnitId(go);
                if (_units.ContainsKey(monsterId))
                {
                    _units.Remove(monsterId);
                    if (OnSpawnEvent != null)
                    {
                        OnSpawnEvent.Invoke(-1);
                    }
                }
                break;
            case GameObjectType.Sheep:
                break;
            case GameObjectType.Player:
                if (_player == go)
                    _player = null;
                break;
        }
        
        Managers.Resource.Destroy(go, time);
    }
    
    public GameObjectType GetWorldObjectType(GameObject go)
    {
        BaseController baseController = go.GetComponent<BaseController>();
        if (baseController == null)
            return GameObjectType.None;

        return baseController.ObjectType;
    }

    public UnitId GetUnitId(GameObject go)
    {
        if (go.TryGetComponent(out MonsterController mc))
        {
            return mc.UnitId;
        }

        if (go.TryGetComponent(out TowerController tc))
        {
            return tc.UnitId;
        }

        return UnitId.UnknownUnit;
    }
}
