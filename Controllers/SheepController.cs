using Google.Protobuf.Protocol;
using UnityEngine;
using State = Google.Protobuf.Protocol.State;

public class SheepController : CreatureController
{
    protected override void Init()
    {
        base.Init();
        
        Managers.Network.Send(new C_SetDest { ObjectId = Id });
        ObjectType = GameObjectType.Sheep;
    }
    
    protected override void UpdateMoving()
    {
        if (PathQueue.Count <= 1) return;
        // Path 설정
        Vector3 cellPos = PathQueue.Peek();
        float step = TotalMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, cellPos, step);
        // Dir 설정
        double moveDir = DirQueue.Peek();
        Quaternion quaternion = Quaternion.Euler(0, (float)moveDir, step);
        transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, step);
        // Queue 정리
        if (Vector3.Distance(transform.position, cellPos) > 0.1f) return;
        DirQueue.Dequeue();
        PathQueue.Dequeue();
        // Vector3 dir = DestPos - transform.position;
        // if (dir.magnitude < Tolerance || DestPos == Vector3.zero)
        // {
        //     if (PathQueue == null || PathQueue.Count == 0) return;
        //     DestPos = PathQueue.Dequeue();
        //     if (DirQueue.Count != 0) MoveDir = DirQueue.Dequeue();
        // }
        // else
        // {
        //     float moveDist = Mathf.Clamp(Stat.MoveSpeed * Time.deltaTime, 0, dir.magnitude);
        //     transform.position += dir.normalized * moveDist;
        //     Quaternion quaternion = Quaternion.Euler(new Vector3(0, (float)MoveDir, 0));
        //     transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, 20 * Time.deltaTime);
        // }
        //
        // if (Time.time > LastSendTime + SendTick)
        // {
        //     LastSendTime = Time.time;
        //     CellPos = transform.position;
        //     Managers.Network.Send(new C_Move { ObjectId = Id, PosX = CellPos.x, PosY = CellPos.y, PosZ = CellPos.z});
        // }
    }
}