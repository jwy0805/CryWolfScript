using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Google.Protobuf.Protocol;

public class MothLunaController : TowerController
{
    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Tower;
        UnitId = UnitId.MothLuna;
    }
}
