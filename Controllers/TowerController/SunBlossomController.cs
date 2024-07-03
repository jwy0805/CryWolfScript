using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class SunBlossomController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SunBlossom;
    }

    protected override void OnEndEvent()
    {
        State = State.Idle;
    }
    
    public override void OnAnimSpeedUpdated(float param) { }
}
