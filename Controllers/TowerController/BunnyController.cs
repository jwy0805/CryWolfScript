using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class BunnyController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Bunny;
        AttackAnimValue = 5 / 6f;
    }
}
