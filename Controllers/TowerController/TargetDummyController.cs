using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Google.Protobuf.Protocol;

public class TargetDummyController : PracticeDummyController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.TargetDummy;
    }
}
