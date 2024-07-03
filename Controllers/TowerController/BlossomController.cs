using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class BlossomController : BloomController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Blossom;
        AttackAnimValue = 5 / 6f;
    }
}
