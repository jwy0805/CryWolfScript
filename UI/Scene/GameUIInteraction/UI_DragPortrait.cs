using System;
using System.Linq;
using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using C_UnitSpawnPos = Google.Protobuf.Protocol.C_UnitSpawnPos;
using Enum = System.Enum;

public class UI_DragPortrait : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Vector3 position;
    public bool endDrag;
    private Vector3 _originalTransform;
    private UI_Mediator _mediator;
    private float _lastSendTime;
    private readonly float _sendTick = 0.15f;
    private string _name;
    
    public void Start()
    {
        _mediator = GameObject.FindWithTag("UI").GetComponent<UI_Mediator>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ButtonBounce bounce = GetComponent<ButtonBounce>();
        bounce.Selected = false;

        var go = gameObject;
        var tf = transform;

        Managers.Game.PickedButton = go;
        _originalTransform = tf.position;
        _lastSendTime = 0;
        _mediator.DraggingObject = go;
        _name = gameObject.GetComponent<Image>().sprite.name;
        endDrag = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
        position = Vector3.zero;
        if (Physics.Raycast(ray, out _, Mathf.Infinity, LayerMask.GetMask("UI"))) return;
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Ground"))) 
            position = hit.point;
        
        if (Time.time < _lastSendTime + _sendTick) return;
        _lastSendTime = Time.time;

        UnitId unitId = (UnitId)Enum.Parse(typeof(UnitId), _name);
        DestVector destVector = new DestVector {X = position.x, Y = position.y, Z = position.z};
        Managers.Network.Send(new C_UnitSpawnPos { UnitId = (int)unitId, DestVector = destVector });
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        endDrag = true;
        transform.position = _originalTransform;
        GetComponent<Image>().color = Color.white;
        _mediator.CurrentWindow = null;
        _mediator.DraggingObject = null;
        var unitId = (UnitId)Enum.Parse(typeof(UnitId), _name);
        Managers.Game.Spawn(unitId, new DestVector {X = position.x, Y = position.y, Z = position.z});
    }
}
