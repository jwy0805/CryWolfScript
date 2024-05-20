using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Google.Protobuf.Protocol;

public class HorrorController : CreeperController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Horror;
        OriginalAttack = ProjectileId.SmallPoison;
        UpgradedAttack = ProjectileId.BigPoison;
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
        if (id == (int)UnitId.Horror && type == GameObjectType.Monster) CurrentAttack = UpgradedAttack;
    }
}
