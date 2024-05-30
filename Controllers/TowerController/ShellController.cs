using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Google.Protobuf.Protocol;

public class ShellController : MonsterController
{
    private float _rollingSpeed;

    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Shell;
    }

    protected override void UpdateRush()
    {
        
    }

    protected override void UpdateKnockBack()
    {
        
    }

    protected override void UpdateSkill()
    {
        
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
    
    public override void OnAnimSpeedUpdated(float param) { }
}
