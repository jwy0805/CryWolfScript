using System;
using System.Linq;
using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using C_UnitSpawnPos = Google.Protobuf.Protocol.C_UnitSpawnPos;
using Enum = System.Enum;

public class UI_DragPortrait : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Vector3 Position { get; set; }
    public bool CanSpawn { get; set; }
    public GameObjectType ObjectType { get; set; }
    private Vector3 _originalTransform;
    private UI_Mediator _mediator;
    private float _lastSendTime;
    private readonly float _sendTick = 0.15f;
    private string _name;
    private Vector3 _hitPoint;
    private GameObject _attackRangeRing;
    private GameObject _skillRangeRing;
    
    public void Start()
    {
        _mediator = GameObject.FindWithTag("UI").GetComponent<UI_Mediator>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var go = gameObject;
        var tf = transform;
        var bounce = GetComponent<ButtonBounce>();
        bounce.Selected = false;

        // Drag start
        Managers.Game.PickedButton = go;
        _originalTransform = tf.position;
        _lastSendTime = 0;
        _mediator.DraggingObject = go;
        _name = gameObject.GetComponent<Image>().sprite.name;
        
        // Send packet to show the range rings
        if (go.TryGetComponent(out UI_Portrait portrait))
        {
            Managers.Network.Send(new C_GetRanges { UnitId = (int)portrait.UnitId });
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        
        if (Camera.main != null)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            Position = Vector3.zero;
            if (Physics.Raycast(ray, out _, Mathf.Infinity, LayerMask.GetMask("UI"))) return;
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                Position = hit.point;
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

        var unitId = (UnitId)Enum.Parse(typeof(UnitId), _name);
        var destVector = new DestVector {X = Position.x, Y = Position.y, Z = Position.z};
        Managers.Network.Send(new C_UnitSpawnPos { UnitId = (int)unitId, DestVector = destVector });
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Hide the range rings
        HideRing();
        
        // Drag ended
        transform.position = _originalTransform;
        GetComponent<Image>().color = Color.white;
        _mediator.CurrentWindow = null;
        _mediator.DraggingObject = null;

        if (CanSpawn)
        {
            // var unitId = (UnitId)Enum.Parse(typeof(UnitId), _name);
            Managers.Game.Spawn(ObjectType);
            CanSpawn = false;
        }
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
