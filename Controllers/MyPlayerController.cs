using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    private TutorialViewModel _tutorialVm;
    
    private int _resource;
    private bool _arrived;
    private float _lastSendTime;

    private float _lastClickTime;
    private const float DoubleClickThreshold = 0.3f;
    private const float CmFingerTolerance = 0.5f;
    private Vector3 _lastClickPos;
    private bool _isDragging;
    private Vector3 _lastMousePos;
    private GameObject _lastClickedObject;
    private GameObject _cameraObject;
    private bool _cameraFound;
    
    [Inject]
    public void Construct(GameViewModel gameViewModel, TutorialViewModel tutorialViewModel)
    {
        _gameVm = gameViewModel;
        _tutorialVm = tutorialViewModel;
    }
    
    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Player;
        Managers.Input.MouseAction += OnMouseEvent;
        _cameraObject = GameObject.FindWithTag("CameraFocus");

        // Test - Unit Spawn
        // TestUnitSpawn();
    }

    private void TestUnitSpawn()
    {
        C_Spawn spawnPacket1 = new()
        {
            Type = GameObjectType.MonsterStatue,
            Num = (int)UnitId.Werewolf,
            PosInfo = new PositionInfo { State = State.Idle, PosX = -3.5f, PosY = 6, PosZ = 12, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket1);
        
        C_Spawn spawnPacket2 = new()
        {
            Type = GameObjectType.MonsterStatue,
            Num = (int)UnitId.Horror,
            PosInfo = new PositionInfo { State = State.Idle, PosX = 0f, PosY = 6, PosZ = 12, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket2); 
        
        C_Spawn spawnPacket3 = new()
        {
            Type = GameObjectType.MonsterStatue,
            Num = (int)UnitId.Cactus,
            PosInfo = new PositionInfo { State = State.Idle, PosX = 4f, PosY = 6, PosZ = 12, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket3); 
        
        C_Spawn spawnPacket4 = new()
        {
            Type = GameObjectType.MonsterStatue,
            Num = (int)UnitId.MoleRat,
            PosInfo = new PositionInfo { State = State.Idle, PosX = 3f, PosY = 6, PosZ = 14, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket4); 
        
        C_Spawn spawnPacket5 = new()
        {
            Type = GameObjectType.MonsterStatue,
            Num = (int)UnitId.SnakeNaga,
            PosInfo = new PositionInfo { State = State.Idle, PosX = -2.5f, PosY = 6, PosZ = 14, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket5); 
        
        C_Spawn spawnPacket6 = new()
        {
            Type = GameObjectType.MonsterStatue,
            Num = (int)UnitId.SkeletonGiant,
            PosInfo = new PositionInfo { State = State.Idle, PosX = 0.5f, PosY = 6, PosZ = 16, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket6);
        
        C_Spawn spawnPacket7 = new()
        {
            Type = GameObjectType.MonsterStatue,
            Num = (int)UnitId.Snake,
            PosInfo = new PositionInfo { State = State.Idle, PosX = -1, PosY = 6, PosZ = 16, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket7);
        
        C_Spawn spawnPacket8 = new()
        {
            Type = GameObjectType.Tower,
            Num = (int)UnitId.Hermit,
            PosInfo = new PositionInfo { State = State.Idle, PosX = -3f, PosY = 6, PosZ = -6f, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket8);
        
        C_Spawn spawnPacket9 = new()
        {
            Type = GameObjectType.Tower,
            Num = (int)UnitId.Hermit,
            PosInfo = new PositionInfo { State = State.Idle, PosX = 3f, PosY = 6, PosZ = -6f, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket9);
        
        C_Spawn spawnPacket10 = new()
        {
            Type = GameObjectType.Tower,
            Num = (int)UnitId.TrainingDummy,
            PosInfo = new PositionInfo { State = State.Idle, PosX = 0, PosY = 6, PosZ = -6f, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket10);
        
        C_Spawn spawnPacket11 = new()
        {
            Type = GameObjectType.Tower,
            Num = (int)UnitId.FlowerPot,
            PosInfo = new PositionInfo { State = State.Idle, PosX = 0, PosY = 6, PosZ = -10f, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket11);
        
        C_Spawn spawnPacket12 = new()
        {
            Type = GameObjectType.Tower,
            Num = (int)UnitId.Blossom,
            PosInfo = new PositionInfo { State = State.Idle, PosX = -2, PosY = 6, PosZ = -10f, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket12);
        
        C_Spawn spawnPacket13 = new()
        {
            Type = GameObjectType.Tower,
            Num = (int)UnitId.Blossom,
            PosInfo = new PositionInfo { State = State.Idle, PosX = -4, PosY = 6, PosZ = -10f, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket13);
        
        C_Spawn spawnPacket14 = new()
        {
            Type = GameObjectType.Tower,
            Num = (int)UnitId.SoulMage,
            PosInfo = new PositionInfo { State = State.Idle, PosX = 2.5f, PosY = 6, PosZ = -10f, Dir = 180 },
            Way = SpawnWay.North
        };
        Managers.Network.Send(spawnPacket14);
    }
    
    private async void OnMouseEvent(Define.MouseEvent evt)
    {
        try
        {
            if (_cameraObject == null && _cameraFound == false)
            {
                _cameraObject = GameObject.FindWithTag("CameraFocus");
                if (_cameraObject != null)
                {
                    _cameraFound = true;
                }
                else
                {
                    Debug.LogWarning("CameraFocus object not found. Please ensure it exists in the scene.");
                    return;
                }
            }
            
            var ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
            bool raycastHit = Physics.Raycast(ray, out var hit, 100.0f);

            Debug.Log($"Mouse Event: {evt}, Raycast Hit: {raycastHit}, Position: {Input.mousePosition}");
            Debug.Log($"CameraObject Position: {_cameraObject.transform.position}");
            if (_cameraObject == null)
            {
                Debug.LogWarning("CameraObject is null. Please ensure it is assigned.");
                return;
            }

            if (_gameVm == null)
            {
                Debug.LogWarning("gameVm is null. Please ensure it is assigned.");
            }
            
            switch (evt)
            {
                case Define.MouseEvent.Click:
                    break;
                
                case Define.MouseEvent.PointerDown:
                    if (raycastHit == false || _gameVm.OnPortraitDrag) { return; }
                    var go = hit.collider.gameObject;
                    await HandleSingleOrDoubleClick(go);
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
                                    return;
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
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    private async Task HandleSingleOrDoubleClick(GameObject go)
    {
        var now = Time.unscaledTime;
        var delta = now - _lastClickTime;

        float dpi = Screen.dpi <= 0 ? 200 : Screen.dpi;
        float pxTolerance = CmFingerTolerance * dpi / 2.54f;
        bool isCloseEnough = Vector2.Distance(Input.mousePosition, _lastClickPos) < pxTolerance;
        bool isSameTarget = go == _lastClickedObject;
        
        if (delta <= DoubleClickThreshold && isCloseEnough && isSameTarget)
        {
            await OnDoubleClick(go);
        }
        else
        {
            await OnClick(go);
        }

        _lastClickTime = now;
        _lastClickPos = Input.mousePosition;
        _lastClickedObject = go;
    }

    private async Task OnClick(GameObject go)
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
                    await AdjustUI<UnitControlWindow>(go, GameObjectType.Tower);
                    _gameVm.SetPortraitFromFieldUnit(cc.UnitId);
                    
                    // Tutorial
                    if (_tutorialVm.Step == 9 && Faction == Faction.Sheep)
                    {
                        _tutorialVm.StepTutorialByClickingUI();
                    }
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
                        var window = await Managers.UI.ShowPopupUiInGame<UnitControlWindow>();
                        window.SelectedUnit = go;
                    }
                }                
                break;
            
            case var _ when layer == LayerMask.NameToLayer("MonsterStatue"):
                if (Faction == Faction.Wolf)
                {
                    await AdjustUI<UnitControlWindow>(go, GameObjectType.MonsterStatue);
                    _gameVm.SetPortraitFromFieldUnit(cc.UnitId);
                    
                    // Tutorial
                    if (_tutorialVm.Step == 7 && Faction == Faction.Wolf)
                    {
                        _tutorialVm.StepTutorialByClickingUI();
                    }
                }
                break;
            
            case var _ when layer == LayerMask.NameToLayer("Fence"):
                if (Faction == Faction.Sheep)
                {
                    await AdjustUI<UnitControlWindow>(go, GameObjectType.Fence);
                    
                    // Tutorial
                    if (_tutorialVm.Step == 13 && Faction == Faction.Sheep)
                    {
                        _tutorialVm.StepTutorialByClickingUI();
                    }
                }
                break;
            
            case var _ when layer == LayerMask.NameToLayer("Sheep"):
                if (Faction == Faction.Sheep)
                {
                    _gameVm.TurnOffSelectRing();
                    _gameVm.TurnOnSelectRing(cc.Id);
                    await Managers.UI.ShowPopupUiInGame<BaseSkillWindow>();
                }
                break;
            
            case var _ when layer == LayerMask.NameToLayer("Player"):
            case var _ when layer == LayerMask.NameToLayer("Base"):
                if (_isDragging) return;
                _gameVm.TurnOffSelectRing();
                // _gameVm.TurnOnSelectRing(cc.Id);
                await Managers.UI.ShowPopupUiInGame<BaseSkillWindow>();
                break;
        }

        _isDragging = true;
        _lastMousePos = Input.mousePosition;
    }
    
    private async Task OnDoubleClick(GameObject go)
    {
        Managers.UI.CloseAllPopupUI();
        CapacityWindow window = null;
        
        if (Faction == Faction.Sheep)
        {
            switch (go.layer)
            {
                case var _ when go.layer == LayerMask.NameToLayer("Tower"):
                    window = await Managers.UI.ShowPopupUiInGame<CapacityWindow>();
                    window.ObjectType = GameObjectType.Tower;
                    break;
                case var _ when go.layer == LayerMask.NameToLayer("Fence"):
                    window = await Managers.UI.ShowPopupUiInGame<CapacityWindow>();
                    window.ObjectType = GameObjectType.Fence;
                    break;
            }
        }
        else if (Faction == Faction.Wolf)
        {
            switch (go.layer)
            {
                case var _ when go.layer == LayerMask.NameToLayer("MonsterStatue"):
                    window = await Managers.UI.ShowPopupUiInGame<CapacityWindow>();
                    window.ObjectType = GameObjectType.MonsterStatue;
                    break;
            }
        }

        if (window != null && go.TryGetComponent(out CreatureController cc))
        {
            _gameVm.TurnOnSelectRing(cc.Id);
        }
    }

    private async Task AdjustUI<T>(GameObject go, GameObjectType type) where T : UI_Popup
    {
        var cc = go.GetComponent<CreatureController>();
        UI_Popup window;
        if (_gameVm.CapacityWindow != null && _gameVm.CapacityWindow.ObjectType == type)
        {
            if (_gameVm.SelectedObjectIds.Contains(cc.Id))
            {
                window = await Managers.UI.ShowPopupUiInGame<T>();
                if (window is UnitControlWindow unitControlWindow)
                {
                    unitControlWindow.SelectedUnit = cc.gameObject;
                }
            }
            _gameVm.TurnOnSelectRing(cc.Id);
        }
        else
        {
            _gameVm.TurnOffSelectRing();
            _gameVm.TurnOnSelectRing(cc.Id);
            window = await Managers.UI.ShowPopupUiInGame<T>();
            if (window is UnitControlWindow unitControlWindow)
            {
                unitControlWindow.SelectedUnit = cc.gameObject;
            }        
        }
    }

    private void OnDestroy()
    {
        Managers.Input.MouseAction -= OnMouseEvent;
    }
}