using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Google.Protobuf.Protocol;

public class HermitController : SpikeController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Hermit;
    }
}
