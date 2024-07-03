using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class PoisonBombController : SnowBombController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.PoisonBomb;
        AttackAnimValue = 2 / 3f;
    }
}
