using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class WolfPupController : MonsterController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.WolfPup;
    }
}
