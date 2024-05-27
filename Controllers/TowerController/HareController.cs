using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class HareController : RabbitController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Hare;
    }

    protected override void OnSkillEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            
        });
    }
}
