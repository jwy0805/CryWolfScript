using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SkeletonGiantController : SkeletonController
{
    protected override void Init()
    {
        base.Init();   
        UnitId = UnitId.SkeletonGiant;
    }
    
    protected override void OnHitEvent()
    {   // 일반 공격
        Managers.Network.Send(new C_Attack { ObjectId = Id, AttackMethod = AttackMethod.NormalAttack, });
    }

    protected override void OnSkillEvent()
    {   // 방어력 디버프 이펙트
        Managers.Network.Send(new C_Skill { ObjectId = Id });
    }
}
