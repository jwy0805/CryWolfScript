using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Google.Protobuf.Protocol;

public class SoulController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Soul;
        Managers.Network.Send(new C_SetDest { ObjectId = Id });
    }

    protected override void UpdateMoving()
    {
        Vector3 dir = DestPos - transform.position;
        if (dir.magnitude < Tolerance || DestPos == Vector3.zero)
        {
            if (DestQueue == null) return;
            if (DestQueue.Count == 0)
            {
                State = State.Idle;
                Managers.Network.Send(new C_State
                {
                    ObjectId = Id,
                    State = State
                });
                return;
            }
            DestPos = DestQueue.Dequeue();
            MoveDir = DirQueue.Dequeue();
        }
        else
        {
            float moveDist = Mathf.Clamp(TotalMoveSpeed * Time.deltaTime, 0, dir.magnitude);
            transform.position += dir.normalized * moveDist;
            Quaternion quaternion = Quaternion.Euler(new Vector3(0, (float)MoveDir, 0));
            transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, 20 * Time.deltaTime);
        }
        
        if (Time.time > LastSendTime + SendTick)
        {
            LastSendTime = Time.time;
            CellPos = transform.position;
            C_Move movePacket = new C_Move { ObjectId = Id, PosX = CellPos.x, PosY = CellPos.y, PosZ = CellPos.z};
            Managers.Network.Send(movePacket);
        }
    }
}
