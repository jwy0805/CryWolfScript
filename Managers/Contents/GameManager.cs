using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager
{
    public GameObject PickedButton;
    public Action<int> OnSpawnEvent;
    public bool GameResult { get; set; }
    
    private GameObject _player;
    private GameObject _sheep;
    private readonly Dictionary<UnitId, GameObject> _units = new();
    
    public void Spawn(GameObjectType type)
    {
        var eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        if (results.Any(result => result.gameObject.layer == LayerMask.NameToLayer("UI"))) return;
        
        var dragPortrait = PickedButton.GetComponent<UI_DragPortrait>();
        if (dragPortrait == null) return;
        
        var pos = Util.NearestCell(dragPortrait.Position);
        var register = type == GameObjectType.Tower;
        var unitName = Managers.Game.PickedButton.GetComponent<Image>().sprite.name;
        
        C_Spawn cSpawnPacket = new()
        {
            Type = type,
            Num = (int)Enum.Parse(typeof(UnitId), unitName),
            PosInfo = new PositionInfo { State = State.Idle, PosX = pos.x, PosY = pos.y, PosZ = pos.z },
            Way = Managers.Map.MapId == 1 ? SpawnWay.North : pos.z > 0 ? SpawnWay.North : SpawnWay.South,
            Register = register
        };
        Managers.Network.Send(cSpawnPacket);
    }
    
    public GameObject Spawn(GameObjectType type, string path, Transform parent = null)
    {
        var go = Managers.Resource.Instantiate(path, parent);
        return go;
    }
    
    public GameObject Spawn(GameObjectType type, string path, Vector3 position)
    {
        var go = Managers.Resource.Instantiate(path, position);
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
