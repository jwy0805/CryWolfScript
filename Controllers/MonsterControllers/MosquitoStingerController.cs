using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class MosquitoStingerController : MosquitoPesterController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.MosquitoStinger;
        AttackAnimValue = 5 / 6f;
    }
}
