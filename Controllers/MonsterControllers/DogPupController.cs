using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class DogPupController : MonsterController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.DogPup;
        AttackAnimValue = 5 / 6f;
    }
}
