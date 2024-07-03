using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SnowBombController : BombController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SnowBomb;
        AttackAnimValue = 5 / 6f;
    }
}
