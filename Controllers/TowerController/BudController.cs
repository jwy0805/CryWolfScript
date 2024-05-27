using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;
using TMPro.Examples;

public class BudController : TowerController
{
    protected ProjectileId CurrentAttack;
    private bool _double = false;
    
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Bud;
        CurrentAttack = ProjectileId.SeedProjectile;
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

    private void OnDoubleEvent()
    {
        if (_double == false) return;
        OnSkillEvent();
    }

    public override void OnSkillUpdated(int id, GameObjectType type, SkillType skillType, int step)
    {
        if (id == (int)UnitId.Bud && type == GameObjectType.Tower) _double = true;
    }
}
