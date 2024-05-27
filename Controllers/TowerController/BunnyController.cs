using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class BunnyController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Bunny;
    }

    protected override void OnHitEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.ProjectileAttack,
            Projectile = ProjectileId.BasicProjectile2
        });
    }
}
