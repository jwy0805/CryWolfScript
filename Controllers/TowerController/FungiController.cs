using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class FungiController : MushroomController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Fungi;
    }
}
