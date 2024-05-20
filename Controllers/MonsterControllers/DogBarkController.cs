using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class DogBarkController : DogPupController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.DogBark;
    }
    
    protected override void UpdateSkill()
    {
        base.UpdateAttack();
    }

    protected override void OnSkillEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id, AttackMethod = AttackMethod.AdditionalAttack
        });
    }
}
