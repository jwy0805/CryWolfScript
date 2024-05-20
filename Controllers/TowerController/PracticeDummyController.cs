using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf.Protocol;

public class PracticeDummyController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.PracticeDummy;
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
