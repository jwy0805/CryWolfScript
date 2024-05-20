using Google.Protobuf.Protocol;
using UnityEngine;

public class ResourceController : BaseController
{
    public int yield;
    public bool moveFlag = false;
    private readonly float _moveSpeed = 11.0f;

    public override State State
    {
        get => PosInfo.State;
        set => PosInfo.State = value;
    }
    
    protected override void UpdateMoving()
    {
        if (moveFlag == false) return;
        Vector3 dir = DestPos - transform.position;
        if (dir.magnitude < 0.3f)
        {
            C_ChangeResource resourcePacket = new C_ChangeResource { Camp = Camp.Sheep, ObjectId = Id, Resource = yield };
            Managers.Network.Send(resourcePacket);
            Managers.Game.Despawn(gameObject);
        }
        else
        {
            float moveDist = Mathf.Clamp(_moveSpeed * Time.deltaTime, 0, dir.magnitude);
            transform.position += dir.normalized * moveDist;
        }
    }
}
