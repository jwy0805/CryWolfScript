using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class CactusController : CactiController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Cactus;
        AttackAnimValue = 5 / 6f;
    }
}
