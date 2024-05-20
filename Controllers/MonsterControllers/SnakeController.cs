using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SnakeController : SnakeletController
{
    protected ProjectileId OriginalAttack = ProjectileId.BasicProjectile;
    protected ProjectileId UpgradedAttack = ProjectileId.SmallFire;
    
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Snake;
        CurrentAttack = OriginalAttack;
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
        if (id == (int)UnitId.Snake && type == GameObjectType.Monster) CurrentAttack = UpgradedAttack;
    }
}
