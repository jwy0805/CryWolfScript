using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using Zenject;

public interface IPortrait
{
    UnitId UnitId { get; set; }
    bool CanSpawn { get; set; }
    Task EnsureRingsAsync(int token, float attackRange, float skillRange);
    void ShowSpawnableBounds(float minZ, float maxZ);
    void DestroyRing();
    void HideSpawnableBounds();
}

public class UI_Portrait : MonoBehaviour, IPortrait, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [CanBeNull] private TutorialViewModel _tutorialVm;
    private GameViewModel _gameVm;

    private Dictionary<int, Contents.UnitData> _data = new();
    private RectTransform _rect;
    private UnitId _unitId;
    private bool _canSpawn = true;
    private Vector3 _originalPos;
    private Vector3 _fencePos;
    private string _name;
    private Vector3 _hitPoint;
    private GameObject _attackRangeRing;
    private GameObject _skillRangeRing;
    private GameObject _spawnableBounds;
    private Image _image;
    
    // Network
    private readonly float _fenceMoveValue = 4f;
    private const float MinWorldDelta = 0.05f;
    private Vector3 _lastSentPos;
    
    // Raycast / Overlap NonAlloc
    private readonly RaycastHit[] _hits = new RaycastHit[1];
    private readonly Collider[] _overlaps = new Collider[8];
    private bool _lastHitValid;
    
    // Caching
    private Vector2 _pointerPos;
    private Camera _camera;
    private Canvas _rootCanvas;
    private int _rayMask;
    private int _denyMask;
    private bool _canSpawnLocal;
    private const float MinZ = -20;
    private const float MaxZ = 20;

    private int _ringToken;
    private bool _loadingAttackRing;
    private bool _loadingSkillRing;
    
    public UnitId UnitId
    {
        get => _unitId;
        set
        {
            _unitId = value;
            var starPanel = transform.Find("StarPanel");
            var level = _gameVm.GetLevelFromUiObject(_unitId);
            if (starPanel == null) return;
            for (var i = 0; i < starPanel.childCount; i++)
            {
                var star = starPanel.GetChild(i);
                star.GetComponent<Image>().color = i < level ? Color.white : Color.black;
            }
        }
    }

    public bool CanSpawn
    {
        get => _canSpawn;
        set
        {
            if (_canSpawn == value) return;
            _canSpawn = value;
            _image.color = _canSpawn ? Color.white : Color.red;

            if (_canSpawn)
            {
                if (_attackRangeRing != null) _attackRangeRing.SetActive(true);
                if (_skillRangeRing != null) _skillRangeRing.SetActive(true);
                
                if ((_attackRangeRing == null || _skillRangeRing == null) &&
                    _data.TryGetValue((int)_unitId, out var data) && data != null)
                {
                    var token = ++_ringToken;
                    _ = EnsureRingsAsync(token, data.Stat.AttackRange, data.Stat.SkillRange);
                }
            }
            else
            {
                if (_attackRangeRing != null) _attackRangeRing.SetActive(false);
                if (_skillRangeRing != null) _skillRangeRing.SetActive(false);
                
                _ringToken++;
            }
        }
    }
    
    [Inject]
    public void Construct(GameViewModel gameViewModel, TutorialViewModel tutorialViewModel)
    {
        _gameVm = gameViewModel;
        _tutorialVm = tutorialViewModel;
    }

    private void Awake()
    {
        _data = Managers.Data.UnitDict;
        _camera = Camera.main;
        _rect = GetComponent<RectTransform>();
        _rayMask = LayerMask.GetMask("Ground", "Fence", "MonsterStatue", "Base", "Sheep");
        _denyMask = LayerMask.GetMask("Fence", "MonsterStatue", "Base", "Sheep");
        _image = GetComponent<Image>();

        var fence = Managers.Object.Find(go => go.CompareTag("Fence"));
        if (fence != null)
        {
            _fencePos = fence.transform.position;
        }

        if (_rootCanvas == null)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null) _rootCanvas = canvas.rootCanvas;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Managers.UI.CloseAllPopupUI();
        var tf = transform;
        var bounce = GetComponent<ButtonBounce>();
        if (bounce == null)
        {
            Debug.LogError("ButtonBounce component is missing on the portrait.");
            return;
        }
        bounce.Selected = false;
        
        // Drag start
        if (gameObject.TryGetComponent(out UI_Portrait portrait) == false)
        {
            Debug.Log("UI_Portrait is null");
            return;
        }
        
        _gameVm.CurrentSelectedPortrait = portrait;
        _gameVm.OnPortraitDrag = true;
        _originalPos = tf.position;
        _lastSentPos = Vector3.positiveInfinity;
        
        // spawnable bounds
        Managers.Network.Send(new C_GetSpawnableBounds { Faction = Util.Faction });
        // Tutorial
        _tutorialVm?.PortraitDragStartHandler();
    }

    public void OnDrag(PointerEventData eventData)
    {
        AdjustSpawnableBounds();
        _pointerPos = eventData.position;

        if (_rootCanvas == null)
        {
            Debug.LogError("rootCanvas is null. Please check if the portrait is under a Canvas.");
            return;
        }
        
        var parentRect = (RectTransform)_rect.parent;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, _rootCanvas ? _rootCanvas.worldCamera : null, out var local);
        _rect.anchoredPosition = local;

        var ray = _camera.ScreenPointToRay(_pointerPos);
        int hitCount = Physics.RaycastNonAlloc(ray, _hits, 1000f, _rayMask, QueryTriggerInteraction.Collide);
        _lastHitValid = hitCount > 0;
        if (!_lastHitValid)
        {
            CanSpawn = false;
            return;
        }
        
        RaycastHit closestHit = _hits[0];
        for (int i = 1; i < hitCount; i++)
        {
            if (_hits[i].distance < closestHit.distance)
            {
                closestHit = _hits[i];
            }
        }
            
        _hitPoint = closestHit.point;
        var hitLayer = closestHit.collider.gameObject.layer;
        
        if (_attackRangeRing != null)
        {
            _attackRangeRing.transform.position = _hitPoint + new Vector3(0, 0.05f, 0);
        }

        if (_skillRangeRing != null)
        {
            _skillRangeRing.transform.position = _hitPoint + new Vector3(0, 0.05f, 0);
        }
        
        if (hitLayer != LayerMask.NameToLayer("Ground"))
        {
            CanSpawn = false;
            return;
        }

        CanSpawn = LocalCanSpawn(_hitPoint, hitLayer);
    }
    
    private bool LocalCanSpawn(Vector3 point, int hitLayer)
    {
        if (point.z is < MinZ or > MaxZ) return false;
        
        // 레이캐스트가 맞은 레이어가 denyMask에 포함돼 있으면 소환 불가
        int hitLayerMask = 1 << hitLayer;
        if ((_denyMask & hitLayerMask) != 0)
            return false;
        
        // 주변에 Fence/Statue/Base 같은 금지 오브젝트가 겹쳐 있는지 한 번 더 체크
        Managers.Data.UnitDict.TryGetValue((int)_unitId, out var unitData);
        if (unitData == null) return false;
        var unitSize = unitData.Stat.SizeX * 0.25f;
        int nonAlloc = 
            Physics.OverlapSphereNonAlloc(point, unitSize, _overlaps, _denyMask, QueryTriggerInteraction.Collide);
        
        return nonAlloc <= 0;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        HideSpawnableBounds();
        DestroyRing();
        
        // Drag ended
        transform.position = _originalPos;
        GetComponent<Image>().color = Color.white;
        _gameVm.CancelClickedEffect();
        _gameVm.OnPortraitDrag = false;
        _gameVm.TurnOffSelectRing();
        
        if (CanSpawn == false || _lastHitValid == false) return;
        Managers.Game.Spawn(_gameVm.CurrentSelectedPortrait.UnitId, _hitPoint);
        
        // Tutorial
        _tutorialVm?.PortraitDragEndHandler();
    }

    public async Task EnsureRingsAsync(int token, float attackRange, float skillRange)
    {
        try
        {
            if (attackRange > 0 && _attackRangeRing == null && !_loadingAttackRing)
            {
                _loadingAttackRing = true;
                try
                {
                    var go = await Managers.Resource.Instantiate("WorldObjects/RangeRing");
                    
                    if (token != _ringToken) 
                    {
                        Managers.Resource.Destroy(go);
                        return;
                    }
                    
                    _attackRangeRing = go;
                    if (go.TryGetComponent(out UI_RangeRing ring))
                    {
                        ring.AboutAttack = true;
                        ring.SetScale(attackRange);
                    }

                    go.SetActive(_canSpawn);
                    go.transform.position = _hitPoint + new Vector3(0, 0.05f, 0);
                }
                finally
                {
                    _loadingAttackRing = false;
                }
            }

            if (skillRange > 0 && _skillRangeRing == null && !_loadingSkillRing)
            {
                _loadingSkillRing = true;
                try
                {
                    var go = await Managers.Resource.Instantiate("WorldObjects/RangeRing");
                    
                    if (token != _ringToken) 
                    {
                        Managers.Resource.Destroy(go);
                        return;
                    }

                    _skillRangeRing = go;
                    if (go.TryGetComponent(out UI_RangeRing ring))
                    {
                        ring.AboutSkill = true;
                        ring.SetScale(skillRange);
                    }
                    
                    go.SetActive(_canSpawn);
                    go.transform.position = _hitPoint + new Vector3(0, 0.05f, 0);
                }
                finally
                {
                    _loadingSkillRing = false;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    public void DestroyRing()
    {
        _ringToken++; // 생성 중 Task 무효화

        if (_attackRangeRing != null)
        {
            Managers.Resource.Destroy(_attackRangeRing);
            _attackRangeRing = null;
        }

        if (_skillRangeRing != null)
        {
            Managers.Resource.Destroy(_skillRangeRing);
            _skillRangeRing = null;
        }
    }
    
    public async void ShowSpawnableBounds(float minZ, float maxZ)
    {
        try
        {
            _spawnableBounds = await Managers.Resource.Instantiate("WorldObjects/SpawnableBounds");
            _spawnableBounds.transform.position = new Vector3(0, 6.01f, minZ + (maxZ - minZ) / 2);
            _spawnableBounds.transform.localScale = new Vector3(220, (maxZ - minZ) * 10);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private void AdjustSpawnableBounds()
    {
        var fence = Managers.Object.Find(go => go.CompareTag("Fence"));
        var fencePos = fence != null ? fence.transform.position : _fencePos;
        if (fencePos.z - _fencePos.z > 1f)
        {
            if (_spawnableBounds == null) return;
            var pos = _spawnableBounds.transform.position;
            _spawnableBounds.transform.position = new Vector3(pos.x, pos.y, pos.z + _fenceMoveValue);
            _fencePos = fencePos;
        }
    }

    public void HideSpawnableBounds()
    {
        Managers.Resource.Destroy(_spawnableBounds);
        _spawnableBounds = null;
    }   
}
