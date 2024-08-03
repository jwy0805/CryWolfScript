using System.Collections.Generic;
using System.IO;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    [SerializeField] private bool axisYInversion;

    private int _resource;
    private bool _arrived;
    private float _lastSendTime;
    private UI_Mediator _mediator;

    private bool _isDragging;
    private Vector3 _lastMousePos;
    private GameObject _cameraObject;

    public Camp Camp { get; set; }
    public int SelectedUnitId { get; set; }

    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Player;
        Managers.Input.MouseAction -= OnMouseEvent;
        Managers.Input.MouseAction += OnMouseEvent;
        _mediator = GameObject.FindWithTag("UI").GetComponent<UI_Mediator>();
        _cameraObject = GameObject.FindWithTag("CameraFocus");

        // Test - Unit Spawn
        // C_Spawn spawnPacket = new()
        // {
        //     Type = GameObjectType.Monster,
        //     Num = (int)UnitId.Creeper,
        //     PosInfo = new PositionInfo { State = State.Idle, PosX = 1, PosY = 8, PosZ = 16, Dir = 180 },
        //     Way = SpawnWay.North
        // };
        // Managers.Network.Send(spawnPacket);
        
        // C_Spawn spawnPacket1 = new()
        // {
        //     Type = GameObjectType.Monster,
        //     Num = (int)UnitId.WolfPup,
        //     PosInfo = new PositionInfo { State = State.Idle, PosX = -1, PosY = 6, PosZ = 16, Dir = 180 },
        //     Way = SpawnWay.North,
        // };
        // Managers.Network.Send(spawnPacket1);
        //
        // C_Spawn spawnPacket2 = new()
        // {
        //     Type = GameObjectType.Monster,
        //     Num = (int)UnitId.WolfPup,
        //     PosInfo = new PositionInfo { State = State.Idle, PosX = 3, PosY = 6, PosZ = 16, Dir = 180 },
        //     Way = SpawnWay.North,
        // };
        // Managers.Network.Send(spawnPacket2);
    }
    
    private void OnMouseEvent(Define.MouseEvent evt)
    {
        var ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
        bool raycastHit = Physics.Raycast(ray, out var hit, 100.0f);
        
        switch (evt)
        {
            case Define.MouseEvent.Click:
                break;
            
            case Define.MouseEvent.PointerDown:
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
                        ProcessUnitControlWindow(go);
                        break;
                    case var _ when layer == LayerMask.NameToLayer("Sheep") || layer == LayerMask.NameToLayer("Fence"):
                        if (Camp == Camp.Sheep)
                        {
                            _mediator.CurrentWindow = _mediator.WindowDictionary["SubResourceWindow"];
                        }
                        break;
                }

                _isDragging = true;
                _lastMousePos = Input.mousePosition;
                break;
            
            case Define.MouseEvent.PointerUp:
                _isDragging = false;
                break;
            
            case Define.MouseEvent.Press:
                if (_isDragging)
                {
                    var currentMousePos = Input.mousePosition;
                    var pressedY = _lastMousePos.y;
                    var direction = axisYInversion ? currentMousePos.y - pressedY : pressedY - currentMousePos.y;
                    var moveZ = direction * Time.deltaTime * 0.08f;

                    if (_cameraObject.transform.position.z < -12.1f && moveZ < 0) return;
                    if (_cameraObject.transform.position.z > 12.1f && moveZ > 0) return;
                    _cameraObject.transform.Translate(0, 0, moveZ);
                }
                break;
        }
    }

    private void ProcessUnitControlWindow(GameObject go)
    {
        _mediator.CurrentSelectedUnit = go;
        
        if (go.TryGetComponent(out CreatureController creatureController))
        {
            SelectedUnitId = creatureController.Id;
            
            // Control Window
            if ((Util.Camp == Camp.Sheep && creatureController.ObjectType == GameObjectType.Tower)
                || (Util.Camp == Camp.Wolf && creatureController.ObjectType == GameObjectType.Monster))
            {
                _mediator.CurrentWindow = _mediator.WindowDictionary["UnitControlWindow"];
            }
        }
    }
}