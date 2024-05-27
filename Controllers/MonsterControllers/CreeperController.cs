using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class CreeperController : LurkerController
{
    protected ProjectileId OriginalAttack = ProjectileId.BasicProjectile;
    protected ProjectileId UpgradedAttack = ProjectileId.SmallPoison;

    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Creeper;
        CurrentAttack = OriginalAttack;
    }

    protected override void UpdateRush()
    {
        base.UpdateMoving();
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
            Managers.Network.Send(new C_State
            {
                ObjectId = Id,
                State = State
            });
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

    protected override void OnHitEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.ProjectileAttack,
            Effect = EffectId.None,
            Projectile = CurrentAttack
        });
    }

    public override void OnSkillUpdated(int id, GameObjectType type, SkillType skillType, int step)
    {
        if (id == (int)UnitId.Creeper && type == GameObjectType.Monster) CurrentAttack = UpgradedAttack;
    }
}
