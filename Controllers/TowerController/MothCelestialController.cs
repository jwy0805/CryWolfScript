using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class MothCelestialController : MothMoonController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.MothCelestial;
    }
}
