using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class MosquitoBugController : MonsterController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.MosquitoBug;
        AttackAnimValue = 4 / 5f;
    }
}
