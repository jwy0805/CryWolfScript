using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using State = Google.Protobuf.Protocol.State;

public class TrainingDummyController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.TrainingDummy;
        AttackAnimValue = 7 / 6f;
    }
}
