using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class DogBarkController : DogPupController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.DogBark;
        AttackAnimValue = 5 / 6f;
    }
}
