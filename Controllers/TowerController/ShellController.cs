using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Google.Protobuf.Protocol;

public class ShellController : MonsterController
{
    private float _rollingSpeed;

    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Shell;
    }

    protected override void UpdateRush()
    {
        Vector3 dir = DestPos - transform.position;
        if (dir.magnitude < Tolerance || DestPos == Vector3.zero)
        {
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
            float moveDist = Mathf.Clamp((TotalMoveSpeed + 1.0f) * Time.deltaTime, 0, dir.magnitude);
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

    protected override void UpdateKnockBack()
    {
        if (SetKnockBackDest == false) return;
        
        Vector3 dir = DestPos - transform.position;
        float moveDist = Mathf.Clamp((TotalMoveSpeed) * Time.deltaTime, 0, dir.magnitude);
        transform.position += dir.normalized * moveDist;
        
        if (dir.magnitude < 0.1f)
        {
            State = State.Idle;
            SetKnockBackDest = false;
        }
        
        if (Time.time > LastSendTime + SendTick)
        {
            LastSendTime = Time.time;
            CellPos = transform.position;
            C_Move movePacket = new C_Move { ObjectId = Id, PosX = CellPos.x, PosY = CellPos.y, PosZ = CellPos.z};
            Managers.Network.Send(movePacket);
        }
    }

    protected override void UpdateSkill()
    {
        DestPos = transform.position;
        if (Time.time > LastSendTime + SendTick)
        {
            LastSendTime = Time.time;
            CellPos = transform.position;
            C_Move movePacket = new C_Move { ObjectId = Id, PosX = CellPos.x, PosY = CellPos.y, PosZ = CellPos.z};
            Managers.Network.Send(movePacket);
        }
    }

    protected override void OnSkillEvent()
    {
        Managers.Network.Send(new C_Skill
        {
            ObjectId = Id, 
            AttackMethod = AttackMethod.NoAttack, 
            Effect = EffectId.None,
            Projectile = ProjectileId.None
        });
    }
    
    public override void OnAnimSpeedUpdated(float param) { }
}
