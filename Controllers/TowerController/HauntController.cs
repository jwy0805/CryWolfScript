using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class HauntController : SoulController
{
    protected ProjectileId CurrentAttack;
    protected ProjectileId OriginalAttack;
    protected ProjectileId UpgradedAttack;
        
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Haunt;
        OriginalAttack = ProjectileId.HauntProjectile;
        UpgradedAttack = ProjectileId.HauntFire;
        CurrentAttack = OriginalAttack;
    }

    protected override void OnSkillEvent()
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
        if (id == (int)UnitId.Haunt && type == GameObjectType.Tower) CurrentAttack = UpgradedAttack;
    }
}
