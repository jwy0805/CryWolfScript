using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Google.Protobuf.Protocol;

public class TargetDummyController : PracticeDummyController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.TargetDummy;
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
}
