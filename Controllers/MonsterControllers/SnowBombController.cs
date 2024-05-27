using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SnowBombController : BombController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SnowBomb;
    }
    
    protected override void OnSkillEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.ProjectileAttack,
            Effect = EffectId.None,
            Projectile = ProjectileId.SnowBombSkill
        });
    }
    
    protected virtual void OnGoingToExplodeEvent()
    {
        Managers.Network.Send(new C_Motion
        {
            ObjectId = Id,
            State = State.GoingToExplode
        });
    }

    protected virtual void OnExplodeEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.EffectAttack,
            Effect = EffectId.SnowBombExplosion
        });
    }
}
