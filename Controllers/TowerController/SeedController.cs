using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SeedController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Seed;
        AttackAnimValue = 5 / 6f;
    }
}
