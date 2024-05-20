using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class MosquitoStingerController : MosquitoPesterController
{
    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Monster;
        UnitId = UnitId.MosquitoStinger;
    }
    
    protected override void OnSkillEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id, 
            AttackMethod = AttackMethod.ProjectileAttack, 
            Effect = EffectId.None,
            Projectile = ProjectileId.MosquitoStingerProjectile
        });
    }
}
