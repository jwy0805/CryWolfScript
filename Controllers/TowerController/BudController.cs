using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class BudController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Bud;
        AttackAnimValue = 5 / 6f;
    }
}
