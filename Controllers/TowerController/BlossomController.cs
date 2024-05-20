using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class BlossomController : BloomController
{
    private readonly ProjectileId _originalAttack = ProjectileId.BlossomSeed;
    private readonly ProjectileId _upgradedAttack = ProjectileId.BlossomProjectile;
    
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Blossom;
        CurrentAttack = _originalAttack;
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
        if (id == (int)UnitId.Blossom && type == GameObjectType.Tower) CurrentAttack = _upgradedAttack;
    }
}
