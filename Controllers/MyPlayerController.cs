using System.Collections.Generic;
using System.IO;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class MyPlayerController : PlayerController
{
    private int _resource;
    private bool _arrived;
    private float _lastSendTime;
    private UI_Mediator _mediator;
    
    public Camp Camp { get; set; }
    public int SelectedUnitId { get; set; }

    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Player;
        Managers.Input.MouseAction -= OnMouseEvent;
        Managers.Input.MouseAction += OnMouseEvent;
        _mediator = GameObject.FindWithTag("UI").GetComponent<UI_Mediator>();

        // Test - Unit Spawn
        C_Spawn spawnPacket = new()
        {
            Type = GameObjectType.Monster,
            Num = (int)UnitId.Snake,
            PosInfo = new PositionInfo { State = State.Idle, PosX = 0, PosY = 6, PosZ = 18, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket);
        
        // C_Spawn spawnPacket1 = new()
        // {
        //     Type = GameObjectType.Monster,
        //     Num = (int)UnitId.Horror,
        //     PosInfo = new PositionInfo { State = State.Idle, PosX = 0, PosY = 6, PosZ = 22, Dir = 180 },
        //     Way = SpawnWay.North,
        // };
        // Managers.Network.Send(spawnPacket1);
        //
        // C_Spawn spawnPacket2 = new()
        // {
        //     Type = GameObjectType.Monster,
        //     Num = (int)UnitId.PoisonBomb,
        //     PosInfo = new PositionInfo { State = State.Idle, PosX = 1, PosY = 6, PosZ = 22, Dir = 180 },
        //     Way = SpawnWay.North,
        // };
        // Managers.Network.Send(spawnPacket2);
        
        // C_Spawn spawnPacket3 = new()
        // {
        //     Type = GameObjectType.Monster,
        //     Num = (int)UnitId.Wolf,
        //     PosInfo = new PositionInfo { State = State.Idle, PosX = 2.5f, PosY = 6, PosZ = 22, Dir = 180 },
        //     Way = SpawnWay.North,
        // };
        // Managers.Network.Send(spawnPacket3);
        
        // C_Spawn spawnPacket4 = new()
        // {
        //     Type = GameObjectType.Tower,
        //     Num = (int)UnitId.SoulMage,
        //     PosInfo = new PositionInfo { State = State.Idle, PosX = 0, PosY = 6, PosZ = 3.5f, Dir = 180 },
        //     Way = SpawnWay.North,
        // };
        // Managers.Network.Send(spawnPacket4);
        
        C_Spawn spawnPacket5 = new()
        {
            Type = GameObjectType.Tower,
            Num = (int)UnitId.Hermit,
            PosInfo = new PositionInfo { State = State.Idle, PosX = 1, PosY = 6, PosZ = 6f, Dir = 180 },
            Way = SpawnWay.North,
        };
        Managers.Network.Send(spawnPacket5);
    }

    private void OnMouseEvent(Define.MouseEvent evt)
    {
        OnMouseEvent_IdleRun(evt);
    }

    private void OnMouseEvent_IdleRun(Define.MouseEvent evt)
    {
        var ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
        bool raycastHit = Physics.Raycast(ray, out var hit, 100.0f);

        if (evt != Define.MouseEvent.PointerDown) return;
        if (!raycastHit) return;
        
        var go = hit.collider.gameObject;
        var layer = go.layer;

        if (go.TryGetComponent(out UI_Skill _) == false)
        {
            Managers.UI.CloseUpgradePopup();
        }

        switch (layer)
        {
            case var _ when layer == LayerMask.NameToLayer("Ground"):
                _mediator.InitState(go);
                break;
            case var _ when layer == LayerMask.NameToLayer("Tower"):
            case var _ when layer == LayerMask.NameToLayer("Monster"):
                ProcessUnitControlWindow(go, layer);
                break;
            case var _ when layer == LayerMask.NameToLayer("Sheep") || layer == LayerMask.NameToLayer("Fence"):
                if (Camp == Camp.Sheep) _mediator.CurrentWindow = _mediator.WindowDictionary["SubResourceWindow"];
                break;
        }
    }

    private void ProcessUnitControlWindow(GameObject go, int layer)
    {
        _mediator.CurrentSelectedUnit = go;
        _mediator.CurrentWindow = _mediator.WindowDictionary["UnitControlWindow"];
        if (go.TryGetComponent(out CreatureController creatureController))
        {
            SelectedUnitId = creatureController.Id;
        }
    }
}