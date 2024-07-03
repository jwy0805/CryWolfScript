using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Google.Protobuf.Protocol;

public class SoulController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Soul;
        AttackAnimValue = 5 / 6f;
    }
}
