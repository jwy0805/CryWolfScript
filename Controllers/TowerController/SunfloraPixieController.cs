using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class SunfloraPixieController : SunflowerFairyController
{
    private ProjectileId _currentAttack;
    private readonly ProjectileId _originalAttack = ProjectileId.SunfloraPixieProjectile;
    private readonly ProjectileId _upgradedAttack = ProjectileId.SunfloraPixieFire;
    
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SunfloraPixie;
        _currentAttack = _originalAttack;
    }

    protected override void OnHitEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.ProjectileAttack,
            Effect = EffectId.None,
            Projectile = _currentAttack
        });
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
    
    public override void OnSkillUpdated(int id, GameObjectType type, SkillType skillType, int step)
    {
        if (id == (int)UnitId.SunfloraPixie && type == GameObjectType.Tower) _currentAttack = _upgradedAttack;
    }
}
