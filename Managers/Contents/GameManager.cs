using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager
{
    public GameObject PickedButton;
    public Action<int> OnSpawnEvent;
    public bool GameResult { get; set; }
    
    private GameObject _player;
    private GameObject _sheep;
    private Dictionary<UnitId, GameObject> _units = new();
    
    public void Spawn(UnitId unitId, DestVector dest)
    {
        var eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        if (results.Any(result => result.gameObject.layer == LayerMask.NameToLayer("UI"))) return;
        
        Managers.Network.Send(new C_UnitSpawnPos {UnitId = (int)unitId, DestVector = dest});
    }
    
    public GameObject Spawn(GameObjectType type, string path, Transform parent = null)
    {
        var go = Managers.Resource.Instantiate(path, parent);

        switch (type)
        {
            case GameObjectType.Tower:
                if (OnSpawnEvent != null) 
                {
                    OnSpawnEvent.Invoke(1);
                }                
                break;
        }

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
