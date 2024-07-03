using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class FlowerPotController : SproutController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.FlowerPot;
    }
}
