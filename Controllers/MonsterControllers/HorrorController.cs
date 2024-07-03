using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Google.Protobuf.Protocol;

public class HorrorController : CreeperController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Horror;
    }
}
