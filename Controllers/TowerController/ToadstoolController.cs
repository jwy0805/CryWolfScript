using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class ToadstoolController : FungiController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Toadstool;
    }
}
