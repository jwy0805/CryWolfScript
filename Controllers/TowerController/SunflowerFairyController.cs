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
        
    }

    protected override void OnEndEvent()
    {
        State = State.Idle;
    }
}
