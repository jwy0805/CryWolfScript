using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SkeletonMageController : SkeletonGiantController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SkeletonMage;
    }

    protected override void OnHitEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.ProjectileAttack,
            Projectile = ProjectileId.SkeletonMageProjectile
        });
    }
}
