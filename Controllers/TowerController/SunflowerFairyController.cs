using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class SunflowerFairyController : SunBlossomController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SunflowerFairy;
    }
    
    protected override  void OnSkillEvent()
    {
        Managers.Network.Send(new C_Skill
        {
            ObjectId = Id, 
            AttackMethod = AttackMethod.NoAttack, 
            Effect = EffectId.None,
            Projectile = ProjectileId.None
        });
    }

    protected override void OnEndEvent()
    {
        State = State.Idle;
    }
}
