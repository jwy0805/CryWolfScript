using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class RabbitController : BunnyController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Rabbit;
    }

    protected override void OnSkillEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.AdditionalProjectileAttack,
            
        });
    }
}
