using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SnakeletController : MonsterController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Snakelet;
        AttackAnimValue = 5 / 6f;
    }
}
