using UnityEngine;
using UnityEngine.AI;
using Google.Protobuf.Protocol;

public class MonsterController : CreatureController
{
    protected MyPlayerController PlayerController;

    
    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Monster;
        Managers.Network.Send(new C_SetDest { ObjectId = Id });
    }
    
    protected override void UpdateMoving()
    {
        Vector3 dir = DestPos - transform.position;
        if (dir.magnitude < Tolerance || DestPos == Vector3.zero)
        {
            if (DestQueue == null || DestQueue.Count == 0) return;
            DestPos = DestQueue.Dequeue();
            if (DirQueue == null) return;
            MoveDir = DirQueue.Dequeue();
        }
        else
        {   // 클라이언트 상에서 게임 내 위치 이동
            float moveDist = Mathf.Clamp(TotalMoveSpeed * Time.deltaTime, 0, dir.magnitude);
            transform.position += dir.normalized * moveDist;
            Quaternion quaternion = Quaternion.Euler(new Vector3(0, (float)MoveDir, 0));
            transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, 20 * Time.deltaTime);
        }
        
        if (Time.time > LastSendTime + SendTick)
        {   // 틱마다 위치 전송, 전송된 위치는 서버 CellPos에 반영
            LastSendTime = Time.time;
            CellPos = transform.position;
            C_Move movePacket = new C_Move { ObjectId = Id, PosX = CellPos.x, PosY = CellPos.y, PosZ = CellPos.z};
            Managers.Network.Send(movePacket);
        }
    }
}
