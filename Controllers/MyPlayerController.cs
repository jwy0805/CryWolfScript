using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Protobuf.Protocol;
using ModestTree;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class MyPlayerController : PlayerController
{
    [SerializeField] private bool axisYInversion;

    private GameViewModel _gameVm;
    
    private int _resource;
    private bool _arrived;
    private float _lastSendTime;

    private float _lastClickTime;
    private const float DoubleClickThreshold = 0.35f;
    private Vector3 _lastClickPos;
    private bool _isDragging;
    private Vector3 _lastMousePos;
    private GameObject _cameraObject;

    
    [Inject]
    public void Construct(GameViewModel gameVm)
    {
        _gameVm = gameVm;
    }
    
    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Player;
        Managers.Input.MouseAction -= OnMouseEvent;
        Managers.Input.MouseAction += OnMouseEvent;
        _cameraObject = GameObject.FindWithTag("CameraFocus");

        // Test - Unit Spawn
        // C_Spawn spawnPacket = new()
        // {
        //     Type = GameObjectType.Monster,
        //     Num = (int)UnitId.Werewolf,
        //     PosInfo = new PositionInfo { State = State.Idle, PosX = -1, PosY = 6, PosZ = 14, Dir = 180 },
        //     Way = SpawnWay.North
        // };
        // Managers.Network.Send(spawnPacket);
        //
        // C_Spawn spawnPacket1 = new()
        // {
        //     Type = GameObjectType.Monster,
        //     Num = (int)UnitId.Werewolf,
        //     PosInfo = new PositionInfo { State = State.Idle, PosX = 2, PosY = 6, PosZ = 15, Dir = 180 },
        //     Way = SpawnWay.North
        // };
        // Managers.Network.Send(spawnPacket1);
        //
        // C_Spawn spawnPacket2 = new()
        // {
        //     Type = GameObjectType.Monster,
        //     Num = (int)UnitId.Wolf,
        //     PosInfo = new PositionInfo { State = State.Idle, PosX = 1, PosY = 6, PosZ = 13, Dir = 180 },
        //     Way = SpawnWay.North,
        // };
        // Managers.Network.Send(spawnPacket2);
        //
        // C_Spawn spawnPacket3 = new()
        // {
        //     Type = GameObjectType.Tower,
        //     Num = (int)UnitId.PracticeDummy,
        //     PosInfo = new PositionInfo { State = State.Idle, PosX = -1, PosY = 6, PosZ = -6, Dir = 180 },
        //     Way = SpawnWay.North,
        // };
        // Managers.Network.Send(spawnPacket3);
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
                if (raycastHit == false) { return; }
                var go = hit.collider.gameObject;
                HandleSingleOrDoubleClick(go);
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

                    if (Faction == Faction.Sheep)
                    {
                        switch (_cameraObject.transform.position.z)
                        {
                            case < -12.1f when moveZ < 0:
                            case > 12.1f when moveZ > 0:
                                return;
                            default:
                                _cameraObject.transform.Translate(0, 0, moveZ);
                                break;
                        }
                    }

                    if (Faction == Faction.Wolf)
                    {
                        switch (_cameraObject.transform.position.z)
                        {
                            case < -12.1f when moveZ > 0:
                            case > 12.1f when moveZ < 0:
                                return;
                            default:
                                _cameraObject.transform.Translate(0, 0, moveZ);
                                break;
                        }
                    }
                }
                break;
        }
    }
    
    private void HandleSingleOrDoubleClick(GameObject go)
    {
        var currentTime = Time.time;
        var timeSinceLastClick = currentTime - _lastClickTime;

        if (timeSinceLastClick <= DoubleClickThreshold && Vector3.Distance(_lastClickPos, Input.mousePosition) < 2f)
        {
            OnDoubleClick(go);
        }
        else
        {
            OnClick(go);
        }

        _lastClickTime = currentTime;
        _lastClickPos = Input.mousePosition;
    }

    private void OnClick(GameObject go)
    {
        var layer = go.layer;

        if (go.TryGetComponent(out UI_Skill _) == false)
        {
            Managers.UI.CloseUpgradePopup();
        }

        go.TryGetComponent(out CreatureController cc);
        switch (layer)
        {
            case var _ when layer == LayerMask.NameToLayer("Ground"):
                Managers.UI.CloseAllPopupUI();
                _gameVm.TurnOffSelectRing();
                break;
                    
            case var _ when layer == LayerMask.NameToLayer("Tower"):
                if (Faction == Faction.Sheep)
                {
                    AdjustUI<UnitControlWindow>(go, GameObjectType.Tower);
                }
                break;
                    
            case var _ when layer == LayerMask.NameToLayer("Monster"):
                if (Faction == Faction.Wolf)
                {
                    if (_gameVm.CapacityWindow is { ObjectType: GameObjectType.Monster })
                    {
                        _gameVm.TurnOnSelectRing(cc.Id);
                    }
                    else
                    {
                        var window = Managers.UI.ShowPopupUiInGame<UnitControlWindow>();
                        window.SelectedUnit = go;
                    }
                }                
                break;
            
            case var _ when layer == LayerMask.NameToLayer("MonsterStatue"):
                if (Faction == Faction.Wolf)
                {
                    AdjustUI<UnitControlWindow>(go, GameObjectType.MonsterStatue);
                }
                break;
            
            case var _ when layer == LayerMask.NameToLayer("Fence"):
                if (Faction == Faction.Sheep)
                {
                    AdjustUI<UnitControlWindow>(go, GameObjectType.Fence);
                }
                break;
            
            case var _ when layer == LayerMask.NameToLayer("Sheep"):
                if (Faction == Faction.Sheep)
                {
                    _gameVm.TurnOffSelectRing();
                    _gameVm.TurnOnSelectRing(cc.Id);
                    Managers.UI.ShowPopupUiInGame<BaseSkillWindow>();
                }
                break;
            
            case var _ when layer == LayerMask.NameToLayer("Player"):
            case var _ when layer == LayerMask.NameToLayer("Base"):
                if (_isDragging) return;
                _gameVm.TurnOffSelectRing();
                // _gameVm.TurnOnSelectRing(cc.Id);
                Managers.UI.ShowPopupUiInGame<BaseSkillWindow>();
                break;
        }

        _isDragging = true;
        _lastMousePos = Input.mousePosition;
    }
    
    private void OnDoubleClick(GameObject go)
    {
        Managers.UI.CloseAllPopupUI();
        CapacityWindow window = null;
        
        if (Faction == Faction.Sheep)
        {
            switch (go.layer)
            {
                case var _ when go.layer == LayerMask.NameToLayer("Tower"):
                    window = Managers.UI.ShowPopupUiInGame<CapacityWindow>();
                    window.ObjectType = GameObjectType.Tower;
                    break;
                case var _ when go.layer == LayerMask.NameToLayer("Fence"):
                    window = Managers.UI.ShowPopupUiInGame<CapacityWindow>();
                    window.ObjectType = GameObjectType.Fence;
                    break;
            }
        }
        else if (Faction == Faction.Wolf)
        {
            switch (go.layer)
            {
                case var _ when go.layer == LayerMask.NameToLayer("MonsterStatue"):
                    window = Managers.UI.ShowPopupUiInGame<CapacityWindow>();
                    window.ObjectType = GameObjectType.MonsterStatue;
                    break;
            }
        }

        if (window != null && go.TryGetComponent(out CreatureController cc))
        {
            _gameVm.TurnOnSelectRing(cc.Id);
        }
    }

    private void AdjustUI<T>(GameObject go, GameObjectType type) where T : UI_Popup
    {
        var cc = go.GetComponent<CreatureController>();
        UI_Popup window;
        if (_gameVm.CapacityWindow != null && _gameVm.CapacityWindow.ObjectType == type)
        {
            if (_gameVm.SelectedObjectIds.Contains(cc.Id))
            {
                window = Managers.UI.ShowPopupUiInGame<T>();
                if (window is UnitControlWindow unitControlWindow)
                {
                    unitControlWindow.SelectedUnit = go;
                }
            }
            _gameVm.TurnOnSelectRing(cc.Id);
        }
        else
        {
            _gameVm.TurnOffSelectRing();
            _gameVm.TurnOnSelectRing(cc.Id);
            window = Managers.UI.ShowPopupUiInGame<T>();
            if (window is UnitControlWindow unitControlWindow)
            {
                unitControlWindow.SelectedUnit = go;
            }        
        }
    }
}