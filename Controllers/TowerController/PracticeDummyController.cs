using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf.Protocol;

public class PracticeDummyController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.PracticeDummy;
        AttackAnimValue = 5 / 6f;
    }
}
