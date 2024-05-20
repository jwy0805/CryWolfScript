using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class MosquitoBugController : MonsterController
{
    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Monster;
        UnitId = UnitId.MosquitoBug;
    }
    
    protected override void OnHitEvent()
    {
        Managers.Network.Send(new C_Attack 
            { ObjectId = Id, AttackMethod = AttackMethod.NormalAttack});
    }
}
