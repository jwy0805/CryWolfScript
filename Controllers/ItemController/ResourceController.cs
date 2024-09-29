using Google.Protobuf.Protocol;
using UnityEngine;

public class ResourceController : BaseController
{
    private float _waitTime = 1f;
    private readonly float _jumpHeight = 3f;
    private readonly float _jumpSpeed = 4f;
    private readonly float _moveSpeed = 8.0f;
    
    protected Vector3 InitPos;

    public override State State
    {
        get => PosInfo.State;
        set => PosInfo.State = value;
    }

    protected override void Init()
    {
        DestPos = Managers.Object.MyPlayer.transform.position;
        InitPos = transform.position;
    }

    protected override void Update()
    {
        if (_waitTime <= 0)
        {
            State = State.Moving;
        }
        else
        {
            Pop();
            _waitTime -= Time.deltaTime;
        }
        
        switch (State)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Moving:
                UpdateMoving();
                break;
        }
    }
    
    protected override void UpdateMoving()
    {
        Vector3 dir = DestPos - transform.position;
        if (dir.magnitude < 0.3f)
        {
            Managers.Game.Despawn(gameObject);
        }
        else
        {
            float moveDist = Mathf.Clamp(_moveSpeed * Time.deltaTime, 0, dir.magnitude);
            transform.position += dir.normalized * moveDist;
        }
    }

    protected virtual void Pop()
    {
        float y = InitPos.y + Mathf.PingPong(Time.time * _jumpSpeed, _jumpHeight);   
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}
