using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class MoleRatController : BurrowController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.MoleRat;
        AttackAnimValue = 2 / 3f;
    }
}