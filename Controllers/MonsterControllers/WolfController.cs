using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class WolfController : WolfPupController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Wolf;
    }
}
