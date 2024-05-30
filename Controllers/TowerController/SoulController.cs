using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Google.Protobuf.Protocol;

public class SoulController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Soul;
        Managers.Network.Send(new C_SetDest { ObjectId = Id });
    }
}
