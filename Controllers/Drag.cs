using Google.Protobuf.Protocol;
using UnityEngine;

public class Drag : MonoBehaviour
{
    private RaycastHit _hitRay, _hitLayerMask;
    private GameObject _objectHitPosition;
    private UI_CanSpawn _canSpawn;
    private TowerController _towerController;
    private Vector3 _pos;
    // private readonly float _sendTick = 0.15f;
    // private float _lastSendTime;

    private readonly DestVector _destVec = new();
    public DestVector DestVec // Client에서 new 를 이용한 객체 생성 불가
    {
        get => _destVec;
        set
        {
            _destVec.X = value.X;
            _destVec.Y = value.Y;
            _destVec.Z = value.Z;
        }
    }
    
    private void Start()
    {
        _canSpawn = gameObject.GetComponent<UI_CanSpawn>();
        _towerController = gameObject.GetComponent<TowerController>();
    }

    private void OnMouseDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out _hitLayerMask, Mathf.Infinity, LayerMask.NameToLayer("Ground")))
        {
            var t = transform;
            var pos = t.position;
            float y = pos.y; /* 높이 저장 */
            pos = Util.NearestCell(new Vector3 (_hitLayerMask.point.x, y, _hitLayerMask.point.z));
            t.position = pos;
            DestVec.X = pos.x;
            DestVec.Y = pos.y;
            DestVec.Z = pos.z;
        }
    }

    private void OnMouseUp()
    {
        // if (_canSpawn.CanSpawn)
        // {
        //     _towerController.Active = true;
        // }
    }
}
