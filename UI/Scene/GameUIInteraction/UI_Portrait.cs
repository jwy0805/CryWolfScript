using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using C_UnitSpawnPos = Google.Protobuf.Protocol.C_UnitSpawnPos;

public interface IPortrait
{
    UnitId UnitId { get; set; }
    bool CanSpawn { get; set; }
    void ShowRing(float attackRange, float skillRange);
}

public class UI_Portrait : MonoBehaviour, IPortrait, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameViewModel _gameVm;
    
    private readonly float _sendTick = 0.15f;
    private bool _canSpawn = true;
    private Vector3 _originalPos;
    private float _lastSendTime;
    private string _name;
    private Vector3 _hitPoint;
    private GameObject _attackRangeRing;
    private GameObject _skillRangeRing;

    public UnitId UnitId { get; set; }

    public bool CanSpawn
    {
        get => _canSpawn;
        set
        {
            _canSpawn = value;
            gameObject.GetComponent<Image>().color = _canSpawn ? Color.white : Color.red;
        }
    }
    
    [Inject]
    public void Construct(GameViewModel gameViewModel)
    {
        _gameVm = gameViewModel;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Managers.UI.CloseAllPopupUI();
        var go = gameObject;
        var tf = transform;
        var bounce = GetComponent<ButtonBounce>();
        bounce.Selected = false;
        
        // Drag start
        _gameVm.CurrentSelectedPortrait = go.GetComponent<UI_Portrait>();
        _gameVm.OnPortraitDrag = true;
        _originalPos = tf.position;
        _lastSendTime = 0;
        
        // Send packet to show the range rings
        Managers.Network.Send(new C_GetRanges { UnitId = (int)_gameVm.CurrentSelectedPortrait.UnitId });
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        
        if (Camera.main != null)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            _hitPoint = Vector3.zero;
            if (Physics.Raycast(ray, out _, Mathf.Infinity, LayerMask.GetMask("UI"))) return;
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                _hitPoint = hit.point;
            }        
        }
        
        // Move range ring toward portrait's transform
        if (_attackRangeRing != null)
        {
            _attackRangeRing.transform.position = _hitPoint + new Vector3(0, 0.05f, 0);
        }

        if (_skillRangeRing != null)
        {
            _skillRangeRing.transform.position = _hitPoint + new Vector3(0, 0.05f, 0);
        }
        
        // Send packet about the position of the unit
        if (Time.time < _lastSendTime + _sendTick) return;
        _lastSendTime = Time.time;
        
        var unitId = _gameVm.CurrentSelectedPortrait.UnitId;
        var destVector = new DestVector {X = _hitPoint.x, Y = _hitPoint.y, Z = _hitPoint.z};
        Managers.Network.Send(new C_UnitSpawnPos { UnitId = (int)unitId, DestVector = destVector });
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Hide the range rings
        HideRing();
        
        // Drag ended
        transform.position = _originalPos;
        GetComponent<Image>().color = Color.white;
        _gameVm.CancelClickedEffect();
        _gameVm.OnPortraitDrag = false;
                
        if (CanSpawn == false) return;
        Managers.Game.Spawn(_gameVm.CurrentSelectedPortrait.UnitId, _hitPoint);
        _gameVm.TurnOffSelectRing();
    }
    
    public void ShowRing(float attackRange, float skillRange)
    {
        if (attackRange > 0)
        {
            _attackRangeRing = Managers.Resource.Instantiate("WorldObjects/RangeRing");
            if (_attackRangeRing.TryGetComponent(out UI_RangeRing ring)) ring.AboutAttack = true;
            _attackRangeRing.transform.position = _hitPoint;
            ring.SetScale(attackRange);
        }

        if (skillRange > 0)
        {
            _skillRangeRing = Managers.Resource.Instantiate("WorldObjects/RangeRing");
            if (_skillRangeRing.TryGetComponent(out UI_RangeRing ring)) ring.AboutSkill = true;
            _skillRangeRing.transform.position = _hitPoint;
            ring.SetScale(skillRange);
        }
    }
    
    private void HideRing()
    {
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
}
