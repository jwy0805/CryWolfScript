using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SnakeletController : MonsterController
{
    protected ProjectileId CurrentAttack;

    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Snakelet;
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
