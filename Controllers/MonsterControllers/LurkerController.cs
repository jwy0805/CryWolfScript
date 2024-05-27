using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class LurkerController : MonsterController
{
    protected ProjectileId CurrentAttack;
    
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Lurker;
        CurrentAttack = ProjectileId.BasicProjectile;
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
}
