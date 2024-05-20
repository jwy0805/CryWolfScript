using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class PoisonBombController : SnowBombController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.PoisonBomb;
    }
    
    protected override void OnSkillEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.ProjectileAttack,
            Effect = EffectId.None,
            Projectile = ProjectileId.PoisonBombSkill
        });
    }

    private void OnDoubleSkillEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.AdditionalProjectileAttack,
            Effect = EffectId.None,
            Projectile = ProjectileId.PoisonBombSkill
        });
    }
    
    protected override void OnHitEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.NormalAttack
        });
    }
    
    protected override void OnExplodeEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.EffectAttack,
            Effect = EffectId.PoisonBombExplosion
        });
    }
}
