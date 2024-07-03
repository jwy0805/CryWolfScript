using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class SoulMageController : HauntController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SoulMage;
    }
}
