using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SnakeController : SnakeletController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Snake;
        AttackAnimValue = 5 / 6f;
    }
}
