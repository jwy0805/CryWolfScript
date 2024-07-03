using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class MushroomController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Mushroom;
        AttackAnimValue = 5 / 6f;
    }
}
