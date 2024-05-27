using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class SoulMageController : HauntController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SoulMage;
        CurrentAttack = ProjectileId.SoulMageProjectile;
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

    protected override void OnSkillEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.EffectAttack,
            Effect = EffectId.SoulMagePunch,
            Projectile = ProjectileId.None
        });
    }
}
